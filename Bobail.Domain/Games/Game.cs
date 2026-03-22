using BoardNamespace = Bobail.Domain.Board;
using Bobail.Domain.Common;
using System.Text.Json.Serialization;
namespace Bobail.Domain.Games;

public class Game : Entity
{
    public BoardNamespace.Board Board { get; }
    public PlayerColor CurrentTurn { get; private set; }
    public bool IsFirstTurn { get; private set; }
    public GameStatus Status { get; private set; }
    public PlayerColor? Winner { get; private set; }
    public TurnPhase CurrentPhase { get; private set; }

    public GameMode Mode { get; private set; }
    public BotDifficulty? BotDifficulty { get; private set; }
    public PlayerColor? BotColor { get; private set; }

    [JsonConstructor]
    private Game(
    Guid id, 
    GameMode mode,
    BotDifficulty? botDifficulty,
    PlayerColor? botColor,
    BoardNamespace.Board board,
    PlayerColor currentTurn,
    bool isFirstTurn,
    GameStatus status,
    TurnPhase currentPhase,
    PlayerColor? winner)
    {
        Id = id; 

        Board = board;

        Mode = mode;
        BotDifficulty = botDifficulty;
        BotColor = botColor;

        CurrentTurn = currentTurn;
        IsFirstTurn = isFirstTurn;
        Status = status;
        CurrentPhase = currentPhase;
        Winner = winner;
    }
    public Game(
    GameMode mode = GameMode.LocalMultiplayer,
    BotDifficulty? botDifficulty = null,
    PlayerColor? botColor = null)
    {

        GenerateId();
        Board = new BoardNamespace.Board();

        CurrentTurn = PlayerColor.Red;
        IsFirstTurn = true;
        Status = GameStatus.InProgress;
        CurrentPhase = TurnPhase.PlayerMoveRequired;

        Mode = mode;
        BotDifficulty = botDifficulty;
        BotColor = botColor;
    }

    public bool IsBotTurn()
    {
        return Mode == GameMode.PlayerVsBot &&
               BotColor.HasValue &&
               CurrentTurn == BotColor.Value;
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

    public Game Clone()
    {
        return new Game(
            Id, 
            Mode,
            BotDifficulty,
            BotColor,
            Board.Clone(),
            CurrentTurn,
            IsFirstTurn,
            Status,
            CurrentPhase,
            Winner);
    }
}