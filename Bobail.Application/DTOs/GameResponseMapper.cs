using Bobail.Domain.Games;

namespace Bobail.Application.DTOs;

public static class GameResponseMapper
{
    public static GameResponse ToResponse(
        Game game,
        PlayerColor? playerColor = null,
        DateTimeOffset? serverTimeUtc = null)
    {
        return new GameResponse
        {
            Id = game.Id,
            Status = game.Status.ToString(),
            CurrentTurn = game.CurrentTurn.ToString(),
            Winner = game.Winner?.ToString(),
            EndReason = game.EndReason?.ToString(),
            IsFirstTurn = game.IsFirstTurn,
            CurrentPhase = game.CurrentPhase.ToString(),
            Mode = game.Mode.ToString(),
            BotColor = game.BotColor?.ToString(),
            PlayerColor = playerColor?.ToString(),
            Clock = ToClockDto(game, serverTimeUtc),
            Pieces = game.Board.Pieces.Select(p => new PieceDto
            {
                Type = p.Type.ToString(),
                Owner = p.Owner?.ToString(),
                Row = p.Position.Row,
                Column = p.Position.Column
            }).ToList()
        };
    }

    private static GameClockDto? ToClockDto(
        Game game,
        DateTimeOffset? serverTimeUtc)
    {
        if (game.Clock is null)
            return null;

        var resolvedServerTimeUtc = (serverTimeUtc ?? DateTimeOffset.UtcNow).ToUniversalTime();
        var activeColor = game.Status == GameStatus.InProgress
            ? game.CurrentTurn
            : (PlayerColor?)null;

        return new GameClockDto
        {
            InitialTimeMilliseconds = game.Clock.TimeControl.InitialTimeMilliseconds,
            RedRemainingMilliseconds = game.Clock.GetRemainingMilliseconds(
                PlayerColor.Red,
                activeColor,
                resolvedServerTimeUtc),
            GreenRemainingMilliseconds = game.Clock.GetRemainingMilliseconds(
                PlayerColor.Green,
                activeColor,
                resolvedServerTimeUtc),
            TurnStartedAtUtc = game.Clock.TurnStartedAtUtc,
            ServerTimeUtc = resolvedServerTimeUtc
        };
    }
}
