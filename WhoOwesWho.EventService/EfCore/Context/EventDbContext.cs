using Microsoft.EntityFrameworkCore;
using WhoOwesWho.EventService.EfCore.DataModels;

namespace WhoOwesWho.EventService.EfCore.Context
{
    public class EventDbContext(DbContextOptions<EventDbContext> options) : DbContext(options)
    {
        public DbSet<Events> Events { get; set; }

        public DbSet<EventAssignments> EventAssingments { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventAssignments>()
                .HasKey(ea => new { ea.EventId, ea.UserId });

           modelBuilder.Entity<EventAssignments>()
              .HasOne<Events>()          
              .WithMany()               
              .HasForeignKey(ea => ea.EventId);
        }
    }
}
