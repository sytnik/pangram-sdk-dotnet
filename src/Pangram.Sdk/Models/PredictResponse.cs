namespace Pangram.Sdk.Models;

/// <summary>
/// Response returned by the V3 text prediction endpoint, including AI-assistance detection.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class PredictResponse
{
    /// <summary>The input text that was classified.</summary>
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    /// <summary>The API version identifier (e.g., "3.0").</summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>Classification headline summarising the result.</summary>
    [JsonPropertyName("headline")]
    public string Headline { get; set; } = string.Empty;

    /// <summary>Long-form prediction string describing the classification.</summary>
    [JsonPropertyName("prediction")]
    public string Prediction { get; set; } = string.Empty;

    /// <summary>Short-form prediction string ("AI", "AI-Assisted", "Human", "Mixed").</summary>
    [JsonPropertyName("prediction_short")]
    public string PredictionShort { get; set; } = string.Empty;

    /// <summary>Fraction of text classified as AI-written (0.0–1.0).</summary>
    [JsonPropertyName("fraction_ai")]
    public float FractionAi { get; set; }

    /// <summary>Fraction of text classified as AI-assisted (0.0–1.0).</summary>
    [JsonPropertyName("fraction_ai_assisted")]
    public float FractionAiAssisted { get; set; }

    /// <summary>Fraction of text classified as human-written (0.0–1.0).</summary>
    [JsonPropertyName("fraction_human")]
    public float FractionHuman { get; set; }

    /// <summary>Number of text segments classified as AI.</summary>
    [JsonPropertyName("num_ai_segments")]
    public int NumAiSegments { get; set; }

    /// <summary>Number of text segments classified as AI-assisted.</summary>
    [JsonPropertyName("num_ai_assisted_segments")]
    public int NumAiAssistedSegments { get; set; }

    /// <summary>Number of text segments classified as human.</summary>
    [JsonPropertyName("num_human_segments")]
    public int NumHumanSegments { get; set; }

    /// <summary>
    /// Link to the dashboard page containing the full classification result.
    /// Only present when <c>publicDashboardLink</c> was set to <c>true</c>.
    /// </summary>
    [JsonPropertyName("dashboard_link")]
    public string? DashboardLink { get; set; }

    /// <summary>List of text windows and their individual classifications.</summary>
    [JsonPropertyName("windows")]
    public List<PredictWindow> Windows { get; set; } = [];
}