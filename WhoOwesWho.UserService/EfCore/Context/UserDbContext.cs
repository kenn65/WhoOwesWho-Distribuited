using Microsoft.EntityFrameworkCore;
using WhoOwesWho.UserService.EfCore.DataModels;

namespace WhoOwesWho.UserService.EfCore.Context
{
    public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
    {
        public DbSet<Users> Users { get; set; }
        public DbSet<ForgotPassword> ForgotPasswords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ForgotPassword>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<Users>()
                .HasIndex(u => u.EmailAddress);

            //modelBuilder.Entity<ForgotPassword>()
            //  .HasOne<Users>()          
            //  .WithMany()               
            //  .HasForeignKey(ea => ea.UserId);
            }

    }
}
