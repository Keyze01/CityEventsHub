using System.ComponentModel.DataAnnotations;

namespace CityEventsHub.Models;

/// <summary>
/// Extra profile information for users acting as event organizers. One-to-one with ApplicationUser.
/// </summary>
public class OrganizerProfile
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    [Required(ErrorMessage = "Organization name is required.")]
    [StringLength(200)]
    public string OrganizationName { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Bio { get; set; }

    [Phone]
    [StringLength(30)]
    public string? ContactPhone { get; set; }

    public bool IsVerified { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
