using Bobail.Application.DTOs;

namespace Bobail.Application.Interfaces.Services;

public interface IAdminService
{
    Task<PagedAdminUsersResponse> GetUsersAsync(
        AdminUsersQuery query,
        CancellationToken cancellationToken = default);

    Task<BanUserResponse> BanUserAsync(
        Guid userId,
        Guid currentAdminId,
        CancellationToken cancellationToken = default);

    Task<AdminUserResponse> UnbanUserAsync(
        Guid userId,
        Guid currentAdminId,
        CancellationToken cancellationToken = default);
}
