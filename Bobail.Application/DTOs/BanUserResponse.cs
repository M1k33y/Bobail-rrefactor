namespace Bobail.Application.DTOs;

public class BanUserResponse
{
    public AdminUserResponse User { get; set; } = new();

    public List<GameResponse> FinishedGames { get; set; } = new();
}
