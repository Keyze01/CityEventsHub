using Microsoft.AspNetCore.Identity.UI.Services;

namespace CityEventsHub.Services;

/// <summary>
/// Development email sender stub: no SMTP server is configured for this project, so
/// outgoing emails (registration confirmation, password reset) are written to the console
/// and to a local log file instead of actually being sent. Swap this out for a real
/// SMTP/SendGrid implementation of <see cref="IEmailSender"/> in production.
/// </summary>
public class EmailSender : IEmailSender
{
    private readonly ILogger<EmailSender> _logger;
    private readonly string _logDirectory;

    public EmailSender(ILogger<EmailSender> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _logDirectory = Path.Combine(environment.ContentRootPath, "EmailLogs");
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        Directory.CreateDirectory(_logDirectory);

        var fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmssfff}_{SanitizeFileName(email)}.html";
        var filePath = Path.Combine(_logDirectory, fileName);

        var content = $"""
            <!-- To: {email} -->
            <!-- Subject: {subject} -->
            <!-- Sent (stub): {DateTime.UtcNow:O} -->
            {htmlMessage}
            """;

        await File.WriteAllTextAsync(filePath, content);

        _logger.LogInformation(
            "Stub email sender: would send '{Subject}' to {Email}. Saved to {Path}",
            subject, email, filePath);
    }

    private static string SanitizeFileName(string value)
    {
        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalid, '_');
        }
        return value;
    }
}
