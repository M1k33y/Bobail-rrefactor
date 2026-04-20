namespace Bobail.AI.Analysis.Services;

public static class OutputPaths
{
    public static string Create(string rootDirectory)
    {
        var outputDirectory = Path.Combine(
            rootDirectory,
            DateTime.Now.ToString("yyyyMMdd-HHmmss"));

        Directory.CreateDirectory(outputDirectory);
        return outputDirectory;
    }
}
