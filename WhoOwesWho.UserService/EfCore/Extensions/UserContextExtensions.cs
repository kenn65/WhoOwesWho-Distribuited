using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using WhoOwesWho.UserService.EfCore.Context;

namespace WhoOwesWho.UserService.EfCore.Extensions
{
    public static class UserContextExtensions
    {
        public static async Task ConfigureDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            await EnsureDatabaseAsync(dbContext);
            await RunMigrationAsync(dbContext); 
        }

        public static async Task EnsureDatabaseAsync(UserDbContext dbContext)
        {
            var dbCreator = dbContext.GetService<IRelationalDatabaseCreator>();
            var strategy = dbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                if (!await dbCreator.ExistsAsync())
                {
                    await dbCreator.CreateAsync();
                }
            });
        }

        public static async Task RunMigrationAsync(UserDbContext dbContext)
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync();
                await dbContext.Database.MigrateAsync();
                await transaction.CommitAsync();
            }); 
        }
    }
}
