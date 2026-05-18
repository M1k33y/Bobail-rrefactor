using System.Text.Json;

namespace Bobail.Training.Profiles;

public static class TrainingProfileWriter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string Save(TrainingRunProfile profile, string outputRootDirectory)
    {
        var outputDirectory = Path.Combine(
            outputRootDirectory,
            profile.CreatedAtUtc.ToString("yyyyMMdd-HHmmss"));

        Directory.CreateDirectory(outputDirectory);

        var outputPath = Path.Combine(outputDirectory, "best-profile.json");
        var json = JsonSerializer.Serialize(profile, Options);

        File.WriteAllText(outputPath, json);

        return outputPath;
    }
}
