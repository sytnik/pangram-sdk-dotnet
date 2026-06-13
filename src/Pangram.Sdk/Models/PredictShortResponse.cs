namespace Pangram.Sdk.Models;

/// <summary>
/// Response returned by the short text prediction endpoint.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class PredictShortResponse
{
    /// <summary>The input text that was classified.</summary>
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// AI likelihood score on a scale from 0.0 (human-written) to 1.0 (AI-written).
    /// </summary>
    [JsonPropertyName("ai_likelihood")]
    public float AiLikelihood { get; set; }

    /// <summary>A string representing the classification result.</summary>
    [JsonPropertyName("prediction")]
    public string Prediction { get; set; } = string.Empty;
}