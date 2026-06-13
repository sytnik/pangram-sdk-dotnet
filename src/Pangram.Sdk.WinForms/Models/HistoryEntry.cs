namespace Pangram.Sdk.WinForms.Models;

public sealed class HistoryEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public string Method { get; set; } = string.Empty;
    public string InputText { get; set; } = string.Empty;
    public string ResponseJson { get; set; } = string.Empty;
}