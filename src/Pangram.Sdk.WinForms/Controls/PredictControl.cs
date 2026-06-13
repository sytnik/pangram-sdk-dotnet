namespace Pangram.Sdk.WinForms.Controls;

public sealed class PredictControl : AnalysisControlBase
{
    private readonly bool _withDashboard;
    private readonly CheckBox? _dashboardCheckBox;

    protected override string MethodName =>
        _withDashboard ? "PredictWithDashboard" : "PredictV3";

    private readonly Label _headlineValue;
    private readonly Label _predShortBadge;
    private readonly Label _predictionValue;

    private readonly ProgressBar _aiBar;
    private readonly Label _aiPct;
    private readonly ProgressBar _assistedBar;
    private readonly Label _assistedPct;
    private readonly ProgressBar _humanBar;
    private readonly Label _humanPct;

    private readonly Label _segmentsLabel;

    private readonly Panel _dashboardPanel;
    private readonly LinkLabel _dashboardLink;

    private readonly DataGridView _windowsGrid;

    public PredictControl(ConfigService configService, HistoryService historyService, bool withDashboard = false)
        : base(configService, historyService)
    {
        _withDashboard = withDashboard;

        if (!withDashboard)
        {
            _dashboardCheckBox = new CheckBox
            {
                Text = "Request public dashboard link",
                AutoSize = true,
                Padding = new Padding(0, 4, 0, 0)
            };
            OptionsPanel.Controls.Add(_dashboardCheckBox);
        }

        var summaryBox = new GroupBox
        {
            Text = "Classification Summary",
            Dock = DockStyle.Top,
            Height = 200,
            Font = new Font("Segoe UI", 9.75f),
            Padding = new Padding(8)
        };

        const int LabelX = 8;
        const int LabelW = 110;
        const int BarX = 122;
        const int BarW = 280;
        const int PctX = 408;

        var headlineLabel = new Label { Text = "Headline:", AutoSize = true, Location = new Point(LabelX, 24) };
        _headlineValue = new Label
        {
            Text = "—", AutoSize = true, Location = new Point(BarX, 24),
            Font = new Font("Segoe UI", 9.75f, FontStyle.Bold), MaximumSize = new Size(500, 0)
        };

        var predShortLabel = new Label { Text = "Classification:", AutoSize = true, Location = new Point(LabelX, 50) };
        _predShortBadge = new Label
        {
            Text = "—",
            AutoSize = false,
            Size = new Size(100, 22),
            Location = new Point(BarX, 47),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.LightGray,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            BorderStyle = BorderStyle.FixedSingle
        };
        _predictionValue = new Label { Text = string.Empty, AutoSize = true, Location = new Point(BarX + 108, 50) };

        static Label MakeBarLabel(string text, int y) => new()
        {
            Text = text, AutoSize = false, Width = LabelW, Location = new Point(LabelX, y),
            TextAlign = ContentAlignment.MiddleLeft
        };

        static ProgressBar MakeBar(int y) => new()
            { Location = new Point(BarX, y), Size = new Size(BarW, 18), Minimum = 0, Maximum = 100 };

        static Label MakePct(int y) => new()
            { Text = "—", AutoSize = true, Location = new Point(PctX, y + 1) };

        _aiBar = MakeBar(83);
        _aiPct = MakePct(83);
        _assistedBar = MakeBar(111);
        _assistedPct = MakePct(111);
        _humanBar = MakeBar(139);
        _humanPct = MakePct(139);

        _segmentsLabel = new Label { Text = string.Empty, AutoSize = true, Location = new Point(LabelX, 168) };

        summaryBox.Controls.AddRange([
            headlineLabel, _headlineValue,
            predShortLabel, _predShortBadge, _predictionValue,
            MakeBarLabel("AI:", 83), _aiBar, _aiPct,
            MakeBarLabel("AI-Assisted:", 111), _assistedBar, _assistedPct,
            MakeBarLabel("Human:", 139), _humanBar, _humanPct,
            _segmentsLabel
        ]);

        _dashboardPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            Visible = false,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(232, 245, 253),
            Padding = new Padding(8, 6, 8, 6)
        };

        var dashLabel = new Label
        {
            Text = "Dashboard:",
            AutoSize = true,
            Location = new Point(8, 14),
            Font = new Font("Segoe UI", 9.75f, FontStyle.Bold)
        };

        _dashboardLink = new LinkLabel
        {
            Text = string.Empty,
            AutoSize = true,
            Location = new Point(100, 14),
            Font = new Font("Segoe UI", 9.75f),
            MaximumSize = new Size(700, 0)
        };
        _dashboardLink.LinkClicked += (_, e) =>
        {
            if (e.Link?.LinkData is string url) OpenUrl(url);
        };

        _dashboardPanel.Controls.AddRange([dashLabel, _dashboardLink]);

        var windowsBox = new GroupBox
        {
            Text = "Analysis Windows",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9.75f),
            Padding = new Padding(4)
        };

        _windowsGrid = BuildWindowsGrid();
        windowsBox.Controls.Add(_windowsGrid);

        ResultsContainer.Controls.Add(windowsBox);
        ResultsContainer.Controls.Add(_dashboardPanel);
        ResultsContainer.Controls.Add(summaryBox);
    }

    private static DataGridView BuildWindowsGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            RowHeadersVisible = false,
            BackgroundColor = SystemColors.Window,
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 9f)
        };

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Label", HeaderText = "Label", Width = 160,
            DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.False }
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "Score", HeaderText = "AI Score", Width = 80 });
        grid.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "Confidence", HeaderText = "Confidence", Width = 90 });
        grid.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "Words", HeaderText = "Words", Width = 60 });
        grid.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "Chars", HeaderText = "Chars", Width = 70 });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Text", HeaderText = "Text",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.True }
        });

        grid.RowPrePaint += (_, e) =>
        {
            if (e.RowIndex < 0 || e.RowIndex >= grid.Rows.Count) return;
            var label = grid.Rows[e.RowIndex].Cells["Label"].Value?.ToString() ?? string.Empty;
            var bg = label.Contains("AI", StringComparison.OrdinalIgnoreCase)
                     && !label.Contains("Human", StringComparison.OrdinalIgnoreCase)
                ? Color.FromArgb(255, 235, 235)
                : label.Contains("Human", StringComparison.OrdinalIgnoreCase)
                    ? Color.FromArgb(235, 255, 240)
                    : Color.FromArgb(255, 248, 225);
            grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = bg;
        };

        return grid;
    }

    protected override async Task<(string responseJson, Action renderAction)> RunAnalysisAsync(
        PangramTextClient client, string text, CancellationToken ct)
    {
        var requestLink = _withDashboard || (_dashboardCheckBox?.Checked ?? false);
        var response = await client.PredictAsync(text, requestLink, ct).ConfigureAwait(false);
        var json = JsonSerializer.Serialize(response, JsonPretty);
        return (json, () => RenderResponse(response));
    }

    private void RenderResponse(PredictResponse r)
    {
        _headlineValue.Text = r.Headline;

        _predShortBadge.Text = r.PredictionShort;
        _predShortBadge.BackColor = PredictionColor(r.PredictionShort);
        _predShortBadge.ForeColor = PredictionForeColor(r.PredictionShort);

        _predictionValue.Text = r.Prediction;

        var aiPct = (int)(r.FractionAi * 100);
        var assistedPct = (int)(r.FractionAiAssisted * 100);
        var humanPct = (int)(r.FractionHuman * 100);

        _aiBar.Value = Math.Clamp(aiPct, 0, 100);
        _aiPct.Text = $"{aiPct}%";
        _assistedBar.Value = Math.Clamp(assistedPct, 0, 100);
        _assistedPct.Text = $"{assistedPct}%";
        _humanBar.Value = Math.Clamp(humanPct, 0, 100);
        _humanPct.Text = $"{humanPct}%";

        _segmentsLabel.Text =
            $"Segments — AI: {r.NumAiSegments}   AI-Assisted: {r.NumAiAssistedSegments}   Human: {r.NumHumanSegments}";

        if (!string.IsNullOrWhiteSpace(r.DashboardLink))
        {
            _dashboardLink.Text = r.DashboardLink;
            _dashboardLink.Links.Clear();
            _dashboardLink.Links.Add(0, r.DashboardLink.Length, r.DashboardLink);
            _dashboardPanel.Visible = true;
        }
        else
        {
            _dashboardPanel.Visible = false;
        }

        _windowsGrid.Rows.Clear();
        foreach (var w in r.Windows)
        {
            _windowsGrid.Rows.Add(
                w.Label,
                w.AiAssistanceScore.ToString("F3"),
                w.Confidence,
                w.WordCount,
                w.EndIndex - w.StartIndex,
                w.Text);
        }
    }
}