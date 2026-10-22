# RaylibGameFramework.Configuration

Loads game configuration with `ConfigLoader.Load(path)` and persists application-defined
settings or save data with `JsonSettingsStore<T>`.

## Save and load

Adapted from RaylibTackleAlley's `PackageContributions/Configuration/JsonSettingsStore.cs`.
The store has no game, graphics, or input dependencies.

```csharp
using RaylibGameFramework.Configuration;

string path = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "MyGame", "save.json");

var store = new JsonSettingsStore<PlayerSave>(path);
PlayerSave save = store.Load() ?? new PlayerSave();
save.Level = 2;
store.Save(save);

public sealed class PlayerSave
{
    public int Level { get; set; } = 1;
    public int HighScore { get; set; }
}
```

- Uses System.Text.Json with case-insensitive property reads and indented output.
  Define save models using serializable public properties.
- Missing files return null without creating a file. Malformed or incompatible JSON,
  including JSON null, throws JsonException.
- Save creates parent directories, serializes before touching the existing save,
  and replaces it using a complete temporary file in the same directory.
  Temporary files are cleaned up if replacement fails.
- FilePath is absolute. Relative store paths resolve against the working directory
  when constructed; ConfigLoader resolves relative paths against AppContext.BaseDirectory.
- The application owns defaults, validation, migrations, when to save, and error reporting.
  Catch IOException, UnauthorizedAccessException, and JsonException as appropriate.
  Coordinate concurrent writers in the application; the store does not merge saves.

## Validation

Run the persistence tests:

```text
dotnet test Tests/RaylibGameFramework.Configuration.Tests.csproj -c Release
dotnet pack RaylibGameFramework.Configuration.csproj -c Release
```
