using Bobail.API.Extensions;
using Bobail.API.Hubs;
using Bobail.API.Realtime;
using Bobail.Application.DTOs;
using Bobail.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;

namespace Bobail.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IHubContext<GameHub> _gameHubContext;
    private readonly IHubContext<AuthHub> _authHubContext;
    private readonly IGameConnectionTracker _connectionTracker;

    public AdminController(
        IAdminService adminService,
        IHubContext<GameHub> gameHubContext,
        IHubContext<AuthHub> authHubContext,
        IGameConnectionTracker connectionTracker)
    {
        _adminService = adminService;
        _gameHubContext = gameHubContext;
        _authHubContext = authHubContext;
        _connectionTracker = connectionTracker;
    }

    [HttpGet("users")]
    public async Task<ActionResult<PagedAdminUsersResponse>> GetUsers(
        [FromQuery] AdminUsersQuery query,
        CancellationToken cancellationToken)
    {
        var users = await _adminService.GetUsersAsync(query, cancellationToken);

        return Ok(users);
    }

    [HttpPatch("users/{id:guid}/ban")]
    public async Task<ActionResult<BanUserResponse>> BanUser(
        Guid id,
        CancellationToken cancellationToken)
    {
        var currentAdminId = User.GetUserId();
        var result = await _adminService.BanUserAsync(
            id,
            currentAdminId,
            cancellationToken);

        await NotifyFinishedGamesAsync(result.FinishedGames, cancellationToken);
        await ForceLogoutAsync(id, cancellationToken);

        return Ok(result);
    }

    [HttpPatch("users/{id:guid}/unban")]
    public async Task<ActionResult<AdminUserResponse>> UnbanUser(
        Guid id,
        CancellationToken cancellationToken)
    {
        var currentAdminId = User.GetUserId();
        var user = await _adminService.UnbanUserAsync(
            id,
            currentAdminId,
            cancellationToken);

        return Ok(user);
    }

    private async Task NotifyFinishedGamesAsync(
        IEnumerable<GameResponse> games,
        CancellationToken cancellationToken)
    {
        foreach (var game in games)
        {
            await _gameHubContext.Clients
                .Group(game.Id.ToString("D"))
                .SendAsync("GameEnded", game, cancellationToken);
        }
    }

    private async Task ForceLogoutAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var connectionIds = _connectionTracker.GetConnectionsForUser(userId);

        if (connectionIds.Count == 0)
            return;

        var payload = new
        {
            message = "Your account has been banned."
        };

        await _authHubContext.Clients
            .Clients(connectionIds)
            .SendAsync("ForceLogout", payload, cancellationToken);

        await _gameHubContext.Clients
            .Clients(connectionIds)
            .SendAsync("ForceLogout", payload, cancellationToken);
    }
}
