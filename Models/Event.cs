using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CityEventsHub.Models;

/// <summary>
/// A city event, created by an organizer and subject to administrator approval
/// before it becomes publicly visible.
/// </summary>
public class Event
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(4000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [Required(ErrorMessage = "Event date is required.")]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "Event time is required.")]
    [DataType(DataType.Time)]
    public TimeSpan Time { get; set; }

    [Required(ErrorMessage = "Venue is required.")]
    [StringLength(200)]
    public string Venue { get; set; } = string.Empty;

    [Required]
    public int CityId { get; set; }
    public City? City { get; set; }

    [Range(-90, 90)]
    public double? Latitude { get; set; }

    [Range(-180, 180)]
    public double? Longitude { get; set; }

    [Required]
    public string OrganizerId { get; set; } = string.Empty;
    public ApplicationUser? Organizer { get; set; }

    [Required(ErrorMessage = "Contact email is required.")]
    [EmailAddress]
    [StringLength(256)]
    public string ContactEmail { get; set; } = string.Empty;

    [Phone]
    [StringLength(30)]
    public string? PhoneNumber { get; set; }

    public bool RegistrationRequired { get; set; }

    [Range(1, 1000000, ErrorMessage = "Maximum attendees must be at least 1.")]
    public int? MaxAttendees { get; set; }

    public string? ImagePath { get; set; }

    /// <summary>Every new event starts unapproved; only admins can flip this.</summary>
    public bool IsApproved { get; set; } = false;

    public bool IsRejected { get; set; } = false;

    [StringLength(1000)]
    public string? RejectionReason { get; set; }

    public bool IsCancelled { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<RSVP> Rsvps { get; set; } = new List<RSVP>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    /// <summary>Combined date + time, used for past/upcoming comparisons and review eligibility.</summary>
    [NotMapped]
    public DateTime StartsAt => Date.Date + Time;

    [NotMapped]
    public bool HasEnded => DateTime.UtcNow > StartsAt;

    [NotMapped]
    public bool IsPubliclyVisible => IsApproved && !IsRejected && !IsCancelled;
}
