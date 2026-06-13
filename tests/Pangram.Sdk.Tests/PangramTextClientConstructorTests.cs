namespace Pangram.Sdk.Tests;

public sealed class PangramTextClientConstructorTests
{
    [Fact]
    public void Constructor_WithApiKey_DoesNotThrow()
    {
        var client = new PangramTextClient("my-api-key");
        client.Dispose();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrWhitespaceApiKey_Throws(string? apiKey)
    {
        Assert.Throws<ArgumentException>(() => new PangramTextClient(apiKey!));
    }

    [Fact]
    public void Constructor_WithHttpClient_DoesNotThrow()
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("x-api-key", "test-key");
        var client = new PangramTextClient(httpClient);
        // client does NOT own the HttpClient — no Dispose needed for httpClient here
    }

    [Fact]
    public void Constructor_WithNullHttpClient_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new PangramTextClient((HttpClient)null!));
    }
}