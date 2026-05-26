using Bobail.Application.DTOs;
using Bobail.Application.Interfaces.Repositories;
using Bobail.Application.Interfaces.Services;
using Bobail.Application.Services;
using Bobail.Domain.Users;
using Moq;

namespace Bobail.Application.Tests.Services;

public class AdminServiceTests
{
    [Fact]
    public async Task GetUsersAsync_Should_Return_Paged_Users()
    {
        var users = new List<User>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Email = "admin@mail.com",
                Nickname = "admin",
                Role = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Email = "player@mail.com",
                Nickname = "player",
                Role = 0,
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(x => x.CountUsersAsync("mail", It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);
        userRepositoryMock
            .Setup(x => x.GetUsersAsync(0, 25, "mail", It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var service = CreateService(userRepositoryMock);

        var result = await service.GetUsersAsync(new AdminUsersQuery
        {
            Search = "mail"
        });

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("Admin", result.Items[0].Role);
        Assert.Equal("User", result.Items[1].Role);
    }

    [Fact]
    public async Task BanUserAsync_Should_Ban_Active_User_And_Forfeit_Games()
    {
        var adminId = Guid.NewGuid();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "player@mail.com",
            Nickname = "player",
            IsActive = true
        };

        User? updatedUser = null;
        var userRepositoryMock = new Mock<IUserRepository>();
        var onlineGameServiceMock = new Mock<IOnlineGameService>();
        userRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id))
            .ReturnsAsync(user);
        userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Callback<User>(candidate => updatedUser = candidate);
        onlineGameServiceMock
            .Setup(x => x.ForfeitActiveGamesForUserAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameResponse>());

        var service = CreateService(userRepositoryMock, onlineGameServiceMock);

        var result = await service.BanUserAsync(user.Id, adminId);

        Assert.False(result.User.IsActive);
        Assert.NotNull(updatedUser);
        Assert.False(updatedUser!.IsActive);
        onlineGameServiceMock.Verify(
            x => x.ForfeitActiveGamesForUserAsync(user.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task BanUserAsync_Should_Block_Self_Ban()
    {
        var adminId = Guid.NewGuid();
        var admin = new User
        {
            Id = adminId,
            Email = "admin@mail.com",
            Nickname = "admin",
            Role = 1,
            IsActive = true
        };

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(x => x.GetByIdAsync(adminId))
            .ReturnsAsync(admin);

        var service = CreateService(userRepositoryMock);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.BanUserAsync(adminId, adminId));

        userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task BanUserAsync_Should_Block_Banning_Active_Admin()
    {
        var adminId = Guid.NewGuid();
        var targetAdmin = new User
        {
            Id = Guid.NewGuid(),
            Email = "target@mail.com",
            Nickname = "target",
            Role = 1,
            IsActive = true
        };

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(x => x.GetByIdAsync(targetAdmin.Id))
            .ReturnsAsync(targetAdmin);

        var service = CreateService(userRepositoryMock);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.BanUserAsync(targetAdmin.Id, adminId));

        userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UnbanUserAsync_Should_Activate_Inactive_User()
    {
        var adminId = Guid.NewGuid();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "player@mail.com",
            Nickname = "player",
            IsActive = false
        };

        User? updatedUser = null;
        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id))
            .ReturnsAsync(user);
        userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Callback<User>(candidate => updatedUser = candidate);

        var service = CreateService(userRepositoryMock);

        var result = await service.UnbanUserAsync(user.Id, adminId);

        Assert.True(result.IsActive);
        Assert.NotNull(updatedUser);
        Assert.True(updatedUser!.IsActive);
    }

    private static AdminService CreateService(
        Mock<IUserRepository> userRepositoryMock,
        Mock<IOnlineGameService>? onlineGameServiceMock = null)
    {
        return new AdminService(
            userRepositoryMock.Object,
            (onlineGameServiceMock ?? new Mock<IOnlineGameService>()).Object);
    }
}
