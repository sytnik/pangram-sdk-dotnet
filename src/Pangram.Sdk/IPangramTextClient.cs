namespace Pangram.Sdk;

/// <summary>
/// Defines the contract for interacting with the Pangram Labs text-analysis API (unofficial).
/// </summary>
public interface IPangramTextClient
{
    /// <summary>
    /// Classify text as AI- or human-written using the short prediction endpoint.
    /// </summary>
    /// <param name="text">The text to classify.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="PredictShortResponse"/> with the classification result.</returns>
    Task<PredictShortResponse> PredictShortAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Classify text as AI-, AI-assisted, or human-written using the V3 API.
    /// </summary>
    /// <param name="text">The text to classify.</param>
    /// <param name="publicDashboardLink">
    /// When <c>true</c>, the response includes a publicly-accessible dashboard link.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="PredictResponse"/> with windowed analysis and AI-assistance detection.</returns>
    Task<PredictResponse> PredictAsync(string text, bool publicDashboardLink = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Classify text and receive a public dashboard link alongside the classification result.
    /// </summary>
    /// <param name="text">The text to classify.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="PredictResponse"/> that includes a <see cref="PredictResponse.DashboardLink"/>.
    /// </returns>
    Task<PredictResponse> PredictWithDashboardLinkAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check text for potential plagiarism against a vast database of online content.
    /// </summary>
    /// <param name="text">The text to check for plagiarism.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="PlagiarismResponse"/> containing match details and statistics.</returns>
    Task<PlagiarismResponse> CheckPlagiarismAsync(string text, CancellationToken cancellationToken = default);
}