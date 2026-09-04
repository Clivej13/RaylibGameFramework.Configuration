using System.Text.Json;

namespace RaylibGameFramework.Configuration;

public static class ConfigLoader
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static GameConfig Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        string resolvedPath = Path.IsPathRooted(path)
            ? path
            : Path.Combine(AppContext.BaseDirectory, path);

        if (!File.Exists(resolvedPath))
        {
            return new GameConfig();
        }

        try
        {
            string json = File.ReadAllText(resolvedPath);
            return JsonSerializer.Deserialize<GameConfig>(json, SerializerOptions)
                ?? throw new InvalidOperationException($"Configuration file '{resolvedPath}' must contain a JSON object.");
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                $"Configuration file '{resolvedPath}' contains malformed JSON.",
                exception);
        }
    }
}
