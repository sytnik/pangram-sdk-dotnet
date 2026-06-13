namespace Pangram.Sdk.WinForms.Controls;

public sealed class HistoryControl : UserControl
{
    private readonly HistoryService _historyService;

    public event EventHandler<HistoryEntry>? RerunRequested;

    private ListBox _listBox = null!;
    private RichTextBox _inputView = null!;
    private RichTextBox _jsonView = null!;
    private Button _rerunButton = null!;
    private Button _copyInputButton = null!;
    private Button _copyJsonButton = null!;

    private HistoryEntry? _selected;

    public HistoryControl(HistoryService historyService)
    {
        _historyService = historyService;
        Dock = DockStyle.Fill;
        BuildLayout();
        _historyService.HistoryChanged += (_, _) => SafeRefresh();
        SafeRefresh();
    }

    private void BuildLayout()
    {
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterWidth = 5
        };
        HandleCreated += (_, _) =>
        {
            split.Panel1MinSize = 200;
            split.Panel2MinSize = 200;
            try
            {
                split.SplitterDistance = 310;
            }
            catch
            {
            }
        };

        var leftTable = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            Padding = new Padding(4)
        };
        leftTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        leftTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _listBox = new ListBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9f),
            IntegralHeight = false
        };
        _listBox.SelectedIndexChanged += ListBox_SelectedIndexChanged;
        leftTable.Controls.Add(_listBox, 0, 0);

        var clearButton = new Button
        {
            Text = "🗑  Clear History",
            AutoSize = true,
            Height = 28,
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.System
        };
        clearButton.Click += (_, _) =>
        {
            if (MessageBox.Show("Clear all history?", "Confirm", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes)
                _historyService.Clear();
        };
        leftTable.Controls.Add(clearButton, 0, 1);
        split.Panel1.Controls.Add(leftTable);

        var rightTable = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            Padding = new Padding(4)
        };
        rightTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightTable.RowStyles.Add(new RowStyle(SizeType.Percent, 35f));
        rightTable.RowStyles.Add(new RowStyle(SizeType.Percent, 65f));

        var buttonFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 4)
        };

        _rerunButton = new Button
            { Text = "▶  Re-run", AutoSize = true, Height = 28, FlatStyle = FlatStyle.System, Enabled = false };
        _copyInputButton = new Button
            { Text = "📋  Copy Input", AutoSize = true, Height = 28, FlatStyle = FlatStyle.System, Enabled = false };
        _copyJsonButton = new Button
            { Text = "📋  Copy JSON", AutoSize = true, Height = 28, FlatStyle = FlatStyle.System, Enabled = false };

        _rerunButton.Click += (_, _) =>
        {
            if (_selected != null) RerunRequested?.Invoke(this, _selected);
        };
        _copyInputButton.Click += (_, _) =>
        {
            if (_selected != null) Clipboard.SetText(_selected.InputText);
        };
        _copyJsonButton.Click += (_, _) =>
        {
            if (_selected != null) Clipboard.SetText(_selected.ResponseJson);
        };

        buttonFlow.Controls.AddRange([_rerunButton, _copyInputButton, _copyJsonButton]);
        rightTable.Controls.Add(buttonFlow, 0, 0);

        var inputBox = new GroupBox { Text = "Input Text", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.75f) };
        _inputView = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BackColor = SystemColors.Window,
            Font = new Font("Segoe UI", 9f),
            ScrollBars = RichTextBoxScrollBars.Vertical,
            BorderStyle = BorderStyle.None
        };
        inputBox.Controls.Add(_inputView);
        rightTable.Controls.Add(inputBox, 0, 1);

        var jsonBox = new GroupBox
            { Text = "Response JSON", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.75f) };
        _jsonView = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BackColor = SystemColors.Window,
            Font = new Font("Courier New", 9f),
            ScrollBars = RichTextBoxScrollBars.Both,
            WordWrap = false,
            BorderStyle = BorderStyle.None
        };
        jsonBox.Controls.Add(_jsonView);
        rightTable.Controls.Add(jsonBox, 0, 2);

        split.Panel2.Controls.Add(rightTable);
        Controls.Add(split);
    }

    private void ListBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        var idx = _listBox.SelectedIndex;
        if (idx < 0 || idx >= _historyService.Entries.Count)
        {
            ClearDetail();
            return;
        }

        _selected = _historyService.Entries[idx];
        _inputView.Text = _selected.InputText;
        _jsonView.Text = _selected.ResponseJson;
        _rerunButton.Enabled = true;
        _copyInputButton.Enabled = true;
        _copyJsonButton.Enabled = true;
    }

    private void ClearDetail()
    {
        _selected = null;
        _inputView.Text = string.Empty;
        _jsonView.Text = string.Empty;
        _rerunButton.Enabled = false;
        _copyInputButton.Enabled = false;
        _copyJsonButton.Enabled = false;
    }

    private void SafeRefresh()
    {
        if (InvokeRequired)
        {
            Invoke(SafeRefresh);
            return;
        }

        RefreshList();
    }

    private void RefreshList()
    {
        _listBox.BeginUpdate();
        _listBox.Items.Clear();
        foreach (var entry in _historyService.Entries)
        {
            var preview = entry.InputText.Length > 60
                ? entry.InputText[..60].Replace('\n', ' ') + "…"
                : entry.InputText.Replace('\n', ' ');
            _listBox.Items.Add($"{entry.Timestamp.LocalDateTime:yyyy-MM-dd HH:mm:ss}  [{entry.Method}]  {preview}");
        }

        _listBox.EndUpdate();

        if (_selected != null)
        {
            var newIdx = _historyService.Entries.TakeWhile(e => e.Id != _selected.Id).Count();
            if (newIdx < _listBox.Items.Count)
                _listBox.SelectedIndex = newIdx;
            else
                ClearDetail();
        }
    }
}