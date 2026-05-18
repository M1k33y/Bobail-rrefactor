using System.Text.Json;
using Bobail.AI.Analysis.Models;
using Bobail.Domain.Games;

namespace Bobail.AI.Analysis.Services;

public sealed class BotProfileLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public IReadOnlyList<BotProfile> Load(AnalysisOptions options)
    {
        var profiles = new List<BotProfile>();

        if (options.IncludeDefaultProfiles)
            profiles.AddRange(BotProfile.CreateDefaults());

        foreach (var path in ResolveProfilePaths(options))
        {
            profiles.Add(LoadFromFile(path));
        }

        EnsureUniqueNames(profiles);

        return profiles;
    }

    private static IEnumerable<string> ResolveProfilePaths(AnalysisOptions options)
    {
        var paths = new List<string>();

        if (!string.IsNullOrWhiteSpace(options.ProfileInputDirectory) &&
            Directory.Exists(options.ProfileInputDirectory))
        {
            paths.AddRange(Directory.GetFiles(
                options.ProfileInputDirectory,
                "*.json",
                SearchOption.TopDirectoryOnly));
        }

        paths.AddRange(options.ProfilePaths.Where(path => !string.IsNullOrWhiteSpace(path)));

        return paths.Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static BotProfile LoadFromFile(string path)
    {
        var json = File.ReadAllText(path);
        var file = JsonSerializer.Deserialize<BotProfileFile>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Could not read bot profile '{path}'.");

        if (string.IsNullOrWhiteSpace(file.Name))
            throw new InvalidOperationException($"Bot profile '{path}' is missing a name.");

        if (!Enum.TryParse<BotDifficulty>(file.Difficulty, ignoreCase: true, out var difficulty))
            throw new InvalidOperationException($"Bot profile '{path}' has invalid difficulty '{file.Difficulty}'.");

        return new BotProfile(file.Name.Trim(), difficulty, file.Weights);
    }

    private static void EnsureUniqueNames(IReadOnlyCollection<BotProfile> profiles)
    {
        var duplicate = profiles
            .GroupBy(profile => profile.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicate is not null)
            throw new InvalidOperationException($"Duplicate bot profile name '{duplicate.Key}'.");
    }
}
