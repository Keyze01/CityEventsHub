using Microsoft.AspNetCore.Identity;

namespace CityEventsHub.Models;

/// <summary>
/// Application user, extending the default Identity user with profile fields
/// used across the dashboard, event organizing, and review features.
/// </summary>
public class ApplicationUser : IdentityUser
{
    [PersonalData]
    public string FullName { get; set; } = string.Empty;

    public string? ProfileImagePath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<RSVP> Rsvps { get; set; } = new List<RSVP>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public OrganizerProfile? OrganizerProfile { get; set; }
}
