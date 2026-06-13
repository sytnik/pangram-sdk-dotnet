namespace Pangram.Sdk.Tests;

public sealed class CheckPlagiarismAsyncTests
{
    private const string ValidResponse = """
                                         {
                                             "text": "To be or not to be, that is the question.",
                                             "plagiarism_detected": true,
                                             "plagiarized_content": [
                                                 {
                                                     "text": "To be or not to be",
                                                     "source": "https://example.com/shakespeare",
                                                     "similarity": 0.98
                                                 }
                                             ],
                                             "total_sentences": 1,
                                             "plagiarized_sentences": ["To be or not to be, that is the question."],
                                             "percent_plagiarized": 100.0
                                         }
                                         """;

    [Fact]
    public async Task CheckPlagiarismAsync_ValidText_ReturnsMappedResponse()
    {
        using var client = ClientFactory.Create(ValidResponse);

        var result = await client.CheckPlagiarismAsync("To be or not to be, that is the question.", TestContext.Current.CancellationToken);

        Assert.True(result.PlagiarismDetected);
        Assert.Equal(1, result.TotalSentences);
        Assert.Equal(100.0f, result.PercentPlagiarized, precision: 1);
        Assert.Single(result.PlagiarizedSentences);
        Assert.Equal("To be or not to be, that is the question.", result.PlagiarizedSentences[0]);
    }

    [Fact]
    public async Task CheckPlagiarismAsync_PlagiarizedContent_AreMapped()
    {
        using var client = ClientFactory.Create(ValidResponse);

        var result = await client.CheckPlagiarismAsync("To be or not to be, that is the question.", TestContext.Current.CancellationToken);

        Assert.Single(result.PlagiarizedContent);
        var match = result.PlagiarizedContent[0];
        Assert.Equal("To be or not to be", match.Text);
        Assert.Equal("https://example.com/shakespeare", match.Source);
        Assert.Equal(0.98f, match.Similarity!.Value, precision: 2);
    }

    [Fact]
    public async Task CheckPlagiarismAsync_NoPlagiarism_ReturnsFalse()
    {
        const string noPlagiarismResponse = """
                                            {
                                                "text": "This is entirely original content.",
                                                "plagiarism_detected": false,
                                                "plagiarized_content": [],
                                                "total_sentences": 1,
                                                "plagiarized_sentences": [],
                                                "percent_plagiarized": 0.0
                                            }
                                            """;
        using var client = ClientFactory.Create(noPlagiarismResponse);

        var result = await client.CheckPlagiarismAsync("This is entirely original content.", TestContext.Current.CancellationToken);

        Assert.False(result.PlagiarismDetected);
        Assert.Empty(result.PlagiarizedContent);
        Assert.Equal(0.0f, result.PercentPlagiarized, precision: 1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task CheckPlagiarismAsync_NullOrEmptyText_ThrowsArgumentException(string? text)
    {
        using var client = ClientFactory.Create(ValidResponse);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.CheckPlagiarismAsync(text!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CheckPlagiarismAsync_ApiReturns429_ThrowsPangramApiException()
    {
        using var client = ClientFactory.Create("Too Many Requests", HttpStatusCode.TooManyRequests);

        var ex = await Assert.ThrowsAsync<PangramApiException>(() =>
            client.CheckPlagiarismAsync("Some text.", TestContext.Current.CancellationToken));

        Assert.Equal(429, ex.StatusCode);
    }

    [Fact]
    public async Task CheckPlagiarismAsync_ApiReturnsErrorField_ThrowsPangramApiException()
    {
        using var client = ClientFactory.Create("""{"error": "service unavailable"}""");

        var ex = await Assert.ThrowsAsync<PangramApiException>(() =>
            client.CheckPlagiarismAsync("Some text.", TestContext.Current.CancellationToken));

        Assert.Contains("service unavailable", ex.Message);
    }
}