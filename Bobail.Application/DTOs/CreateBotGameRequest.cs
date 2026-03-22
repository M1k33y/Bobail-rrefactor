using Bobail.Domain.Games;

namespace Bobail.Application.DTOs;

public class CreateBotGameRequest
{
    public PlayerColor BotColor { get; set; }
    
    public BotDifficulty Difficulty { get; set; }
}