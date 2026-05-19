using Bobail.Domain.Games;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bobail.Infrastructure.Persistance;

public static class GameSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        IncludeFields = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    public static string Serialize(Game game)
    {
        var json = JsonSerializer.Serialize(game, Options);

        return json;
    }

    public static Game Deserialize(string json)
    {
        var game = JsonSerializer.Deserialize<Game>(json, Options);

        if (game == null)
            throw new InvalidOperationException("Failed to deserialize game.");

        return game;
    }
}
