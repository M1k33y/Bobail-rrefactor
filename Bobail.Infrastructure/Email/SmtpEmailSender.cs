using Bobail.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace Bobail.Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var host = _configuration["Email:Smtp:Host"] ?? throw new InvalidOperationException("Email:Smtp:Host is missing.");
        var portValue = _configuration["Email:Smtp:Port"] ?? "587";
        var fromEmail = _configuration["Email:FromEmail"] ?? throw new InvalidOperationException("Email:FromEmail is missing.");
        var fromName = _configuration["Email:FromName"] ?? "Bobail";
        var username = _configuration["Email:Smtp:Username"];
        var password = _configuration["Email:Smtp:Password"];
        var enableSslValue = _configuration["Email:Smtp:EnableSsl"] ?? "true";

        if (!int.TryParse(portValue, out var port))
            throw new InvalidOperationException("Email:Smtp:Port must be a valid integer.");

        if (!bool.TryParse(enableSslValue, out var enableSsl))
            throw new InvalidOperationException("Email:Smtp:EnableSsl must be true or false.");

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl
        };

        if (!string.IsNullOrWhiteSpace(username))
        {
            client.Credentials = new NetworkCredential(username, password);
        }

        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        await client.SendMailAsync(message);
        _logger.LogInformation("Sent SMTP email to {Email} with subject {Subject}", toEmail, subject);
    }
}
