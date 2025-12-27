using Microsoft.EntityFrameworkCore;
using Raffle.Api.Models;

namespace Raffle.Api.Database
{
    public class RaffleDbContext : DbContext
    {
        public DbSet<RaffleDraw> RaffleDraws => Set<RaffleDraw>();
        public DbSet<RaffleMember> Members => Set<RaffleMember>();

        public RaffleDbContext(DbContextOptions<RaffleDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RaffleDraw>()
                .HasIndex(d => d.Name)
                .IsUnique();

            modelBuilder.Entity<RaffleMember>()
                .HasOne(m => m.RaffleDraw)
                .WithMany(r => r.RaffleMembers)
                .HasForeignKey(m => m.RaffleDrawId);

            modelBuilder.Entity<RaffleDraw>()
                .HasOne(r => r.WinnerMember)
                .WithMany()
                .HasForeignKey(r => r.WinnerMemberId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
