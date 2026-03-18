using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WhoOwesWho.UserService.EfCore.Context;

namespace WhoOwesWho.UserServiceTests.Repositories.Context
{
    public static class DbContextFactory
    {
        public static UserDbContext CreateContext(out SqliteConnection connection)
        {
            connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new UserDbContext(options);

            context.Database.EnsureCreated();

            return context;
        }
    }
}
