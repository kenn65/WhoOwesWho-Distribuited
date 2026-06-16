

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// --- SQL SERVER ------------------------------------------------------------------------
IResourceBuilder<ParameterResource>? dbPassword = builder.AddParameter("dbPassword", true);
var sql = builder.AddSqlServer("sql", dbPassword, 1455)
    .WithContainerName("wow-sql-server")
    .WithDataVolume("sqlserver-data")
    .WithVolume("sqlserver-backup-data", "/backup")    // ? sql container MUST mount the volume
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("MSSQL_SA_PASSWORD", dbPassword)
    .WithLifetime(ContainerLifetime.Persistent);

var whooweswhoUsers = sql.AddDatabase("whooweswho-users");
var whooweswhoEvents = sql.AddDatabase("whooweswho-events");
var whooweswhoPayments = sql.AddDatabase("whooweswho-payments");

var cache = builder.AddRedis("redis-cache")
    .WithContainerName("wow-redis-cache")
    .WithDataVolume()
    .WithEnvironment("ALLOW_EMPTY_PASSWORD", "yes")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithRedisInsight();


//--- AZURE SERVICE BUS EMULATOR ----------------------------------------------------
var serviceBus = builder.AddConnectionString("servicebus");

// --- WHO OWES WHO MICROSERVICES ----------------------------------------------------

var authorizationService = builder.AddProject<Projects.WhoOwesWho_AuthorizationService>("authorizationservice")
     .WithUrlForEndpoint("https", url => url.Url = "/scalar/v1")
     .WithReference(serviceBus)
     .WithReference(sql)
     .WithReference(cache)
     .WaitFor(serviceBus)
     .WaitFor(sql);


var currencyService = builder.AddProject<Projects.WhoOwesWho_CurrencyService>("currencyservice")
     .WithUrlForEndpoint("https", url => url.Url = "/scalar/v1")
     .WithReference(serviceBus)
     .WithReference(sql)
     .WithReference(cache)
     .WaitFor(serviceBus)
     .WaitFor(sql);

var encryptionService = builder.AddProject<Projects.WhoOwesWho_EncryptionService>("encryptionservice")
     .WithUrlForEndpoint("https", url => url.Url = "/scalar/v1")
     .WithReference(serviceBus)
     .WithReference(sql)
     .WithReference(cache)
     .WaitFor(serviceBus)
     .WaitFor(sql);

var eventService = builder.AddProject<Projects.WhoOwesWho_EventService>("eventservice")
     .WithUrlForEndpoint("https", url => url.Url = "/scalar/v1")
     .WithReference(serviceBus)
     .WithReference(sql)
     .WithReference(cache)
     .WithReference(whooweswhoEvents)
     .WaitFor(serviceBus)
     .WaitFor(sql);

var messagingService = builder.AddProject<Projects.WhoOwesWho_MessagingService>("messagingservice")
     .WithUrlForEndpoint("https", url => url.Url = "/scalar/v1")
     .WithReference(serviceBus)
     .WithReference(sql)
     .WaitFor(serviceBus)
     .WaitFor(sql);

var paymentService = builder.AddProject<Projects.WhoOwesWho_PaymentService>("paymentservice")
     .WithUrlForEndpoint("https", url => url.Url = "/scalar/v1")
     .WithReference(serviceBus)
     .WithReference(sql)
     .WithReference(cache)
     .WithReference(whooweswhoPayments)
     .WaitFor(serviceBus)
     .WaitFor(sql);

var userService = builder.AddProject<Projects.WhoOwesWho_UserService>("userservice")
     .WithUrlForEndpoint("https", url => url.Url = "/scalar/v1")
     .WithReference(serviceBus)
     .WithReference(sql)
     .WithReference(cache)
     .WithReference(whooweswhoUsers)
     .WaitFor(serviceBus)
     .WaitFor(sql);

builder.AddProject<Projects.WhoOwesWho_WebApp>("whooweswho-webapp")
    .WithReference(authorizationService)
    .WaitFor(authorizationService)
    .WithReference(currencyService)
    .WaitFor(currencyService)
    .WithReference(encryptionService)
    .WaitFor(encryptionService)
    .WithReference(eventService)
    .WaitFor(eventService)
    .WithReference(messagingService)
    .WaitFor(messagingService)
    .WithReference(paymentService)
    .WaitFor(paymentService)
    .WithReference(userService)
    .WaitFor(userService);

builder.Build().Run();
