using Bobail.Application.Interfaces.Repositories;
using Bobail.Domain.Users;
using Bobail.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Bobail.Infrastructure.Persistance.Entities;

namespace Bobail.Infrastructure.Persistance.Repositories;

public class SqlEmailVerificationTokenRepository : IEmailVerificationTokenRepository
{
    private readonly GameDbContext _context;

    public SqlEmailVerificationTokenRepository(GameDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(EmailVerificationToken token)
    {
        var entity = new EmailVerificationTokenEntity
        {
            Id = token.Id,
            UserId = token.UserId,
            TokenHash = token.TokenHash,
            ExpiresAtUtc = token.ExpiresAtUtc,
            CreatedAtUtc = token.CreatedAtUtc
        };

        _context.EmailVerificationTokens.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<EmailVerificationToken?> GetByTokenHashAsync(string tokenHash)
    {
        var entity = await _context.EmailVerificationTokens
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

        if (entity == null)
            return null;

        return new EmailVerificationToken
        {
            Id = entity.Id,
            UserId = entity.UserId,
            TokenHash = entity.TokenHash,
            ExpiresAtUtc = entity.ExpiresAtUtc,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }

    public async Task DeleteByUserIdAsync(Guid userId)
    {
        await _context.EmailVerificationTokens
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync();
    }

    public async Task DeleteAsync(Guid tokenId)
    {
        await _context.EmailVerificationTokens
            .Where(x => x.Id == tokenId)
            .ExecuteDeleteAsync();
    }
}
