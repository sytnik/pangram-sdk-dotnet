namespace Pangram.Sdk.WinForms.Forms;

public sealed class SettingsForm : Form
{
    private readonly ConfigService _configService;
    private readonly TextBox _apiKeyBox;

    public SettingsForm(ConfigService configService)
    {
        _configService = configService;

        Text = "Settings";
        Size = new Size(520, 220);
        MinimumSize = new Size(420, 200);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Font = new Font("Segoe UI", 9.75f);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            RowCount = 4,
            ColumnCount = 2
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var keyLabel = new Label { Text = "API Key:", AutoSize = true, Padding = new Padding(0, 6, 8, 0) };
        _apiKeyBox = new TextBox
        {
            Dock = DockStyle.Fill,
            UseSystemPasswordChar = true,
            Font = new Font("Consolas", 10f),
            Text = configService.Config.ApiKey
        };

        layout.Controls.Add(keyLabel, 0, 0);
        layout.Controls.Add(_apiKeyBox, 1, 0);

        var showCheckBox = new CheckBox { Text = "Show key", AutoSize = true, Padding = new Padding(0, 4, 0, 0) };
        showCheckBox.CheckedChanged += (_, _) =>
            _apiKeyBox.UseSystemPasswordChar = !showCheckBox.Checked;
        layout.SetColumnSpan(showCheckBox, 2);
        layout.Controls.Add(showCheckBox, 0, 1);

        var hint = new Label
        {
            Text = $"Stored in:  {ConfigService.SettingsPath}",
            AutoSize = true,
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 8.5f),
            Padding = new Padding(0, 4, 0, 0)
        };
        layout.SetColumnSpan(hint, 2);
        layout.Controls.Add(hint, 0, 2);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 8, 0, 0),
            AutoSize = true
        };

        var cancelBtn = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            AutoSize = true,
            Height = 28,
            Padding = new Padding(10, 0, 10, 0),
            FlatStyle = FlatStyle.System
        };

        var saveBtn = new Button
        {
            Text = "Save",
            DialogResult = DialogResult.OK,
            AutoSize = true,
            Height = 28,
            Padding = new Padding(10, 0, 10, 0),
            FlatStyle = FlatStyle.System
        };
        saveBtn.Click += SaveButton_Click;

        buttonPanel.Controls.Add(cancelBtn);
        buttonPanel.Controls.Add(saveBtn);
        layout.SetColumnSpan(buttonPanel, 2);
        layout.Controls.Add(buttonPanel, 0, 3);

        Controls.Add(layout);
        AcceptButton = saveBtn;
        CancelButton = cancelBtn;
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        _configService.Config.ApiKey = _apiKeyBox.Text.Trim();
        _configService.Save();
    }
}