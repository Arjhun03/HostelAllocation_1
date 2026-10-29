using HostelAllocation.Models;
using Microsoft.EntityFrameworkCore;

namespace HostelAllocation.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Room> Rooms => Set<Room>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(room => room.Id);

            entity.HasIndex(room => room.RoomNumber)
                .IsUnique();

            entity.Property(room => room.RoomNumber)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(room => room.RoomType)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(room => room.MonthlyFee)
                .HasColumnType("decimal(18,2)");
        });
    }
}