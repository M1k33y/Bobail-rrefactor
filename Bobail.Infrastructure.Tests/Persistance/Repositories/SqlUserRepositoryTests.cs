using Bobail.Domain.Users;
using Bobail.Infrastructure.Persistance.Repositories;
using Bobail.Infrastructure.Tests.TestSupport;
using FluentAssertions;

namespace Bobail.Infrastructure.Tests.Persistance.Repositories;

public class SqlUserRepositoryTests
{
    [Fact]
    public async Task AddAsync_And_GetByEmailAsync_RoundTrip_User()
    {
        using var db = InfrastructureTestDb.Create();
        var repository = new SqlUserRepository(db.Context);
        var user = User("mihai@mail.com", "Mihai");

        await repository.AddAsync(user);
        var restored = await repository.GetByEmailAsync("mihai@mail.com");

        restored.Should().NotBeNull();
        restored!.Id.Should().Be(user.Id);
        restored.Email.Should().Be("mihai@mail.com");
        restored.Nickname.Should().Be("Mihai");
        restored.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_Persists_User_Changes()
    {
        using var db = InfrastructureTestDb.Create();
        var repository = new SqlUserRepository(db.Context);
        var user = User("player@mail.com", "Player");
        await repository.AddAsync(user);

        user.Nickname = "Updated";
        user.IsActive = false;
        user.IsEmailVerified = true;
        user.EmailVerifiedAtUtc = DateTime.UtcNow;
        await repository.UpdateAsync(user);

        var restored = await repository.GetByIdAsync(user.Id);
        restored!.Nickname.Should().Be("Updated");
        restored.IsActive.Should().BeFalse();
        restored.IsEmailVerified.Should().BeTrue();
        restored.EmailVerifiedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUsersAsync_And_CountUsersAsync_Apply_Search_And_Paging()
    {
        using var db = InfrastructureTestDb.Create();
        var repository = new SqlUserRepository(db.Context);
        await repository.AddAsync(User("ana@mail.com", "Ana"));
        await repository.AddAsync(User("bob@mail.com", "Bob"));
        await repository.AddAsync(User("mihai@mail.com", "Mihai"));

        var count = await repository.CountUsersAsync("MAIL");
        var users = await repository.GetUsersAsync(skip: 1, take: 1, search: "mail");

        count.Should().Be(3);
        users.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateAsync_When_User_Is_Missing_Throws()
    {
        using var db = InfrastructureTestDb.Create();
        var repository = new SqlUserRepository(db.Context);
        var user = User("missing@mail.com", "Missing");

        var act = () => repository.UpdateAsync(user);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage($"User with id '{user.Id}' was not found.");
    }

    private static User User(string email, string nickname)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Nickname = nickname,
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }
}
