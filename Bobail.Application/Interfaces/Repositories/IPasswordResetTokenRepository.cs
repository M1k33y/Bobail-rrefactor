using Bobail.Domain.Users;

namespace Bobail.Application.Interfaces.Repositories
{
    public interface IPasswordResetTokenRepository
    {
        Task AddAsync(PasswordResetToken token);
        Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash);
        Task DeleteByUserIdAsync(Guid userId);
        Task MarkAsUsedAsync(Guid tokenId);
    }
}
