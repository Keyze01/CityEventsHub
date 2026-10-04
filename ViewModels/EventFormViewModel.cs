using System.ComponentModel.DataAnnotations;

namespace CityEventsHub.ViewModels;

/// <summary>
/// Shared input model for creating and editing events.
/// </summary>
public class EventFormViewModel
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(4000)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a category.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Event date is required.")]
    [DataType(DataType.Date)]
    [FutureOrTodayDate(ErrorMessage = "The event date cannot be in the past.")]
    public DateTime Date { get; set; } = DateTime.UtcNow.Date.AddDays(1);

    [Required(ErrorMessage = "Event time is required.")]
    [DataType(DataType.Time)]
    public TimeSpan Time { get; set; } = new TimeSpan(9, 0, 0);

    [Required(ErrorMessage = "Venue is required.")]
    [StringLength(200)]
    public string Venue { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a city.")]
    [Display(Name = "City")]
    public int CityId { get; set; }

    [Range(-90, 90)]
    public double? Latitude { get; set; }

    [Range(-180, 180)]
    public double? Longitude { get; set; }

    [Required(ErrorMessage = "Contact email is required.")]
    [EmailAddress]
    [StringLength(256)]
    [Display(Name = "Contact Email")]
    public string ContactEmail { get; set; } = string.Empty;

    [Phone]
    [StringLength(30)]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Registration Required")]
    public bool RegistrationRequired { get; set; }

    [Range(1, 1000000, ErrorMessage = "Maximum attendees must be at least 1.")]
    [Display(Name = "Maximum Attendees")]
    public int? MaxAttendees { get; set; }

    [Display(Name = "Event Image")]
    public IFormFile? ImageFile { get; set; }

    public string? ExistingImagePath { get; set; }
}

/// <summary>Validates that a date is today or in the future.</summary>
public class FutureOrTodayDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime date && date.Date < DateTime.UtcNow.Date)
        {
            return new ValidationResult(ErrorMessage ?? "The date cannot be in the past.");
        }
        return ValidationResult.Success;
    }
}
