using Bobail.Domain.Games;

public interface IBoardEvaluator
{
    int Evaluate(Game game, PlayerColor botColor);
}