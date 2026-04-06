using Bobail.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Bobail.Infrastructure.Email;

public class InMemoryEmailSender : IEmailSender, IEmailOutbox
{
    private readonly List<SentEmailMessage> _messages = [];
    private readonly ILogger<InMemoryEmailSender> _logger;

    public InMemoryEmailSender(ILogger<InMemoryEmailSender> logger)
    {
        _logger = logger;
    }

    public IReadOnlyList<SentEmailMessage> Messages => _messages;

    public Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var message = new SentEmailMessage
        {
            ToEmail = toEmail,
            Subject = subject,
            HtmlBody = htmlBody,
            SentAtUtc = DateTime.UtcNow
        };

        _messages.Add(message);
        _logger.LogInformation("Captured email to {Email} with subject {Subject}", toEmail, subject);

        return Task.CompletedTask;
    }
}
