namespace Pangram.Sdk.WinForms.Controls;

public sealed class PredictShortControl : AnalysisControlBase
{
    protected override string MethodName => "PredictShort";

    private readonly Label _predictionBadge;
    private readonly Label _scoreLabel;
    private readonly ProgressBar _likelihoodBar;
    private readonly Label _percentLabel;

    public PredictShortControl(ConfigService configService, HistoryService historyService)
        : base(configService, historyService)
    {
        var gb = new GroupBox
        {
            Text = "Result",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9.75f)
        };

        _predictionBadge = new Label
        {
            Text = "—",
            AutoSize = false,
            Size = new Size(260, 48),
            Location = new Point(16, 28),
            Font = new Font("Segoe UI", 18f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.LightGray,
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        _scoreLabel = new Label
        {
            Text = "AI Likelihood:",
            AutoSize = true,
            Location = new Point(16, 90),
            Font = new Font("Segoe UI", 9.75f)
        };

        _likelihoodBar = new ProgressBar
        {
            Location = new Point(16, 112),
            Size = new Size(340, 22),
            Minimum = 0,
            Maximum = 100,
            Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
        };

        _percentLabel = new Label
        {
            Text = "—",
            AutoSize = true,
            Location = new Point(364, 115),
            Font = new Font("Segoe UI", 9.75f, FontStyle.Bold)
        };

        gb.Controls.AddRange([_predictionBadge, _scoreLabel, _likelihoodBar, _percentLabel]);
        ResultsContainer.Controls.Add(gb);
    }

    protected override async Task<(string responseJson, Action renderAction)> RunAnalysisAsync(
        PangramTextClient client, string text, CancellationToken ct)
    {
        var response = await client.PredictShortAsync(text, ct).ConfigureAwait(false);
        var json = JsonSerializer.Serialize(response, JsonPretty);

        return (json, () =>
        {
            var pct = (int)(response.AiLikelihood * 100);
            var shortLabel = response.Prediction;
            var bgColor = PredictionColor(shortLabel.Contains("AI", StringComparison.OrdinalIgnoreCase)
                ? "AI"
                : shortLabel.Contains("Human", StringComparison.OrdinalIgnoreCase)
                    ? "HUMAN"
                    : "MIXED");

            _predictionBadge.Text = response.Prediction;
            _predictionBadge.BackColor = bgColor;
            _predictionBadge.ForeColor = Color.White;

            _scoreLabel.Text = $"AI Likelihood:  {pct}%";
            _likelihoodBar.Value = Math.Clamp(pct, 0, 100);
            _percentLabel.Text = $"{pct}%";
        });
    }
}