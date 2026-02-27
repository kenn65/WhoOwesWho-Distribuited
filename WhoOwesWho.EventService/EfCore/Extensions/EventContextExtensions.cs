using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using WhoOwesWho.EventService.EfCore.Context;


namespace WhoOwesWho.EventService.EfCore.Extensions
{
    public static class EventContextExtensions
    {
        public static async Task ConfigureDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EventDbContext>();
            await EnsureDatabaseAsync(dbContext);
            await RunMigrationAsync(dbContext);
        }

        public static async Task EnsureDatabaseAsync(EventDbContext dbContext)
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

        public static async Task RunMigrationAsync(EventDbContext dbContext)
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
