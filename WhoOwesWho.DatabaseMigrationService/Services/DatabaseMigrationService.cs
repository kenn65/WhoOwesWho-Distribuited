using Microsoft.Data.SqlClient;
using WhoOwesWho.DatabaseMigrationService.Models;
using WhoOwesWho.DatabaseMigrationService.Services.Base;

namespace WhoOwesWho.DatabaseMigrationService.Services
{
    public interface IDatabaseMigrationService
    {
        Task<MigrationResponseModel> RestoreDatabases();
        Task<MigrationResponseModel> BackupDatabases();
    }

    public class DatabaseMigrationService(IConfiguration configuration)
        : ServiceBase(configuration), IDatabaseMigrationService
    {
        public async Task<MigrationResponseModel> RestoreDatabases()
        {
            try
            {
                await Task.Delay(2000);
                await using (var connection = new SqlConnection(AppSettings.DatabaseConnectionString))
                {
                    IList<string> queries = new List<string>
                    {
                        //"CREATE ROLE MigrationRole AUTHORIZATION dbo",
                        
                        "USE [WoW.Users]; ALTER DATABASE [WoW.Users] SET SINGLE_USER WITH ROLLBACK IMMEDIATE",
                        "USE [WoW.Users]; ALTER DATABASE [WoW.Users] SET MULTI_USER",
                        "USE master; DROP DATABASE [WoW.Users]",
                        "USE master; RESTORE DATABASE [WoW.Users] FROM  DISK = N'/var/opt/mssql/data/backup/WoW.Users.bak' WITH  FILE = 1,  MOVE N'WoW.Users' TO N'/var/opt/mssql/data/WoW.Users.mdf',  MOVE N'WoW.Users_log' TO N'/var/opt/mssql/data/WoW.Users_log.ldf',  NOUNLOAD,  STATS = 5",
                        "USE [WoW.Events]; ALTER DATABASE [WoW.Events] SET SINGLE_USER WITH ROLLBACK IMMEDIATE",
                        "USE [WoW.Events]; ALTER DATABASE [WoW.Events] SET MULTI_USER",
                        "USE master; DROP DATABASE [WoW.Events]",
                        "USE master; RESTORE DATABASE [WoW.Events] FROM  DISK = N'/var/opt/mssql/data/backup/WoW.Events.bak' WITH  FILE = 1,  MOVE N'WoW.Events' TO N'/var/opt/mssql/data/WoW.Events.mdf',  MOVE N'WoW.Events_log' TO N'/var/opt/mssql/data/WoW.Events_log.ldf',  NOUNLOAD,  STATS = 5",
                        "USE [WoW.Payments]; ALTER DATABASE [WoW.Payments] SET SINGLE_USER WITH ROLLBACK IMMEDIATE",
                        "USE [WoW.Payments]; ALTER DATABASE [WoW.Payments] SET MULTI_USER",
                        "USE master; DROP DATABASE [WoW.Payments]",
                        "USE master; RESTORE DATABASE [WoW.Payments] FROM  DISK = N'/var/opt/mssql/data/backup/WoW.Payments.bak' WITH  FILE = 1,  MOVE N'WoW.Payments' TO N'/var/opt/mssql/data/WoW.Payments.mdf',  MOVE N'WoW.Payments_log' TO N'/var/opt/mssql/data/WoW.Payments_log.ldf',  NOUNLOAD,  STATS = 5"
                    };

                    await connection.OpenAsync();
                    foreach (var query in queries)
                    {
                       var command = new SqlCommand(query, connection);
                        command.CommandType = System.Data.CommandType.Text;
                        await command.ExecuteNonQueryAsync();
                        await Task.Delay(2000);
                    }
                    await connection.CloseAsync();
                }

                return await Task.FromResult(new MigrationResponseModel
                {
                    Success = true
                });
            }
            catch (SqlException e)
            {
                return await Task.FromResult(new MigrationResponseModel
                {
                    Message = "A SqlException error occurred An error occurred while migrating the databases",
                    ExceptionMessage = e.Message
                });
            }
            catch (Exception e)
            {
                return await Task.FromResult(new MigrationResponseModel
                {
                    Message = "An unexpected error occurred An error occurred while migrating the databases",
                    ExceptionMessage = e.Message
                });
            }
        }

        public async Task<MigrationResponseModel> BackupDatabases()
        {
            try
            {
                await using (var connection = new SqlConnection(AppSettings.DatabaseConnectionString))
                {
                    IList<string> queries = new List<string>
                    {
                        "BACKUP DATABASE [WoW.Users] TO  DISK = N'/var/opt/mssql/data/backup/WoW.Users.bak' WITH NOFORMAT, NOINIT,  NAME = N'WoW.Users-Full Database Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10",
                        "BACKUP DATABASE [WoW.Events] TO  DISK = N'/var/opt/mssql/data/backup/WoW.Events.bak' WITH NOFORMAT, NOINIT,  NAME = N'WoW.Events-Full Database Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10",
                        "BACKUP DATABASE [WoW.Payments] TO  DISK = N'/var/opt/mssql/data/backup/WoW.Payments.bak' WITH NOFORMAT, NOINIT,  NAME = N'WoW.Payments-Full Database Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10"
                    };
                    foreach (var query in queries)
                    {
                        connection.Open();
                        var command = new SqlCommand(query, connection);
                        command.CommandType = System.Data.CommandType.Text;
                        await command.ExecuteNonQueryAsync();
                        await connection.CloseAsync();
                    }
                }

                return await Task.FromResult(new MigrationResponseModel
                {
                    Success = true
                });
            }
            catch (SqlException e)
            {
                return await Task.FromResult(new MigrationResponseModel
                {
                    Message = "A SqlException error occurred An error occurred while backing up the databases",
                    ExceptionMessage = e.Message
                });
            }
            catch (Exception e)
            {
                return await Task.FromResult(new MigrationResponseModel
                {
                    Message = "An unexpected error occurred An error occurred while backing up the databases",
                    ExceptionMessage = e.Message
                });
            }
        }
    }
}
