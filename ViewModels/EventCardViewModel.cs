namespace CityEventsHub.ViewModels;

/// <summary>
/// Lightweight projection of an Event used for cards on the homepage, browse list, and favorites.
/// </summary>
public class EventCardViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
    public string Venue { get; set; } = string.Empty;
    public string OrganizerName { get; set; } = string.Empty;
    public bool RegistrationRequired { get; set; }
    public int? MaxAttendees { get; set; }
    public int AttendeeCount { get; set; }
    public double? AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsCancelled { get; set; }

    public DateTime StartsAt => Date.Date + Time;
    public bool IsFull => MaxAttendees.HasValue && AttendeeCount >= MaxAttendees.Value;
}
