using Bobail.Application.DTOs;
using Bobail.Application.Interfaces.Repositories;
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

        var service = new AdminService(userRepositoryMock.Object);

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
    public async Task ToggleUserActiveAsync_Should_Ban_Active_User()
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
        userRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id))
            .ReturnsAsync(user);
        userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Callback<User>(candidate => updatedUser = candidate);

        var service = new AdminService(userRepositoryMock.Object);

        var result = await service.ToggleUserActiveAsync(user.Id, adminId);

        Assert.False(result.IsActive);
        Assert.NotNull(updatedUser);
        Assert.False(updatedUser!.IsActive);
    }

    [Fact]
    public async Task ToggleUserActiveAsync_Should_Block_Self_Toggle()
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

        var service = new AdminService(userRepositoryMock.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.ToggleUserActiveAsync(adminId, adminId));

        userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task ToggleUserActiveAsync_Should_Block_Banning_Active_Admin()
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

        var service = new AdminService(userRepositoryMock.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.ToggleUserActiveAsync(targetAdmin.Id, adminId));

        userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }
}
