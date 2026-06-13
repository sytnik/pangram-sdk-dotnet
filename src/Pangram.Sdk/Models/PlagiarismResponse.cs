namespace Pangram.Sdk.Models;

/// <summary>
/// Response returned by the plagiarism detection endpoint.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class PlagiarismResponse
{
    /// <summary>The input text that was checked.</summary>
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    /// <summary>Whether plagiarism was detected.</summary>
    [JsonPropertyName("plagiarism_detected")]
    public bool PlagiarismDetected { get; set; }

    /// <summary>List of detected plagiarised content with their sources.</summary>
    [JsonPropertyName("plagiarized_content")]
    public List<PlagiarizedContent> PlagiarizedContent { get; set; } = [];

    /// <summary>Total number of sentences checked.</summary>
    [JsonPropertyName("total_sentences")]
    public int TotalSentences { get; set; }

    /// <summary>List of sentences detected as plagiarised.</summary>
    [JsonPropertyName("plagiarized_sentences")]
    public List<string> PlagiarizedSentences { get; set; } = [];

    /// <summary>Percentage of text detected as plagiarised (0.0–100.0).</summary>
    [JsonPropertyName("percent_plagiarized")]
    public float PercentPlagiarized { get; set; }
}