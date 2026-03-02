using Bobail.Domain.Board;
using Bobail.Domain.Common;

namespace Bobail.Domain.Games;

public class Game : Entity
{
    public Board.Board Board { get; }
    public PlayerColor CurrentTurn { get; private set; }
    public bool IsFirstTurn { get; private set; }
    public GameStatus Status { get; private set; }
    public PlayerColor? Winner { get; private set; }
    public TurnPhase CurrentPhase { get; private set; }

    public Game()
    {
        Board = new Board.Board();
        CurrentTurn = PlayerColor.Red;
        IsFirstTurn = true;
        Status = GameStatus.InProgress;
        CurrentPhase = TurnPhase.PlayerMoveRequired;
    }

    public void ExecuteBobailMove(Position target)
    {
        if (Status != GameStatus.InProgress)
            throw new DomainException("Game is not active.");

        if (IsFirstTurn)
            throw new DomainException("Bobail cannot be moved on first turn.");

        if (CurrentPhase != TurnPhase.BobailMoveRequired)
            throw new DomainException("Bobail has already been moved.");

        GameRules.ValidateBobailMove(this, target);
        GameRules.ApplyBobailMove(this, target);
        GameRules.CheckVictory(this);

        if (Status == GameStatus.Finished)
            return;

        CurrentPhase = TurnPhase.PlayerMoveRequired;
    }

    public void ExecutePlayerMove(Position from, Position to)
    {
        if (Status != GameStatus.InProgress)
            throw new DomainException("Game is not active.");

        if (CurrentPhase != TurnPhase.PlayerMoveRequired)
            throw new DomainException("Bobail must be moved first.");

        GameRules.ValidatePlayerMove(this, from, to);
        GameRules.ApplyPlayerMove(this, from, to);

        GameRules.CheckVictory(this);

        if (Status == GameStatus.InProgress)
        {
            SwitchTurn();

            if (IsFirstTurn)
                IsFirstTurn = false;

            CurrentPhase = TurnPhase.BobailMoveRequired;
        }
    }

    private void SwitchTurn()
    {
        CurrentTurn = CurrentTurn == PlayerColor.Red
            ? PlayerColor.Green
            : PlayerColor.Red;
    }

    public void Finish(PlayerColor winner)
    {
        Status = GameStatus.Finished;
        Winner = winner;
    }
    public List<Position> GetValidPlayerMoves(Position from)
    {
        return GameRules.GetValidPlayerMoves(this, from);
    }

    public List<Position> GetValidBobailMoves()
    {
        return GameRules.GetValidBobailMoves(this);
    }
}
