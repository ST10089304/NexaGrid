using NexaGrid.Desktop.Services;
using NexaGrid.Desktop.Theme;
using NexaGrid.Shared.DTOs;
using NexaGrid.Shared.Enums;

namespace NexaGrid.Desktop.Forms;

public class SensorWorkspaceForm : Form
{
    private readonly ApiClient _apiClient;
    private readonly SensorApiService _sensorService;

    private readonly TextBox _deviceNameTextBox;
    private readonly TextBox _macAddressTextBox;
    private readonly TextBox _manufacturerTextBox;
    private readonly TextBox _sensorNameTextBox;
    private readonly TextBox _identifierTextBox;
    private readonly TextBox _locationTextBox;
    private readonly ComboBox _categoryComboBox;

    private readonly Button _registerButton;
    private readonly Button _refreshButton;
    private readonly DataGridView _sensorGrid;
    private readonly Label _statusLabel;
    private readonly Label _recordCountLabel;

    public SensorWorkspaceForm()
    {
        AutoScaleMode =
            AutoScaleMode.Dpi;

        Text = "NexaGrid / Sensor Workspace";
        StartPosition =
            FormStartPosition.CenterParent;
        MinimumSize =
            new Size(1200, 760);
        Size =
            new Size(1380, 820);
        BackColor =
            AppPalette.Background;
        ForeColor =
            AppPalette.Black;
        Font =
            AppFonts.Body;

        _apiClient =
            new ApiClient();

        _sensorService =
            new SensorApiService(
                _apiClient);

        _deviceNameTextBox =
            CreateTextBox();

        _macAddressTextBox =
            CreateTextBox();

        _manufacturerTextBox =
            CreateTextBox();

        _sensorNameTextBox =
            CreateTextBox();

        _identifierTextBox =
            CreateTextBox();

        _locationTextBox =
            CreateTextBox();

        _categoryComboBox =
            CreateCategoryComboBox();

        _registerButton =
            CreatePrimaryButton(
                "REGISTER SENSOR");

        _refreshButton =
            CreateSecondaryButton(
                "REFRESH");

        _sensorGrid =
            CreateSensorGrid();

        _statusLabel =
            new Label
            {
                Text = "Checking API connection...",
                Font = AppFonts.Caption,
                ForeColor = AppPalette.MidGray,
                AutoSize = true
            };

        _recordCountLabel =
            new Label
            {
                Text = "0 SENSOR RECORDS",
                Font = AppFonts.Uppercase,
                ForeColor = AppPalette.MidGray,
                AutoSize = true
            };

        BuildInterface();
        RegisterEvents();
    }

    protected override async void OnShown(
        EventArgs eventArgs)
    {
        base.OnShown(eventArgs);

        await CheckApiAndLoadAsync();
    }

    protected override void Dispose(
        bool disposing)
    {
        if (disposing)
        {
            _apiClient.Dispose();
        }

        base.Dispose(disposing);
    }

    private void BuildInterface()
    {
        var root =
            new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = AppPalette.Background,
                Padding = Padding.Empty,
                Margin = Padding.Empty
            };

        root.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                30));

        root.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                120));

        root.RowStyles.Add(
            new RowStyle(
                SizeType.Percent,
                100));

        root.Controls.Add(
            BuildTopStrip(),
            0,
            0);

        root.Controls.Add(
            BuildHeader(),
            0,
            1);

        root.Controls.Add(
            BuildContent(),
            0,
            2);

        Controls.Add(root);
    }

    private static Panel BuildTopStrip()
    {
        var strip =
            new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppPalette.Black
            };

        strip.Controls.Add(
            new Label
            {
                Text =
                    "NEXAGRID / SENSOR DATA INGESTION",
                ForeColor = AppPalette.White,
                Font = AppFonts.Uppercase,
                AutoSize = true,
                Location = new Point(35, 8)
            });

        return strip;
    }

    private Panel BuildHeader()
    {
        var header =
            new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppPalette.Background
            };

        var title =
            new Label
            {
                Text = "SENSOR REGISTRY",
                ForeColor = AppPalette.Black,
                Font = AppFonts.PageTitle,
                AutoSize = true,
                Location = new Point(38, 19)
            };

        var subtitle =
            new Label
            {
                Text =
                    "Register, validate and inspect connected sensor profiles.",
                ForeColor = AppPalette.DarkGray,
                Font = AppFonts.Body,
                AutoSize = true,
                Location = new Point(42, 76)
            };

        _statusLabel.Location =
            new Point(980, 38);

        _statusLabel.Anchor =
            AnchorStyles.Top |
            AnchorStyles.Right;

        header.Controls.Add(title);
        header.Controls.Add(subtitle);
        header.Controls.Add(_statusLabel);

        header.Resize += (_, _) =>
        {
            _statusLabel.Left =
                header.ClientSize.Width -
                _statusLabel.Width -
                42;
        };

        return header;
    }

    private Control BuildContent()
    {
        var content =
            new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppPalette.Background,
                Padding =
                    new Padding(40, 15, 40, 40)
            };

        content.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Percent,
                35));

        content.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Percent,
                65));

        content.Controls.Add(
            BuildRegistrationPanel(),
            0,
            0);

        content.Controls.Add(
            BuildRegistryPanel(),
            1,
            0);

        return content;
    }

    private Control BuildRegistrationPanel()
    {
        var panel =
            new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppPalette.Paper,
                BorderStyle =
                    BorderStyle.FixedSingle,
                Padding =
                    new Padding(28),
                Margin =
                    new Padding(0, 0, 12, 0)
            };

        var form =
            new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 16,
                BackColor = AppPalette.Paper
            };

        form.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                38));

        AddField(
            form,
            "DEVICE NAME",
            _deviceNameTextBox);

        AddField(
            form,
            "MAC ADDRESS",
            _macAddressTextBox);

        AddField(
            form,
            "MANUFACTURER",
            _manufacturerTextBox);

        AddField(
            form,
            "SENSOR NAME",
            _sensorNameTextBox);

        AddField(
            form,
            "UNIQUE IDENTIFIER",
            _identifierTextBox);

        AddField(
            form,
            "DEPLOYMENT LOCATION",
            _locationTextBox);

        AddField(
            form,
            "SENSOR CATEGORY",
            _categoryComboBox);

        form.Controls.Add(
            _registerButton);

        panel.Controls.Add(form);

        return panel;
    }

    private Control BuildRegistryPanel()
    {
        var panel =
            new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppPalette.Paper,
                BorderStyle =
                    BorderStyle.FixedSingle,
                Padding =
                    new Padding(24),
                Margin =
                    new Padding(12, 0, 0, 0)
            };

        var layout =
            new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = AppPalette.Paper
            };

        layout.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                48));

        layout.RowStyles.Add(
            new RowStyle(
                SizeType.Percent,
                100));

        var toolbar =
            new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppPalette.Paper
            };

        _recordCountLabel.Location =
            new Point(0, 14);

        _refreshButton.Location =
            new Point(580, 4);

        _refreshButton.Anchor =
            AnchorStyles.Top |
            AnchorStyles.Right;

        toolbar.Controls.Add(
            _recordCountLabel);

        toolbar.Controls.Add(
            _refreshButton);

        toolbar.Resize += (_, _) =>
        {
            _refreshButton.Left =
                toolbar.ClientSize.Width -
                _refreshButton.Width;
        };

        layout.Controls.Add(
            toolbar,
            0,
            0);

        layout.Controls.Add(
            _sensorGrid,
            0,
            1);

        panel.Controls.Add(layout);

        return panel;
    }

    private void RegisterEvents()
    {
        _registerButton.Click +=
            async (_, _) =>
                await RegisterSensorAsync();

        _refreshButton.Click +=
            async (_, _) =>
                await LoadSensorsAsync();
    }

    private async Task CheckApiAndLoadAsync()
    {
        bool healthy =
            await _apiClient.IsHealthyAsync();

        if (!healthy)
        {
            _statusLabel.Text =
                "● API OFFLINE";

            _statusLabel.ForeColor =
                AppPalette.Danger;

            ShowFeedback(
                "Start NexaGrid.API before using the desktop application.",
                MessageBoxIcon.Warning);

            return;
        }

        _statusLabel.Text =
            "● API CONNECTED";

        _statusLabel.ForeColor =
            AppPalette.Success;

        await LoadSensorsAsync();
    }

    private async Task LoadSensorsAsync()
    {
        SetBusy(true);

        try
        {
            IReadOnlyList<SensorResponse> sensors =
                await _sensorService.GetAllAsync();

            _sensorGrid.DataSource =
                sensors.ToList();

            _recordCountLabel.Text =
                $"{sensors.Count} SENSOR RECORD(S)";
        }
        catch (Exception exception)
        {
            ShowFeedback(
                exception.Message,
                MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task RegisterSensorAsync()
    {
        if (!ValidateForm())
        {
            return;
        }

        var request =
            new RegisterSensorRequest
            {
                DeviceName =
                    _deviceNameTextBox.Text.Trim(),
                MacAddress =
                    _macAddressTextBox.Text.Trim(),
                Manufacturer =
                    _manufacturerTextBox.Text.Trim(),
                SensorName =
                    _sensorNameTextBox.Text.Trim(),
                UniqueIdentifier =
                    _identifierTextBox.Text.Trim(),
                DeploymentLocation =
                    _locationTextBox.Text.Trim(),
                Category =
                    (SensorCategory)
                    _categoryComboBox.SelectedItem!
            };

        SetBusy(true);

        try
        {
            SensorResponse sensor =
                await _sensorService.RegisterAsync(
                    request);

            ShowFeedback(
                $"Sensor {sensor.UniqueIdentifier} registered successfully.",
                MessageBoxIcon.Information);

            ClearForm();

            await LoadSensorsAsync();
        }
        catch (ApiException exception)
        {
            ShowFeedback(
                exception.Message,
                MessageBoxIcon.Warning);
        }
        catch (Exception exception)
        {
            ShowFeedback(
                exception.Message,
                MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(
                _deviceNameTextBox.Text) ||
            string.IsNullOrWhiteSpace(
                _macAddressTextBox.Text) ||
            string.IsNullOrWhiteSpace(
                _manufacturerTextBox.Text) ||
            string.IsNullOrWhiteSpace(
                _sensorNameTextBox.Text) ||
            string.IsNullOrWhiteSpace(
                _identifierTextBox.Text) ||
            string.IsNullOrWhiteSpace(
                _locationTextBox.Text) ||
            _categoryComboBox.SelectedItem is null)
        {
            ShowFeedback(
                "Complete every sensor registration field.",
                MessageBoxIcon.Warning);

            return false;
        }

        return true;
    }

    private void SetBusy(bool busy)
    {
        _registerButton.Enabled = !busy;
        _refreshButton.Enabled = !busy;

        UseWaitCursor = busy;

        _registerButton.Text = busy
            ? "PROCESSING..."
            : "REGISTER SENSOR";
    }

    private void ClearForm()
    {
        _deviceNameTextBox.Clear();
        _macAddressTextBox.Clear();
        _manufacturerTextBox.Clear();
        _sensorNameTextBox.Clear();
        _identifierTextBox.Clear();
        _locationTextBox.Clear();

        _categoryComboBox.SelectedIndex = 0;

        _deviceNameTextBox.Focus();
    }

    private void ShowFeedback(
        string message,
        MessageBoxIcon icon)
    {
        MessageBox.Show(
            this,
            message,
            "NexaGrid",
            MessageBoxButtons.OK,
            icon);
    }

    private static void AddField(
        TableLayoutPanel form,
        string labelText,
        Control control)
    {
        form.RowCount++;

        form.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                22));

        form.Controls.Add(
            new Label
            {
                Text = labelText,
                Font = AppFonts.Uppercase,
                ForeColor = AppPalette.DarkGray,
                Dock = DockStyle.Fill,
                TextAlign =
                    ContentAlignment.BottomLeft
            });

        form.RowCount++;

        form.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                43));

        form.Controls.Add(control);
    }

    private static TextBox CreateTextBox()
    {
        return new TextBox
        {
            Dock = DockStyle.Fill,
            Font = AppFonts.Body,
            BackColor = AppPalette.White,
            ForeColor = AppPalette.Black,
            BorderStyle =
                BorderStyle.FixedSingle,
            Margin =
                new Padding(0, 3, 0, 7)
        };
    }

    private static ComboBox CreateCategoryComboBox()
    {
        var comboBox =
            new ComboBox
            {
                Dock = DockStyle.Fill,
                Font = AppFonts.Body,
                BackColor = AppPalette.White,
                ForeColor = AppPalette.Black,
                DropDownStyle =
                    ComboBoxStyle.DropDownList,
                Margin =
                    new Padding(0, 3, 0, 7)
            };

        comboBox.DataSource =
            Enum.GetValues<SensorCategory>();

        return comboBox;
    }

    private static Button CreatePrimaryButton(
        string text)
    {
        var button =
            new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                Height = 42,
                Font = AppFonts.Button,
                BackColor = AppPalette.Black,
                ForeColor = AppPalette.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin =
                    new Padding(0, 12, 0, 0)
            };

        button.FlatAppearance.BorderSize = 0;

        return button;
    }

    private static Button CreateSecondaryButton(
        string text)
    {
        var button =
            new Button
            {
                Text = text,
                Size = new Size(105, 36),
                Font = AppFonts.Button,
                BackColor = AppPalette.Paper,
                ForeColor = AppPalette.Black,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

        button.FlatAppearance.BorderColor =
            AppPalette.Black;

        return button;
    }

    private static DataGridView CreateSensorGrid()
    {
        var grid =
            new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = AppPalette.Paper,
                BorderStyle = BorderStyle.None,
                GridColor = AppPalette.LightGray,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

        grid.EnableHeadersVisualStyles = false;

        grid.ColumnHeadersDefaultCellStyle =
            new DataGridViewCellStyle
            {
                BackColor = AppPalette.Black,
                ForeColor = AppPalette.White,
                Font = AppFonts.Uppercase,
                Padding = new Padding(6),
                SelectionBackColor =
                    AppPalette.Black
            };

        grid.DefaultCellStyle =
            new DataGridViewCellStyle
            {
                BackColor = AppPalette.Paper,
                ForeColor = AppPalette.Black,
                Font = AppFonts.Caption,
                Padding = new Padding(5),
                SelectionBackColor =
                    AppPalette.LightGray,
                SelectionForeColor =
                    AppPalette.Black
            };

        grid.RowTemplate.Height = 38;

        grid.Columns.Add(
            CreateColumn(
                "Id",
                "ID",
                45));

        grid.Columns.Add(
            CreateColumn(
                "UniqueIdentifier",
                "IDENTIFIER",
                105));

        grid.Columns.Add(
            CreateColumn(
                "SensorName",
                "SENSOR",
                145));

        grid.Columns.Add(
            CreateColumn(
                "MacAddress",
                "MAC ADDRESS",
                130));

        grid.Columns.Add(
            CreateColumn(
                "DeploymentLocation",
                "LOCATION",
                165));

        grid.Columns.Add(
            CreateColumn(
                "Category",
                "CATEGORY",
                110));

        grid.Columns.Add(
            CreateColumn(
                "Status",
                "STATUS",
                90));

        return grid;
    }

    private static DataGridViewTextBoxColumn
        CreateColumn(
            string propertyName,
            string heading,
            int minimumWidth)
    {
        return new DataGridViewTextBoxColumn
        {
            DataPropertyName = propertyName,
            HeaderText = heading,
            MinimumWidth = minimumWidth,
            SortMode =
                DataGridViewColumnSortMode.Automatic
        };
    }
}
