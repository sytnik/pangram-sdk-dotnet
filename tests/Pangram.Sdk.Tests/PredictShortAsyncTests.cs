namespace Pangram.Sdk.Tests;

public sealed class PredictShortAsyncTests
{
    private const string ValidResponse = """
                                         {
                                             "text": "Hello world",
                                             "ai_likelihood": 0.92,
                                             "prediction": "Likely AI-Generated"
                                         }
                                         """;

    [Fact]
    public async Task PredictShortAsync_ValidText_ReturnsMappedResponse()
    {
        using var client = ClientFactory.Create(ValidResponse);

        var result = await client.PredictShortAsync("Hello world", TestContext.Current.CancellationToken);

        Assert.Equal("Hello world", result.Text);
        Assert.Equal(0.92f, result.AiLikelihood, precision: 2);
        Assert.Equal("Likely AI-Generated", result.Prediction);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task PredictShortAsync_NullOrEmptyText_ThrowsArgumentException(string? text)
    {
        using var client = ClientFactory.Create(ValidResponse);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.PredictShortAsync(text!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task PredictShortAsync_ApiReturns500_ThrowsPangramApiException()
    {
        using var client = ClientFactory.Create("Internal Server Error", HttpStatusCode.InternalServerError);

        var ex = await Assert.ThrowsAsync<PangramApiException>(() =>
            client.PredictShortAsync("Hello", TestContext.Current.CancellationToken));

        Assert.Equal(500, ex.StatusCode);
    }

    [Fact]
    public async Task PredictShortAsync_ApiReturnsErrorField_ThrowsPangramApiException()
    {
        using var client = ClientFactory.Create("""{"error": "quota exceeded"}""");

        var ex = await Assert.ThrowsAsync<PangramApiException>(() =>
            client.PredictShortAsync("Hello", TestContext.Current.CancellationToken));

        Assert.Contains("quota exceeded", ex.Message);
    }
}