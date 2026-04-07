using Bobail.Application.Interfaces.Repositories;
using Bobail.Domain.Users;
using Bobail.Infrastructure.Persistance.Entities;
using Bobail.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class SqlUserRepository : IUserRepository
{
    private readonly GameDbContext _context;

    public SqlUserRepository(GameDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(User user)
    {
        var entity = new UserEntity
        {
            Id = user.Id,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            Nickname = user.Nickname,
            IsActive = user.IsActive,
            IsEmailVerified = user.IsEmailVerified,
            EmailVerifiedAtUtc = user.EmailVerifiedAtUtc
        };

        _context.Users.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        var entity = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == userId);

        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var entity = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == email);

        return entity == null ? null : MapToDomain(entity);
    }

    public async Task UpdateAsync(User user)
    {
        var entity = await _context.Users.FirstOrDefaultAsync(x => x.Id == user.Id);

        if (entity == null)
            throw new InvalidOperationException($"User with id '{user.Id}' was not found.");

        entity.Email = user.Email;
        entity.PasswordHash = user.PasswordHash;
        entity.Role = user.Role;
        entity.CreatedAt = user.CreatedAt;
        entity.Nickname = user.Nickname;
        entity.IsActive = user.IsActive;
        entity.IsEmailVerified = user.IsEmailVerified;
        entity.EmailVerifiedAtUtc = user.EmailVerifiedAtUtc;

        await _context.SaveChangesAsync();
    }

    private static User MapToDomain(UserEntity entity)
    {
        return new User
        {
            Id = entity.Id,
            Email = entity.Email,
            PasswordHash = entity.PasswordHash,
            Role = entity.Role,
            CreatedAt = entity.CreatedAt,
            Nickname = entity.Nickname,
            IsActive = entity.IsActive,
            IsEmailVerified = entity.IsEmailVerified,
            EmailVerifiedAtUtc = entity.EmailVerifiedAtUtc
        };
    }
}
