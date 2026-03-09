using Microsoft.EntityFrameworkCore;
using WhoOwesWho.PaymentService.EfCore.DataModels;

namespace WhoOwesWho.PaymentService.EfCore.Context
{
    public class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
    {
        public DbSet<Payments> Payments { get; set; }
        public DbSet<PaymentUsers> PaymentUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Payments>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payments>()
                .Property(p => p.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payments>()
                .Property(p => p.OriginalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PaymentUsers>()
               .HasKey(p => new { p.PaymentId, p.Created});
        }
    }
}
