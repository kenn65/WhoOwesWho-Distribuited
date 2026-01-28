IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// --- DATABASE BACKUP FILES SEEDING CONTAINER ----------------------------------------------------
IResourceBuilder<ContainerResource> seedContainer = builder.AddContainer("sql-bak-seeder", "alpine")
    .WithContainerName("wow-sql-bak-seeder")
    .WithBindMount(@"\\wsl.localhost\Ubuntu\home\kenn\who-owes-who-backups", "/seed")
    .WithVolume("sqlserver-backup-data", "/backup")
    .WithEntrypoint("sh")
    .WithArgs("-c", @"
        echo 'Seeding .bak files...';
        cp -f /seed/*.bak /backup/ || echo 'No .bak files found in /seed';
        echo 'Fixing permissions so SQL can read the files...';
        chown -R 10001:0 /backup;
        echo 'Backup seed complete.';
    ")
    .WithLifetime(ContainerLifetime.Session);


// --- SQL SERVER ------------------------------------------------------------------------
IResourceBuilder<ParameterResource>? dbPassword = builder.AddParameter("dbPassword", true);
var sql = builder.AddSqlServer("sql", dbPassword, 1455)
    .WithContainerName("wow-sql-server")
    .WithDataVolume("sqlserver-data")
    .WithVolume("sqlserver-backup-data", "/backup")    // ? sql container MUST mount the volume
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("MSSQL_SA_PASSWORD", dbPassword)
    .WithLifetime(ContainerLifetime.Persistent);


// --- DATABASES RESTORE CONTAINER ----------------------------------------------------
var restoreContainer = builder
    .AddContainer("sql-restore", "mcr.microsoft.com/mssql-tools")
    .WithContainerName("wow-sql-restore")
    .WithReference(sql)
    .WithVolume("sqlserver-backup-data", "/backup")
    .WithEnvironment("MSSQL_SA_PASSWORD", builder.Configuration["Parameters:dbPassword"])
    .WithEntrypoint("/bin/sh")
    .WithArgs("-c", @"
    echo 'Fixing permissions...';
    chown -R 10001:0 /backup;
    echo 'Waiting for SQL Server...';
    until /opt/mssql-tools/bin/sqlcmd -S sql,1433 -U sa -P $MSSQL_SA_PASSWORD -Q 'SELECT 1' > /dev/null 2>&1
    do
        echo 'SQL not ready yet...'
        sleep 2
    done
    echo 'SQL is ready. Restoring databases...';
    /opt/mssql-tools/bin/sqlcmd -S sql,1433 -U sa -P $MSSQL_SA_PASSWORD -d master -Q \
     ""RESTORE DATABASE [WoW.Users] FROM DISK='/backup/WoW.Users.bak' WITH REPLACE, STATS=5"";
    /opt/mssql-tools/bin/sqlcmd -S sql,1433 -U sa -P $MSSQL_SA_PASSWORD -d master -Q \
     ""RESTORE DATABASE [WoW.Events] FROM DISK='/backup/WoW.Events.bak' WITH REPLACE, STATS=5"";
    /opt/mssql-tools/bin/sqlcmd -S sql,1433 -U sa -P $MSSQL_SA_PASSWORD -d master -Q \
     ""RESTORE DATABASE [WoW.Payments] FROM DISK='/backup/WoW.Payments.bak' WITH REPLACE, STATS=5"";
    echo 'SQL RESTORE COMPLETE';
    ".Replace("\r\n", "\n"))
        .WithLifetime(ContainerLifetime.Session);

// --- DATABASES BACKUP CONTAINER ----------------------------------------------------
var backupContainer = builder
    .AddContainer("sql-backup", "mcr.microsoft.com/mssql-tools")
    .WithContainerName("wow-sql-bak-bakup")
    .WithReference(sql)
    .WithVolume("sqlserver-backup-data", "/backup")
    .WithEnvironment("MSSQL_SA_PASSWORD", builder.Configuration["Parameters:dbPassword"])
    .WithEntrypoint("/bin/bash")
    .WithArgs("-c", @"
    echo 'SQL BACKUP SIDEcar started.';
    echo 'Waiting for SQL Server...';
        until /opt/mssql-tools/bin/sqlcmd -S sql,1433 -U sa -P $MSSQL_SA_PASSWORD -Q 'SELECT 1' > /dev/null 2>&1
        do
           sleep 10
        done
    while true
    do
        echo 'SQL Server ready...'
        sleep 300
        echo 'Running SQL BACKUP...';
        /opt/mssql-tools/bin/sqlcmd -S sql,1433 -U sa -P $MSSQL_SA_PASSWORD -d master -Q ""BACKUP DATABASE [WoW.Users]   TO DISK='/backup/WoW.Users.bak'   WITH INIT"";
        /opt/mssql-tools/bin/sqlcmd -S sql,1433 -U sa -P $MSSQL_SA_PASSWORD -d master -Q ""BACKUP DATABASE [WoW.Events]  TO DISK='/backup/WoW.Events.bak'  WITH INIT"";
        /opt/mssql-tools/bin/sqlcmd -S sql,1433 -U sa -P $MSSQL_SA_PASSWORD -d master -Q ""BACKUP DATABASE [WoW.Payments] TO DISK='/backup/WoW.Payments.bak' WITH INIT"";
        echo 'SQL BACKUP DONE. Sleeping 120 seconds...';
        sleep 300
    done
    ".Replace("\r\n", "\n"))
        .WithLifetime(ContainerLifetime.Persistent);



//--- AZURE SERVICE BUS EMULATOR ----------------------------------------------------
var serviceBus = builder
    .AddAzureServiceBus("sbemulatorns")
    .RunAsEmulator(c => c
        .WithContainerName("wow-service-bus-emulator")
        .WithLifetime(ContainerLifetime.Persistent));

serviceBus
    .AddServiceBusTopic("whooweswho-messaging-dispatch-request")
    .AddServiceBusSubscription("messaging");

serviceBus
    .AddServiceBusTopic("whooweswho-messaging-dispatch-succeeded")
    .AddServiceBusSubscription("messaging-observability-succeeded");

serviceBus
    .AddServiceBusTopic("whooweswho-messaging-dispatch-failed")
    .AddServiceBusSubscription("messaging-observability-failed");



// --- WHO OWES WHO MICROSERVICES ----------------------------------------------------

var authorizationService = builder.AddProject<Projects.WhoOwesWho_AuthorizationService>("authorizationservice")
     .WithReference(serviceBus)
     .WithReference(sql)
     .WaitFor(serviceBus)
     .WaitFor(sql);

var currencyService = builder.AddProject<Projects.WhoOwesWho_CurrencyService>("currencyservice")
     .WithReference(serviceBus)
     .WithReference(sql)
     .WaitFor(serviceBus)
     .WaitFor(sql);

var encryptionService = builder.AddProject<Projects.WhoOwesWho_EncryptionService>("encryptionservice")
     .WithReference(serviceBus)
     .WithReference(sql)
     .WaitFor(serviceBus)
     .WaitFor(sql);

var eventService = builder.AddProject<Projects.WhoOwesWho_EventService>("eventservice")
     .WithReference(serviceBus)
     .WithReference(sql)
     .WaitFor(serviceBus)
     .WaitFor(sql);

var messagingService = builder.AddProject<Projects.WhoOwesWho_MessagingService>("messagingservice")
     .WithReference(serviceBus)
     .WithReference(sql)
     .WaitFor(serviceBus)
     .WaitFor(sql);

var paymentService = builder.AddProject<Projects.WhoOwesWho_PaymentService>("paymentservice")
     .WithReference(serviceBus)
     .WithReference(sql)
     .WaitFor(serviceBus)
     .WaitFor(sql);

var userService = builder.AddProject<Projects.WhoOwesWho_UserService>("userservice")
     .WithReference(serviceBus)
     .WithReference(sql)
     .WaitFor(serviceBus)
     .WaitFor(sql);

//var frontend = builder.AddExecutable(
//    "frontend",
//    "npm",
//    @"D:\WhoOwesWhoAspire\WhoOwesWho.Next\whooweswho-app",
//    "run",
//    "dev"
//)
//.WithHttpEndpoint(targetPort: 3000).WaitFor(authorizationService)
//        .WaitFor(userService)
//        .WaitFor(currencyService)
//        .WaitFor(eventService)
//        .WaitFor(paymentService);


//var gateway = builder.AddProject<Projects.WhoOwesWho_Gateway>("gateway")
//        .WaitFor(frontend);

//authorizationService.WithReference(serviceBus).WithReference(sql);
//currencyService.WithReference(serviceBus).WithReference(sql);
//encryptionService.WithReference(serviceBus).WithReference(sql);
//eventService.WithReference(serviceBus).WithReference(sql);
//messagingService.WithReference(serviceBus).WithReference(sql);
//paymentService.WithReference(serviceBus).WithReference(sql);
//userService.WithReference(serviceBus).WithReference(sql);
//gateway.WithReference(serviceBus).WithReference(sql);

builder.Build().Run();
