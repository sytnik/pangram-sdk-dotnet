namespace Pangram.Sdk;

/// <summary>
/// Unofficial .NET client for the Pangram Labs text-analysis API.
/// </summary>
/// <remarks>
/// <para>
/// Create one instance per application lifetime and reuse it. When you supply
/// your own <see cref="HttpClient"/> the client does <em>not</em> dispose it;
/// when the parameterless / API-key constructors create their own
/// <see cref="HttpClient"/> it is disposed together with this instance.
/// </para>
/// <para>
/// Non-deprecated surface:
/// <list type="bullet">
///   <item><see cref="PredictShortAsync"/> — fast single-score classification.</item>
///   <item><see cref="PredictAsync"/> — full V3 windowed analysis with AI-assistance detection.</item>
///   <item><see cref="PredictWithDashboardLinkAsync"/> — same as <see cref="PredictAsync"/> but returns a public dashboard URL.</item>
///   <item><see cref="CheckPlagiarismAsync"/> — plagiarism detection against online content.</item>
/// </list>
/// </para>
/// </remarks>
public sealed class PangramTextClient : IPangramTextClient, IDisposable
{
    // -----------------------------------------------------------------------
    // API endpoints
    // -----------------------------------------------------------------------
    private const string ApiEndpoint = "https://text.api.pangram.com/v3";
    private const string PlagiarismApiEndpoint = "https://plagiarism.api.pangram.com";
    private const string SourceVersion = "dotnet_sdk_1.0.0";

    // -----------------------------------------------------------------------
    // Fields
    // -----------------------------------------------------------------------
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // -----------------------------------------------------------------------
    // Constructors
    // -----------------------------------------------------------------------

    /// <summary>
    /// Initialises the client with an API key. An internal <see cref="HttpClient"/>
    /// with a 90-second timeout is created and owned by this instance.
    /// </summary>
    /// <param name="apiKey">Your Pangram API key.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="apiKey"/> is null or whitespace.</exception>
    public PangramTextClient(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key must not be null or empty.", nameof(apiKey));

        _httpClient = BuildHttpClient(apiKey);
        _ownsHttpClient = true;
    }

    /// <summary>
    /// Initialises the client using the <c>PANGRAM_API_KEY</c> environment variable.
    /// An internal <see cref="HttpClient"/> is created and owned by this instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <c>PANGRAM_API_KEY</c> is not set in the environment.
    /// </exception>
    public PangramTextClient()
    {
        var apiKey = Environment.GetEnvironmentVariable("PANGRAM_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException(
                "API key is required. Set the PANGRAM_API_KEY environment variable or use the PangramTextClient(string apiKey) constructor.");

        _httpClient = BuildHttpClient(apiKey);
        _ownsHttpClient = true;
    }

    /// <summary>
    /// Initialises the client with a pre-configured <see cref="HttpClient"/>.
    /// The caller is responsible for disposing the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="httpClient">
    /// A configured <see cref="HttpClient"/> whose <c>x-api-key</c> default request header
    /// is already set to a valid Pangram API key.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpClient"/> is <c>null</c>.</exception>
    public PangramTextClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _ownsHttpClient = false;
    }

    // -----------------------------------------------------------------------
    // Public API
    // -----------------------------------------------------------------------

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">Thrown when <paramref name="text"/> is null or empty.</exception>
    /// <exception cref="PangramApiException">Thrown when the API returns an error response.</exception>
    public async Task<PredictShortResponse> PredictShortAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        ValidateText(text);

        var payload = new { text, source = SourceVersion };
        return await PostAsync<PredictShortResponse>(ApiEndpoint, payload, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">Thrown when <paramref name="text"/> is null or empty.</exception>
    /// <exception cref="PangramApiException">Thrown when the API returns an error response.</exception>
    public async Task<PredictResponse> PredictAsync(
        string text,
        bool publicDashboardLink = false,
        CancellationToken cancellationToken = default)
    {
        ValidateText(text);

        var payload = new { text, source = SourceVersion, public_dashboard_link = publicDashboardLink };
        return await PostAsync<PredictResponse>(ApiEndpoint, payload, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">Thrown when <paramref name="text"/> is null or empty.</exception>
    /// <exception cref="PangramApiException">Thrown when the API returns an error response.</exception>
    public Task<PredictResponse> PredictWithDashboardLinkAsync(
        string text,
        CancellationToken cancellationToken = default)
        => PredictAsync(text, publicDashboardLink: true, cancellationToken);

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">Thrown when <paramref name="text"/> is null or empty.</exception>
    /// <exception cref="PangramApiException">Thrown when the API returns an error response.</exception>
    public async Task<PlagiarismResponse> CheckPlagiarismAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        ValidateText(text);

        var payload = new { text, source = SourceVersion };
        return await PostAsync<PlagiarismResponse>(PlagiarismApiEndpoint, payload, cancellationToken)
            .ConfigureAwait(false);
    }

    // -----------------------------------------------------------------------
    // IDisposable
    // -----------------------------------------------------------------------

    /// <summary>
    /// Disposes the internal <see cref="HttpClient"/> if it was created by this instance.
    /// </summary>
    public void Dispose()
    {
        if (_ownsHttpClient)
            _httpClient.Dispose();
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    private async Task<T> PostAsync<T>(string endpoint, object payload, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(endpoint, content, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new PangramApiException("An error occurred while sending the request to the Pangram API.", ex);
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new PangramApiException(
                $"The Pangram API returned an error: [{(int)response.StatusCode}] {body}",
                (int)response.StatusCode,
                body);

        // Surface application-level errors that arrive with HTTP 200
        using var doc = JsonDocument.Parse(body);
        if (doc.RootElement.TryGetProperty("error", out var errorProp))
            throw new PangramApiException($"The Pangram API returned an error: {errorProp.GetString()}");

        return JsonSerializer.Deserialize<T>(body, JsonOptions)
               ?? throw new PangramApiException("The Pangram API returned an empty or unreadable response.");
    }

    private static HttpClient BuildHttpClient(string apiKey)
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(90) };
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    private static void ValidateText(string text)
    {
        if (string.IsNullOrEmpty(text))
            throw new ArgumentException("Text must not be null or empty.", nameof(text));
    }
}