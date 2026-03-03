using Bobail.Domain.Board;
using Bobail.Domain.Games;

namespace Bobail.Application.Services.Bot;

public interface IBotStrategy
{
    BotDifficulty Difficulty { get; }

    BotMove DecideMove(Game game);
}