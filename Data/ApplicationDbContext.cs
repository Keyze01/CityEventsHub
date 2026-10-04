using CityEventsHub.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CityEventsHub.Data;

/// <summary>
/// EF Core database context for City Events Hub, built on top of ASP.NET Core Identity.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<RSVP> Rsvps => Set<RSVP>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<OrganizerProfile> OrganizerProfiles => Set<OrganizerProfile>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ---- Category ----
        builder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();

        // ---- City ----
        builder.Entity<City>()
            .HasIndex(c => new { c.Name, c.Country })
            .IsUnique();

        // ---- Event ----
        builder.Entity<Event>(entity =>
        {
            entity.Property(e => e.Date).HasColumnType("date");

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.City)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Organizer)
                .WithMany(u => u.OrganizedEvents)
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => e.IsApproved);
        });

        // ---- Favorite ----
        builder.Entity<Favorite>(entity =>
        {
            entity.HasIndex(f => new { f.UserId, f.EventId }).IsUnique();

            entity.HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.Event)
                .WithMany(e => e.Favorites)
                .HasForeignKey(f => f.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- RSVP ----
        builder.Entity<RSVP>(entity =>
        {
            entity.HasIndex(r => new { r.UserId, r.EventId }).IsUnique();

            entity.HasOne(r => r.User)
                .WithMany(u => u.Rsvps)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Event)
                .WithMany(e => e.Rsvps)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- Review ----
        builder.Entity<Review>(entity =>
        {
            entity.HasIndex(r => new { r.UserId, r.EventId }).IsUnique();

            entity.HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Event)
                .WithMany(e => e.Reviews)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- Notification ----
        builder.Entity<Notification>(entity =>
        {
            entity.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(n => n.RelatedEvent)
                .WithMany()
                .HasForeignKey(n => n.RelatedEventId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ---- OrganizerProfile ----
        builder.Entity<OrganizerProfile>(entity =>
        {
            entity.HasIndex(o => o.UserId).IsUnique();

            entity.HasOne(o => o.User)
                .WithOne(u => u.OrganizerProfile)
                .HasForeignKey<OrganizerProfile>(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
