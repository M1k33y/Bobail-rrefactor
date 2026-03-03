using Bobail.Domain.Games;

namespace Bobail.API.DTOs;

public class CreateBotGameRequest
{
    public PlayerColor BotColor { get; set; }
    
    public BotDifficulty Difficulty { get; set; }
}