using System.Diagnostics;

namespace Pangram.Sdk.WinForms.Controls;

public abstract class AnalysisControlBase : UserControl
{
    private readonly ConfigService _configService;
    protected readonly HistoryService HistoryService;

    private CancellationTokenSource? _cts;
    private readonly ToolTip _toolTip = new();

    public RichTextBox InputTextBox { get; private set; } = null!;

    protected Button AnalyseButton { get; private set; } = null!;
    protected Button CancelButton { get; private set; } = null!;
    protected Button OpenFileButton { get; private set; } = null!;
    protected Label StatusLabel { get; private set; } = null!;

    protected Panel ResultsContainer { get; private set; } = null!;
    protected FlowLayoutPanel OptionsPanel { get; private set; } = null!;

    protected abstract string MethodName { get; }

    protected abstract Task<(string responseJson, Action renderAction)> RunAnalysisAsync(
        PangramTextClient client, string text, CancellationToken ct);

    protected AnalysisControlBase(ConfigService configService, HistoryService historyService)
    {
        _configService = configService;
        HistoryService = historyService;
        BuildLayout();
    }

    private void BuildLayout()
    {
        Dock = DockStyle.Fill;

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterWidth = 5
        };
        HandleCreated += (_, _) =>
        {
            split.Panel1MinSize = 140;
            split.Panel2MinSize = 80;
            try
            {
                split.SplitterDistance = 210;
            }
            catch
            {
            }
        };

        var inputTable = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 4,
            ColumnCount = 1,
            Padding = new Padding(4, 4, 4, 0)
        };
        inputTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        inputTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        inputTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        inputTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 4)
        };

        OpenFileButton = new Button
        {
            Text = "📂  Open File…",
            AutoSize = true,
            Height = 28,
            Padding = new Padding(6, 0, 6, 0),
            FlatStyle = FlatStyle.System
        };

        if (!PandocExtractor.IsPandocAvailable)
        {
            OpenFileButton.Enabled = false;
            _toolTip.SetToolTip(OpenFileButton,
                "Pandoc not found on PATH — install from https://pandoc.org to enable file import.");
        }

        StatusLabel = new Label
        {
            Text = "Ready.",
            AutoSize = true,
            Padding = new Padding(8, 6, 0, 0),
            ForeColor = Color.Gray
        };

        toolbar.Controls.Add(OpenFileButton);
        toolbar.Controls.Add(StatusLabel);
        inputTable.Controls.Add(toolbar, 0, 0);

        InputTextBox = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ScrollBars = RichTextBoxScrollBars.Vertical,
            Font = new Font("Segoe UI", 10f),
            AcceptsTab = false,
            DetectUrls = false
        };
        inputTable.Controls.Add(InputTextBox, 0, 1);

        var buttonRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            Padding = new Padding(0, 4, 0, 2)
        };

        AnalyseButton = new Button
        {
            Text = "▶  Analyse",
            AutoSize = true,
            Height = 28,
            Padding = new Padding(10, 0, 10, 0),
            FlatStyle = FlatStyle.System
        };

        CancelButton = new Button
        {
            Text = "✕  Cancel",
            AutoSize = true,
            Height = 28,
            Padding = new Padding(8, 0, 8, 0),
            FlatStyle = FlatStyle.System,
            Enabled = false
        };

        buttonRow.Controls.Add(AnalyseButton);
        buttonRow.Controls.Add(CancelButton);
        inputTable.Controls.Add(buttonRow, 0, 2);

        OptionsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 4)
        };
        inputTable.Controls.Add(OptionsPanel, 0, 3);

        split.Panel1.Controls.Add(inputTable);

        ResultsContainer = new Panel { Dock = DockStyle.Fill };
        split.Panel2.Controls.Add(ResultsContainer);

        Controls.Add(split);

        AnalyseButton.Click += AnalyseButton_Click;
        CancelButton.Click += (_, _) => _cts?.Cancel();
        OpenFileButton.Click += OpenFileButton_Click;
    }

    private async void AnalyseButton_Click(object? sender, EventArgs e)
    {
        var text = InputTextBox.Text.Trim();
        if (string.IsNullOrEmpty(text))
        {
            SetStatus("Please enter some text to analyse.", error: true);
            return;
        }

        var apiKey = _configService.Config.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            SetStatus("No API key configured — go to File → Settings.", error: true);
            return;
        }

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        SetBusy(true);
        try
        {
            using var client = new PangramTextClient(apiKey);
            var (responseJson, render) = await RunAnalysisAsync(client, text, _cts.Token);
            render();

            await HistoryService.AppendAsync(new HistoryEntry
            {
                Method = MethodName,
                InputText = text,
                ResponseJson = responseJson
            });

            SetStatus($"Completed at {DateTime.Now:HH:mm:ss}");
        }
        catch (OperationCanceledException)
        {
            SetStatus("Cancelled.");
        }
        catch (PangramApiException ex)
        {
            SetStatus($"API error: {ex.Message}", error: true);
        }
        catch (Exception ex)
        {
            SetStatus($"Error: {ex.Message}", error: true);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void OpenFileButton_Click(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Open document for text extraction via Pandoc",
            Filter = "Supported documents|*.doc;*.docx;*.pdf;*.odt;*.rtf;*.txt;*.md|All files|*.*"
        };

        if (ofd.ShowDialog() != DialogResult.OK) return;

        SetStatus($"Extracting text from {Path.GetFileName(ofd.FileName)}…");
        OpenFileButton.Enabled = false;

        try
        {
            var text = await PandocExtractor.ExtractTextAsync(ofd.FileName);
            InputTextBox.Text = text.Trim();
            SetStatus($"Loaded: {Path.GetFileName(ofd.FileName)}");
        }
        catch (Exception ex)
        {
            SetStatus($"Extraction failed: {ex.Message}", error: true);
        }
        finally
        {
            OpenFileButton.Enabled = PandocExtractor.IsPandocAvailable;
        }
    }

    protected void SetBusy(bool busy)
    {
        AnalyseButton.Enabled = !busy;
        CancelButton.Enabled = busy;
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
    }

    protected void SetStatus(string msg, bool error = false)
    {
        if (InvokeRequired)
        {
            Invoke(() => SetStatus(msg, error));
            return;
        }

        StatusLabel.Text = msg;
        StatusLabel.ForeColor = error ? Color.Crimson : Color.DarkGreen;
    }

    protected static Color PredictionColor(string predictionShort) =>
        predictionShort.ToUpperInvariant() switch
        {
            "AI" => Color.FromArgb(192, 57, 43),
            "AI-ASSISTED" => Color.FromArgb(211, 84, 0),
            "MIXED" => Color.FromArgb(243, 156, 18),
            "HUMAN" => Color.FromArgb(39, 174, 96),
            _ => Color.Gray
        };

    protected static Color PredictionForeColor(string predictionShort) =>
        predictionShort.ToUpperInvariant() == "MIXED" ? Color.Black : Color.White;

    protected static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
        }
    }

    protected static JsonSerializerOptions JsonPretty { get; } = new() { WriteIndented = true };
}