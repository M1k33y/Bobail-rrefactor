using Bobail.Domain.Games;

namespace Bobail.Infrastructure.Bots;

//am 32 biti pentru 2 uint uri marchez pozitiile pieselor rosii si apoi a piseselor verzi in altul

//!! Cacheing ul global nu prea are sens pentru ca e foarte weight dependent

internal readonly record struct SearchBoardStateKey(
    uint RedPieces,
    uint GreenPieces,
    byte BobailIndex,
    PlayerColor CurrentTurn,
    TurnPhase CurrentPhase,
    GameStatus Status,
    byte Winner,
    bool IsFirstTurn);

internal static class SearchBoardStateKeyBuilder
{
    private const int BoardSize = 5;

    public static SearchBoardStateKey FromGame(Game game)
    {
        uint redPieces = 0;
        uint greenPieces = 0;
        byte bobailIndex = 0;

        foreach (var piece in game.Board.Pieces)
        {
            int index = (piece.Position.Row * BoardSize) + piece.Position.Column;
            uint bit = 1u << index;

            if (piece.IsBobail)
            {
                bobailIndex = (byte)index;
                continue;
            }

            if (piece.Owner == PlayerColor.Red)
            {
                redPieces |= bit;
            }
            else
            {
                greenPieces |= bit;
            }
        }

        return new SearchBoardStateKey(
            redPieces,
            greenPieces,
            bobailIndex,
            game.CurrentTurn,
            game.CurrentPhase,
            game.Status,
            EncodeWinner(game.Winner),
            game.IsFirstTurn);
    }

    private static byte EncodeWinner(PlayerColor? winner)
    {
        return winner switch
        {
            PlayerColor.Red => 1,
            PlayerColor.Green => 2,
            _ => 0
        };
    }
}
