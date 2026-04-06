using Bobail.Domain.Users;

namespace Bobail.Application.Interfaces.Repositories
{
    public interface IEmailVerificationTokenRepository
    {
        Task AddAsync(EmailVerificationToken token);
        Task<EmailVerificationToken?> GetByTokenHashAsync(string tokenHash);
        Task DeleteByUserIdAsync(Guid userId);
        Task DeleteAsync(Guid tokenId);
    }
}
