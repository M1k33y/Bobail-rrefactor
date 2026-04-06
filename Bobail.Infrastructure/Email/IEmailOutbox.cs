namespace Bobail.Infrastructure.Email;

public interface IEmailOutbox
{
    IReadOnlyList<SentEmailMessage> Messages { get; }
}
