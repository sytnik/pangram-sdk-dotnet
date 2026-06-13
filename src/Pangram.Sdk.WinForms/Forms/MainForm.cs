using Pangram.Sdk.WinForms.Controls;

namespace Pangram.Sdk.WinForms.Forms;

public sealed class MainForm : Form
{
    private readonly ConfigService _configService = new();
    private readonly HistoryService _historyService = new();

    private readonly PredictShortControl _predictShortCtrl;
    private readonly PredictControl _predictV3Ctrl;
    private readonly PredictControl _predictDashboardCtrl;
    private readonly PlagiarismControl _plagiarismCtrl;
    private readonly HistoryControl _historyCtrl;

    private readonly TabControl _tabs;
    private readonly ToolStripStatusLabel _statusLabel;
    private readonly ToolStripProgressBar _progressBar;

    public MainForm()
    {
        _configService.Load();
        _historyService.Load();

        Text = "Pangram Text Analyser";
        Size = new Size(1150, 750);
        MinimumSize = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9.75f);

        var menu = new MenuStrip();
        var fileMenu = new ToolStripMenuItem("&File");
        var settingsItem = new ToolStripMenuItem("⚙  &Settings…", null, (_, _) => OpenSettings());
        var exitItem = new ToolStripMenuItem("E&xit", null, (_, _) => Close());
        fileMenu.DropDownItems.Add(settingsItem);
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(exitItem);

        var helpMenu = new ToolStripMenuItem("&Help");
        var aboutItem = new ToolStripMenuItem("&About", null, (_, _) =>
            MessageBox.Show(
                "Pangram Text Analyser\nUnofficial .NET SDK demo app\n\nPowered by Pangram Labs API",
                "About", MessageBoxButtons.OK, MessageBoxIcon.Information));
        helpMenu.DropDownItems.Add(aboutItem);

        menu.Items.Add(fileMenu);
        menu.Items.Add(helpMenu);
        MainMenuStrip = menu;

        var statusStrip = new StatusStrip();
        _statusLabel = new ToolStripStatusLabel("Ready.")
        {
            Spring = true,
            TextAlign = ContentAlignment.MiddleLeft
        };
        _progressBar = new ToolStripProgressBar
        {
            Visible = false,
            Width = 120,
            Style = ProgressBarStyle.Marquee,
            MarqueeAnimationSpeed = 30
        };
        statusStrip.Items.Add(_statusLabel);
        statusStrip.Items.Add(_progressBar);

        _tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.75f) };

        _predictShortCtrl = new PredictShortControl(_configService, _historyService);
        _predictV3Ctrl = new PredictControl(_configService, _historyService, withDashboard: false);
        _predictDashboardCtrl = new PredictControl(_configService, _historyService, withDashboard: true);
        _plagiarismCtrl = new PlagiarismControl(_configService, _historyService);
        _historyCtrl = new HistoryControl(_historyService);

        _tabs.TabPages.Add(MakeTab("Predict Short", _predictShortCtrl));
        _tabs.TabPages.Add(MakeTab("Predict V3", _predictV3Ctrl));
        _tabs.TabPages.Add(MakeTab("Predict + Dashboard Link", _predictDashboardCtrl));
        _tabs.TabPages.Add(MakeTab("Check Plagiarism", _plagiarismCtrl));
        _tabs.TabPages.Add(MakeTab("History", _historyCtrl));

        _historyCtrl.RerunRequested += HistoryCtrl_RerunRequested;

        Controls.Add(_tabs);
        Controls.Add(menu);
        Controls.Add(statusStrip);

        if (string.IsNullOrWhiteSpace(_configService.Config.ApiKey))
            statusStrip.InvokeIfRequired(() =>
                _statusLabel.Text = "⚠  No API key configured — go to File → Settings.");
    }

    private static TabPage MakeTab(string title, Control control)
    {
        var page = new TabPage(title) { UseVisualStyleBackColor = true };
        control.Dock = DockStyle.Fill;
        page.Controls.Add(control);
        return page;
    }

    private void OpenSettings()
    {
        using var dlg = new SettingsForm(_configService);
        if (dlg.ShowDialog(this) == DialogResult.OK)
            _statusLabel.Text = "Settings saved.";
    }

    private void HistoryCtrl_RerunRequested(object? sender, HistoryEntry entry)
    {
        AnalysisControlBase? target = entry.Method switch
        {
            "PredictShort" => _predictShortCtrl,
            "PredictV3" => _predictV3Ctrl,
            "PredictWithDashboard" => _predictDashboardCtrl,
            "CheckPlagiarism" => _plagiarismCtrl,
            _ => null
        };

        if (target == null) return;

        var page = _tabs.TabPages.Cast<TabPage>()
            .FirstOrDefault(p => p.Controls.Contains(target));

        if (page != null)
            _tabs.SelectedTab = page;

        target.InputTextBox.Text = entry.InputText;
        _statusLabel.Text = $"Re-run loaded from history entry [{entry.Timestamp.LocalDateTime:HH:mm:ss}].";
    }
}

internal static class ControlExtensions
{
    internal static void InvokeIfRequired(this Control ctrl, Action action)
    {
        if (ctrl.InvokeRequired) ctrl.Invoke(action);
        else action();
    }
}