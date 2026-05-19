using Bobail.Domain.Games;

namespace Bobail.Application.Interfaces.Services;

public interface IBoardEvaluator
{
    int Evaluate(Game game, PlayerColor botColor);
}
