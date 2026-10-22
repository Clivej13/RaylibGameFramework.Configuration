using System.Text.Json;

namespace RaylibGameFramework.Configuration;

/// <summary>Persists application-defined settings or save data as JSON.</summary>
/// <typeparam name="T">The application's serializable data model.</typeparam>
public sealed class JsonSettingsStore<T> where T : class
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    /// <summary>The absolute path used for all reads and writes.</summary>
    public string FilePath { get; }

    /// <summary>Creates a store. Relative paths resolve against the current working directory.</summary>
    public JsonSettingsStore(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        FilePath = Path.GetFullPath(filePath);
    }

    /// <summary>Loads saved data, or returns null if the file does not exist.</summary>
    /// <remarks>Invalid JSON and filesystem errors are passed to the caller.</remarks>
    public T? Load()
    {
        if (!File.Exists(FilePath)) return null;
        return JsonSerializer.Deserialize<T>(File.ReadAllText(FilePath), Options)
            ?? throw new JsonException("Settings must contain a JSON object.");
    }

    /// <summary>Saves data, creating parent directories and replacing any existing file.</summary>
    /// <remarks>Serializes before writing and moves a complete temporary file into place.</remarks>
    public void Save(T settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        string json = JsonSerializer.Serialize(settings, Options);
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        string temporary = FilePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporary, json);
            File.Move(temporary, FilePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
}
