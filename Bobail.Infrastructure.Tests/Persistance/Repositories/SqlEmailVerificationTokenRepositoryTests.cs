using Bobail.Domain.Users;
using Bobail.Infrastructure.Persistance.Entities;
using Bobail.Infrastructure.Persistance.Repositories;
using Bobail.Infrastructure.Tests.TestSupport;
using FluentAssertions;

namespace Bobail.Infrastructure.Tests.Persistance.Repositories;

public class SqlEmailVerificationTokenRepositoryTests
{
    [Fact]
    public async Task AddAsync_And_GetByTokenHashAsync_RoundTrip_Token()
    {
        using var db = InfrastructureTestDb.Create();
        var repository = new SqlEmailVerificationTokenRepository(db.Context);
        var userId = await AddUserAsync(db);
        var token = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = "token-hash",
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddHours(1)
        };

        await repository.AddAsync(token);
        var restored = await repository.GetByTokenHashAsync("token-hash");

        restored.Should().NotBeNull();
        restored!.Id.Should().Be(token.Id);
        restored.UserId.Should().Be(userId);
        restored.TokenHash.Should().Be("token-hash");
    }

    [Fact]
    public async Task DeleteByUserIdAsync_Removes_User_Tokens()
    {
        using var db = InfrastructureTestDb.Create();
        var repository = new SqlEmailVerificationTokenRepository(db.Context);
        var userId = await AddUserAsync(db);
        await repository.AddAsync(Token(userId, "one"));
        await repository.AddAsync(Token(userId, "two"));

        await repository.DeleteByUserIdAsync(userId);

        (await repository.GetByTokenHashAsync("one")).Should().BeNull();
        (await repository.GetByTokenHashAsync("two")).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_Removes_Single_Token()
    {
        using var db = InfrastructureTestDb.Create();
        var repository = new SqlEmailVerificationTokenRepository(db.Context);
        var userId = await AddUserAsync(db);
        var token = Token(userId, "delete-me");
        await repository.AddAsync(token);

        await repository.DeleteAsync(token.Id);

        (await repository.GetByTokenHashAsync("delete-me")).Should().BeNull();
    }

    private static EmailVerificationToken Token(Guid userId, string hash)
    {
        return new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = hash,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddHours(1)
        };
    }

    private static async Task<Guid> AddUserAsync(InfrastructureTestDb db)
    {
        var userId = Guid.NewGuid();
        db.Context.Users.Add(new UserEntity
        {
            Id = userId,
            Email = $"{userId:N}@mail.com",
            Nickname = $"user-{userId:N}",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        await db.Context.SaveChangesAsync();
        return userId;
    }
}
