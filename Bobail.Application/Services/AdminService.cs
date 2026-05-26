using Bobail.Application.DTOs;
using Bobail.Application.Interfaces.Repositories;
using Bobail.Application.Interfaces.Services;
using Bobail.Domain.Users;

namespace Bobail.Application.Services;

public class AdminService : IAdminService
{
    private const int AdminRole = 1;

    private readonly IUserRepository _userRepository;
    private readonly IOnlineGameService _onlineGameService;

    public AdminService(
        IUserRepository userRepository,
        IOnlineGameService onlineGameService)
    {
        _userRepository = userRepository;
        _onlineGameService = onlineGameService;
    }

    public async Task<PagedAdminUsersResponse> GetUsersAsync(
        AdminUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await _userRepository.CountUsersAsync(
            query.Search,
            cancellationToken);

        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)query.PageSize);
        var currentPage = totalPages == 0
            ? 1
            : Math.Min(query.Page, totalPages);
        var skip = (currentPage - 1) * query.PageSize;

        var users = totalCount == 0
            ? new List<User>()
            : await _userRepository.GetUsersAsync(
                skip,
                query.PageSize,
                query.Search,
                cancellationToken);

        return new PagedAdminUsersResponse
        {
            Page = currentPage,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPreviousPage = currentPage > 1,
            HasNextPage = currentPage < totalPages,
            Items = users.Select(MapToResponse).ToList()
        };
    }

    public async Task<BanUserResponse> BanUserAsync(
        Guid userId,
        Guid currentAdminId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
            throw new KeyNotFoundException("User not found.");

        if (user.Id == currentAdminId)
            throw new UnauthorizedAccessException("Admins cannot change their own active status.");

        if (user.Role == AdminRole && user.IsActive)
            throw new UnauthorizedAccessException("Admins cannot ban other admins.");

        if (!user.IsActive)
        {
            return new BanUserResponse
            {
                User = MapToResponse(user)
            };
        }

        user.IsActive = false;

        await _userRepository.UpdateAsync(user);

        var finishedGames = await _onlineGameService.ForfeitActiveGamesForUserAsync(
            user.Id,
            cancellationToken);

        return new BanUserResponse
        {
            User = MapToResponse(user),
            FinishedGames = finishedGames.ToList()
        };
    }

    public async Task<AdminUserResponse> UnbanUserAsync(
        Guid userId,
        Guid currentAdminId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
            throw new KeyNotFoundException("User not found.");

        if (user.IsActive)
            return MapToResponse(user);

        user.IsActive = true;

        await _userRepository.UpdateAsync(user);

        return MapToResponse(user);
    }

    private static AdminUserResponse MapToResponse(User user)
    {
        return new AdminUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Nickname = user.Nickname,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            Role = ToRoleName(user.Role)
        };
    }

    private static string ToRoleName(int role)
    {
        return role == AdminRole ? "Admin" : "User";
    }
}
