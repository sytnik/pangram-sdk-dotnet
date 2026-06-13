namespace Pangram.Sdk.Tests.Helpers;

/// <summary>Factory methods for creating <see cref="PangramTextClient"/> instances backed by a stub handler.</summary>
internal static class ClientFactory
{
    public static PangramTextClient Create(string responseBody,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handler = new StubHttpMessageHandler(responseBody, statusCode);
        var httpClient = new HttpClient(handler);
        httpClient.DefaultRequestHeaders.Add("x-api-key", "test-key");
        return new PangramTextClient(httpClient);
    }
}