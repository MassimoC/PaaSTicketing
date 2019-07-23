using Microsoft.EntityFrameworkCore;
using PaaS.Ticketing.ApiLib.Entities;

namespace PaaS.Ticketing.ApiLib.Context
{
    public class TicketingContext : DbContext
    {
        public DbSet<Concert> Concerts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ConcertUser> Orders { get; set; }

        public TicketingContext(DbContextOptions<TicketingContext> options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //http://www.entityframeworktutorial.net/code-first/configure-many-to-many-relationship-in-code-first.aspx

            modelBuilder.Entity<ConcertUser>()
                .HasKey(cu => new { cu.ConcertId, cu.UserId });
            modelBuilder.Entity<ConcertUser>()
                .HasOne(cu => cu.Concert)
                .WithMany(c => c.ConcertUser)
                .HasForeignKey(cu => cu.ConcertId);
            modelBuilder.Entity<ConcertUser>()
                .HasOne(cu => cu.User)
                .WithMany(u => u.ConcertUser)
                .HasForeignKey(cu => cu.UserId);

            modelBuilder.Entity<ConcertUser>()
                .Property(b => b.CreatedOn)
                .HasDefaultValueSql("getdate()");
            modelBuilder.Entity<ConcertUser>()
                .Property(b => b.DateRegistration)
                .HasDefaultValueSql("getdate()");


            // SEED

            base.OnModelCreating(modelBuilder);
        }
    }
}
