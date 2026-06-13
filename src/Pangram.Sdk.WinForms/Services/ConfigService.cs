namespace Pangram.Sdk.WinForms.Services;

public sealed class ConfigService
{
    public static readonly string AppDataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PangramSdkApp");

    public static readonly string SettingsPath =
        Path.Combine(AppContext.BaseDirectory, "settings.json");

    private static readonly string ConfigPath = SettingsPath;

    private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

    private AppConfig _config = new();

    public AppConfig Config => _config;

    public void Load()
    {
        if (!File.Exists(ConfigPath))
        {
            _config = new AppConfig();
            return;
        }

        try
        {
            var json = File.ReadAllText(ConfigPath);
            _config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }
        catch
        {
            _config = new AppConfig();
        }
    }

    public void Save()
    {
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(_config, WriteOptions));
    }
}
