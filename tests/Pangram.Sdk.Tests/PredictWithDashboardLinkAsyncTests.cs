namespace Pangram.Sdk.Tests;

public sealed class PredictWithDashboardLinkAsyncTests
{
    private const string ResponseWithLink = """
                                            {
                                                "text": "Sample text",
                                                "version": "3.0",
                                                "headline": "AI",
                                                "prediction": "AI generated",
                                                "prediction_short": "AI",
                                                "fraction_ai": 1.0,
                                                "fraction_ai_assisted": 0.0,
                                                "fraction_human": 0.0,
                                                "num_ai_segments": 1,
                                                "num_ai_assisted_segments": 0,
                                                "num_human_segments": 0,
                                                "dashboard_link": "https://dashboard.pangram.com/result/xyz",
                                                "windows": []
                                            }
                                            """;

    [Fact]
    public async Task PredictWithDashboardLinkAsync_ReturnsDashboardLink()
    {
        using var client = ClientFactory.Create(ResponseWithLink);

        var result = await client.PredictWithDashboardLinkAsync("Sample text", TestContext.Current.CancellationToken);

        Assert.Equal("https://dashboard.pangram.com/result/xyz", result.DashboardLink);
    }

    [Fact]
    public async Task PredictWithDashboardLinkAsync_ReturnsSameModelAsPredictAsync()
    {
        using var client = ClientFactory.Create(ResponseWithLink);

        var result = await client.PredictWithDashboardLinkAsync("Sample text", TestContext.Current.CancellationToken);

        Assert.Equal("AI", result.PredictionShort);
        Assert.Equal(1.0f, result.FractionAi, precision: 2);
    }
}