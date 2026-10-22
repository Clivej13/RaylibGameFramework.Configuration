using System.Text.Json;
using RaylibGameFramework.Configuration;
using Xunit;

public sealed class JsonSettingsStoreTests
{
    [Fact]
    public void PersistenceAndFailureBehavior()
    {
        string directory = Path.Combine(AppContext.BaseDirectory, "settings-tests-" + Guid.NewGuid().ToString("N"));
        try
        {
            string path = Path.Combine(directory, "nested", "save.json");
            var store = new JsonSettingsStore<SaveData>(path);
            Check(store.Load() is null && !Directory.Exists(directory), "Missing saves must not create files.");

            store.Save(new() { Level = 3, Bindings = new() { ["Jump"] = "Space" } });
            var restored = new JsonSettingsStore<SaveData>(path).Load()!;
            Check(restored.Level == 3 && restored.Bindings["Jump"] == "Space", "Data must survive a new store.");

            store.Save(new() { Level = 5 });
            Check(store.Load()!.Level == 5, "Saving must replace existing data.");
            Check(Directory.GetFiles(Path.GetDirectoryName(path)!).Length == 1, "Successful writes must clean temporary files.");

            File.WriteAllText(path, "{\"level\":7}");
            Check(store.Load()!.Level == 7, "Property names must load case-insensitively.");
            foreach (string invalid in new[] { "{broken", "null", "[]", "{\"Level\":\"bad\"}" })
            {
                File.WriteAllText(path, invalid);
                Throws<JsonException>(() => store.Load());
                Check(File.ReadAllText(path) == invalid, "Loading must preserve invalid saves.");
            }

            store.Save(new() { Level = 9 });
            string previous = File.ReadAllText(path);
            Throws<ArgumentNullException>(() => store.Save(null!));
            var cycle = new CyclicData();
            cycle.Next = cycle;
            Throws<JsonException>(() => new JsonSettingsStore<CyclicData>(path).Save(cycle));
            Check(File.ReadAllText(path) == previous, "Failed serialization must preserve the previous save.");
            Check(Directory.GetFiles(Path.GetDirectoryName(path)!).Length == 1, "Failed serialization must not leave temporary files.");

            string blocked = Path.Combine(directory, "blocked");
            Directory.CreateDirectory(blocked);
            Throws<IOException>(() => new JsonSettingsStore<SaveData>(Path.Combine(path, "child.json")).Save(new()));
            var failure = Record.Exception(() => new JsonSettingsStore<SaveData>(blocked).Save(new()));
            Assert.True(failure is IOException or UnauthorizedAccessException);
            Check(!Directory.GetFiles(directory, "*.tmp", SearchOption.AllDirectories).Any(), "Failed replacement must clean temporary files.");

            Throws<ArgumentNullException>(() => new JsonSettingsStore<SaveData>(null!));
            Throws<ArgumentException>(() => new JsonSettingsStore<SaveData>(" "));
            var relative = new JsonSettingsStore<SaveData>("relative-save.json");
            Check(relative.FilePath == Path.GetFullPath("relative-save.json"), "Relative paths must resolve at construction.");
            Console.WriteLine("All JSON settings store checks passed.");
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }

    }

    static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    static void Throws<TException>(Action action) where TException : Exception
    {
        try { action(); }
        catch (TException) { return; }
        throw new Exception($"Expected {typeof(TException).Name}.");
    }

}

public sealed class SaveData
{
    public int Level { get; set; }
    public Dictionary<string, string> Bindings { get; set; } = new();
}

public sealed class CyclicData
{
    public CyclicData? Next { get; set; }
}
