using Bobail.Application.Interfaces.Repositories;
using Bobail.Domain.Users;
using Bobail.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Bobail.Infrastructure.Persistance.Entities;
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
        var tokens = await _context.EmailVerificationTokens
            .Where(x => x.UserId == userId)
            .ToListAsync();

        if (tokens.Count == 0)
            return;

        _context.EmailVerificationTokens.RemoveRange(tokens);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid tokenId)
    {
        var entity = await _context.EmailVerificationTokens
            .FirstOrDefaultAsync(x => x.Id == tokenId);

        if (entity == null)
            return;

        _context.EmailVerificationTokens.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
