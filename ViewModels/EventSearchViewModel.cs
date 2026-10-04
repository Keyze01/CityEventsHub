namespace CityEventsHub.ViewModels;

/// <summary>
/// Search/filter criteria for the public event browse page. All fields are optional and combine with AND.
/// </summary>
public class EventSearchViewModel
{
    public string? Keyword { get; set; }
    public int? CategoryId { get; set; }
    public int? CityId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? OrganizerName { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 9;
}
