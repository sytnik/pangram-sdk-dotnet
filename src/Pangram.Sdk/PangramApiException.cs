namespace Pangram.Sdk;

/// <summary>
/// Exception thrown when the Pangram API returns an error response.
/// </summary>
public sealed class PangramApiException : Exception
{
    /// <summary>The HTTP status code returned by the API, if available.</summary>
    public int? StatusCode { get; }

    /// <summary>The raw response body returned by the API, if available.</summary>
    public string? ResponseBody { get; }

    /// <summary>
    /// Initialises a new instance of <see cref="PangramApiException"/> with a message.
    /// </summary>
    public PangramApiException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initialises a new instance of <see cref="PangramApiException"/> with a message and HTTP details.
    /// </summary>
    public PangramApiException(string message, int statusCode, string responseBody)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    /// <summary>
    /// Initialises a new instance of <see cref="PangramApiException"/> with a message and inner exception.
    /// </summary>
    public PangramApiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}