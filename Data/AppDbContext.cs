using Microsoft.EntityFrameworkCore;
using SegmentationService.Models;

namespace SegmentationService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Segment> Segments { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(u => u.Segments)
                .WithMany(s => s.Users)
                .UsingEntity(j => j.ToTable("UserSegments"));


            modelBuilder.Entity<Segment>()
                .HasIndex(s => s.Name)
                .IsUnique();
        }
    }
}
