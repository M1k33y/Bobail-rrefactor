using Bobail.Application.Interfaces.Repositories;
using Bobail.Domain.Users;
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
            CreatedAt = user.CreatedAt
        };

        _context.Users.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var entity = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == email);

        if (entity == null)
            return null;

        return new User
        {
            Id = entity.Id,
            Email = entity.Email,
            PasswordHash = entity.PasswordHash,
            Role = entity.Role,
            CreatedAt = entity.CreatedAt
        };
    }
}