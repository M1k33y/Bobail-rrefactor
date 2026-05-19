using Bobail.Application.Interfaces.Repositories;
using Bobail.Domain.Users;
using Bobail.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Bobail.Infrastructure.Persistance.Entities;

namespace Bobail.Infrastructure.Persistance.Repositories;

public class SqlPasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly GameDbContext _context;

    public SqlPasswordResetTokenRepository(GameDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PasswordResetToken token)
    {
        var entity = new PasswordResetTokenEntity
        {
            Id = token.Id,
            UserId = token.UserId,
            TokenHash = token.TokenHash,
            ExpiresAtUtc = token.ExpiresAtUtc,
            Used = token.Used,
            CreatedAtUtc = token.CreatedAtUtc,
            UsedAtUtc = token.UsedAtUtc
        };

        _context.PasswordResetTokens.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash)
    {
        var entity = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

        if (entity == null)
            return null;

        return new PasswordResetToken
        {
            Id = entity.Id,
            UserId = entity.UserId,
            TokenHash = entity.TokenHash,
            ExpiresAtUtc = entity.ExpiresAtUtc,
            Used = entity.Used,
            CreatedAtUtc = entity.CreatedAtUtc,
            UsedAtUtc = entity.UsedAtUtc
        };
    }

    public async Task DeleteByUserIdAsync(Guid userId)
    {
        await _context.PasswordResetTokens
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync();
    }

    public async Task MarkAsUsedAsync(Guid tokenId)
    {
        var entity = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(x => x.Id == tokenId);

        if (entity == null)
            return;

        entity.Used = true;
        entity.UsedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
