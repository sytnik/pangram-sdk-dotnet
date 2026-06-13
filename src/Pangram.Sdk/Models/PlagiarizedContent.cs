namespace Pangram.Sdk.Models;

/// <summary>
/// A piece of plagiarised content identified in the source text, including the original source.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class PlagiarizedContent
{
    /// <summary>The plagiarised text snippet.</summary>
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    /// <summary>URL or reference of the original source.</summary>
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    /// <summary>Similarity score for this piece of content (0.0–1.0), if provided.</summary>
    [JsonPropertyName("similarity")]
    public float? Similarity { get; set; }
}