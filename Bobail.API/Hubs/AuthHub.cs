using Bobail.API.Extensions;
using Bobail.API.Realtime;
using Bobail.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Bobail.API.Hubs;

[Authorize]
public class AuthHub : Hub
{
    private readonly IGameConnectionTracker _connectionTracker;
    private readonly IUserRepository _userRepository;

    public AuthHub(
        IGameConnectionTracker connectionTracker,
        IUserRepository userRepository)
    {
        _connectionTracker = connectionTracker;
        _userRepository = userRepository;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User!.GetUserId();
        var user = await _userRepository.GetByIdAsync(userId);

        if (user is null || !user.IsActive)
        {
            Context.Abort();
            return;
        }

        await _connectionTracker.TrackConnectionAsync(
            Context.ConnectionId,
            userId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _connectionTracker.RemoveConnectionAsync(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
