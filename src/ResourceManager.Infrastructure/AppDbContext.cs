using Microsoft.EntityFrameworkCore;
using ResourceManager.Domain;

namespace ResourceManager.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<User> Users => Set<User>();
    public DbSet<BookingAudit> BookingAudits => Set<BookingAudit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Resource>(resource =>
        {
            resource.HasKey(r => r.Id);
            resource.Property(r => r.Name).HasMaxLength(120).IsRequired();
            resource.Property(r => r.Location).HasMaxLength(120).IsRequired();
            resource.Property(r => r.Type).HasConversion<string>().HasMaxLength(20);
            resource.HasIndex(r => r.Name).IsUnique();
        });

        modelBuilder.Entity<User>(user =>
        {
            user.HasKey(u => u.Id);
            user.Property(u => u.FullName).HasMaxLength(120).IsRequired();
            user.Property(u => u.Email).HasMaxLength(160).IsRequired();
            user.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
            user.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Booking>(booking =>
        {
            booking.HasKey(b => b.Id);
            booking.Property(b => b.Purpose).HasMaxLength(200).IsRequired();
            booking.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);

            booking.HasOne(b => b.Resource)
                .WithMany(r => r.Bookings)
                .HasForeignKey(b => b.ResourceId)
                .OnDelete(DeleteBehavior.Restrict);

            booking.HasOne(b => b.Requester)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            booking.HasIndex(b => new { b.ResourceId, b.StartsAt });
        });

        modelBuilder.Entity<BookingAudit>(audit =>
        {
            audit.HasKey(a => a.Id);
            audit.Property(a => a.Note).HasMaxLength(200).IsRequired();
            audit.Property(a => a.FromStatus).HasConversion<string>().HasMaxLength(20);
            audit.Property(a => a.ToStatus).HasConversion<string>().HasMaxLength(20);
            audit.HasIndex(a => a.BookingId);
        });
    }
}
