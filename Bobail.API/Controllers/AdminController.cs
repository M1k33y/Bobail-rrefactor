using Bobail.API.Extensions;
using Bobail.Application.DTOs;
using Bobail.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bobail.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("users")]
    public async Task<ActionResult<PagedAdminUsersResponse>> GetUsers(
        [FromQuery] AdminUsersQuery query,
        CancellationToken cancellationToken)
    {
        var users = await _adminService.GetUsersAsync(query, cancellationToken);

        return Ok(users);
    }

    [HttpPatch("users/{id:guid}/toggle-active")]
    public async Task<ActionResult<AdminUserResponse>> ToggleUserActive(
        Guid id,
        CancellationToken cancellationToken)
    {
        var currentAdminId = User.GetUserId();
        var user = await _adminService.ToggleUserActiveAsync(
            id,
            currentAdminId,
            cancellationToken);

        return Ok(user);
    }
}
