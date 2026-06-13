namespace Pangram.Sdk.WinForms.Services;

public sealed class HistoryService
{
    private static readonly string HistoryPath = Path.Combine(ConfigService.AppDataDir, "history.json");
    private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

    private readonly List<HistoryEntry> _entries = [];

    public IReadOnlyList<HistoryEntry> Entries => _entries.AsReadOnly();

    public event EventHandler? HistoryChanged;

    public void Load()
    {
        if (!File.Exists(HistoryPath))
            return;

        try
        {
            var json = File.ReadAllText(HistoryPath);
            var loaded = JsonSerializer.Deserialize<List<HistoryEntry>>(json);
            if (loaded != null)
            {
                _entries.Clear();
                _entries.AddRange(loaded);
            }
        }
        catch { }    }

    public async Task AppendAsync(HistoryEntry entry)
    {
        _entries.Insert(0, entry);
        await SaveAsync().ConfigureAwait(false);
        HistoryChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        _entries.Clear();
        Directory.CreateDirectory(ConfigService.AppDataDir);
        File.WriteAllText(HistoryPath, "[]");
        HistoryChanged?.Invoke(this, EventArgs.Empty);
    }

    private async Task SaveAsync()
    {
        Directory.CreateDirectory(ConfigService.AppDataDir);
        var json = JsonSerializer.Serialize(_entries, WriteOptions);
        await File.WriteAllTextAsync(HistoryPath, json).ConfigureAwait(false);
    }
}
