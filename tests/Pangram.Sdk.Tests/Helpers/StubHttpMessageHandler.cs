namespace Pangram.Sdk.Tests.Helpers;

/// <summary>
/// A delegating handler that returns a pre-configured response without making any real HTTP calls.
/// </summary>
internal sealed class StubHttpMessageHandler(string responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
    : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(responseBody, System.Text.Encoding.UTF8, "application/json")
        };
        return Task.FromResult(response);
    }
}