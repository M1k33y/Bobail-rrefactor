namespace Bobail.Infrastructure.Email;

public class SentEmailMessage
{
    public string ToEmail { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string HtmlBody { get; set; } = string.Empty;

    public DateTime SentAtUtc { get; set; }
}
