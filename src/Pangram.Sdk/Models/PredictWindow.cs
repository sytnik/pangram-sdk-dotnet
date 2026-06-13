
namespace Pangram.Sdk.Models;

/// <summary>
/// A single text window and its classification within a <see cref="PredictResponse"/>.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class PredictWindow
{
    /// <summary>The window text.</summary>
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Descriptive classification label (e.g., "AI-Generated", "Moderately AI-Assisted").
    /// </summary>
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Score detailing the level of AI assistance (0.0 = no AI assistance, 1.0 = AI-generated).
    /// </summary>
    [JsonPropertyName("ai_assistance_score")]
    public float AiAssistanceScore { get; set; }

    /// <summary>Confidence level for the classification ("High", "Medium", "Low").</summary>
    [JsonPropertyName("confidence")]
    public string Confidence { get; set; } = string.Empty;

    /// <summary>Starting character index in the original text.</summary>
    [JsonPropertyName("start_index")]
    public int StartIndex { get; set; }

    /// <summary>Ending character index in the original text.</summary>
    [JsonPropertyName("end_index")]
    public int EndIndex { get; set; }

    /// <summary>Number of words in the window.</summary>
    [JsonPropertyName("word_count")]
    public int WordCount { get; set; }

    /// <summary>Token length of the window.</summary>
    [JsonPropertyName("token_length")]
    public int TokenLength { get; set; }
}