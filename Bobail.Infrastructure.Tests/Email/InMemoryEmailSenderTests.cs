using Bobail.Infrastructure.Email;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Bobail.Infrastructure.Tests.Email;

public class InMemoryEmailSenderTests
{
    [Fact]
    public async Task SendAsync_Captures_Email_In_Outbox()
    {
        var sender = new InMemoryEmailSender(Mock.Of<ILogger<InMemoryEmailSender>>());

        await sender.SendAsync("player@mail.com", "Verify", "<p>Hello</p>");

        sender.Messages.Should().ContainSingle();
        sender.Messages[0].ToEmail.Should().Be("player@mail.com");
        sender.Messages[0].Subject.Should().Be("Verify");
        sender.Messages[0].HtmlBody.Should().Be("<p>Hello</p>");
        sender.Messages[0].SentAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
