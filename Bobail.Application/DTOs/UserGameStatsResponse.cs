namespace Bobail.Application.DTOs;

public class UserGameStatsResponse
{
    public int TotalGamesPlayed { get; set; }
    public int TotalWins { get; set; }
    public int TotalLosses { get; set; }
    public DateTime MemberSince { get; set; }
    public int WinsWithGreen { get; set; }
    public int WinsWithRed { get; set; }
    public int LossesWithGreen { get; set; }
    public int LossesWithRed { get; set; }
}
