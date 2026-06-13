namespace Pangram.Sdk.WinForms.Controls;

public sealed class PlagiarismControl : AnalysisControlBase
{
    protected override string MethodName => "CheckPlagiarism";

    private readonly Label _detectedBadge;
    private readonly ProgressBar _percentBar;
    private readonly Label _percentValue;
    private readonly Label _sentenceStats;

    private readonly DataGridView _contentGrid;

    public PlagiarismControl(ConfigService configService, HistoryService historyService)
        : base(configService, historyService)
    {
        var summaryBox = new GroupBox
        {
            Text = "Plagiarism Summary",
            Dock = DockStyle.Top,
            Height = 130,
            Font = new Font("Segoe UI", 9.75f),
            Padding = new Padding(8)
        };

        _detectedBadge = new Label
        {
            Text = "—",
            AutoSize = false,
            Size = new Size(230, 36),
            Location = new Point(8, 24),
            Font = new Font("Segoe UI", 13f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.LightGray,
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        var pctLabel = new Label
        {
            Text = "Plagiarised:", AutoSize = false, Width = 100, Location = new Point(8, 76),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _percentBar = new ProgressBar
            { Location = new Point(112, 76), Size = new Size(280, 18), Minimum = 0, Maximum = 100 };
        _percentValue = new Label { Text = "—", AutoSize = true, Location = new Point(398, 78) };

        _sentenceStats = new Label { Text = string.Empty, AutoSize = true, Location = new Point(8, 106) };

        summaryBox.Controls.AddRange([_detectedBadge, pctLabel, _percentBar, _percentValue, _sentenceStats]);

        var contentBox = new GroupBox
        {
            Text = "Plagiarised Content",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9.75f),
            Padding = new Padding(4)
        };

        _contentGrid = BuildContentGrid();
        contentBox.Controls.Add(_contentGrid);

        ResultsContainer.Controls.Add(contentBox);
        ResultsContainer.Controls.Add(summaryBox);
    }

    private DataGridView BuildContentGrid()
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
            Name = "Text", HeaderText = "Plagiarised Text",
            Width = 340,
            DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.True }
        });
        grid.Columns.Add(new DataGridViewLinkColumn
        {
            Name = "Source", HeaderText = "Source URL",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.False }
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "Similarity", HeaderText = "Similarity", Width = 80 });

        grid.CellContentClick += (_, e) =>
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) return;
            if (grid.Columns[e.ColumnIndex].Name != "Source") return;
            var url = grid.Rows[e.RowIndex].Cells["Source"].Value?.ToString();
            if (!string.IsNullOrWhiteSpace(url)) OpenUrl(url);
        };

        return grid;
    }

    protected override async Task<(string responseJson, Action renderAction)> RunAnalysisAsync(
        PangramTextClient client, string text, CancellationToken ct)
    {
        var response = await client.CheckPlagiarismAsync(text, ct).ConfigureAwait(false);
        var json = JsonSerializer.Serialize(response, JsonPretty);
        return (json, () => RenderResponse(response));
    }

    private void RenderResponse(PlagiarismResponse r)
    {
        if (r.PlagiarismDetected)
        {
            _detectedBadge.Text = "⚠  PLAGIARISM DETECTED";
            _detectedBadge.BackColor = Color.FromArgb(192, 57, 43);
        }
        else
        {
            _detectedBadge.Text = "✔  NO PLAGIARISM DETECTED";
            _detectedBadge.BackColor = Color.FromArgb(39, 174, 96);
        }

        var pct = (int)Math.Round(r.PercentPlagiarized);
        _percentBar.Value = Math.Clamp(pct, 0, 100);
        _percentValue.Text = $"{r.PercentPlagiarized:F1}%";
        _sentenceStats.Text =
            $"Sentences checked: {r.TotalSentences}   |   Plagiarised sentences: {r.PlagiarizedSentences.Count}";

        _contentGrid.Rows.Clear();
        foreach (var item in r.PlagiarizedContent)
        {
            _contentGrid.Rows.Add(
                item.Text,
                item.Source,
                item.Similarity.HasValue ? $"{item.Similarity.Value:P0}" : "—");
        }
    }
}