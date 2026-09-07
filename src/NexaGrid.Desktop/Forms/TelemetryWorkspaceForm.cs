using System.Globalization;
using System.Text.Json;

using NexaGrid.Desktop.Controls;
using NexaGrid.Desktop.Services;
using NexaGrid.Shared.DTOs;
using NexaGrid.Shared.Enums;

namespace NexaGrid.Desktop.Forms;

public sealed class TelemetryWorkspaceForm : Form
{
    private static readonly Color BackgroundColour =
        Color.FromArgb(239, 237, 232);

    private static readonly Color SurfaceColour =
        Color.FromArgb(248, 247, 243);

    private static readonly Color PrimaryColour =
        Color.FromArgb(10, 10, 10);

    private static readonly Color SecondaryColour =
        Color.FromArgb(94, 92, 87);

    private static readonly Color BorderColour =
        Color.FromArgb(110, 108, 102);

    private static readonly Color SuccessColour =
        Color.FromArgb(48, 104, 70);

    private static readonly Color WarningColour =
        Color.FromArgb(174, 54, 42);

    private readonly ApiClient _apiClient;
    private readonly SensorApiService _sensorService;
    private readonly TelemetryApiService _telemetryService;

    private readonly ComboBox _sensorComboBox;
    private readonly ComboBox _dataTypeComboBox;
    private readonly TextBox _valueTextBox;
    private readonly TextBox _unitTextBox;
    private readonly TextBox _minimumTextBox;
    private readonly TextBox _maximumTextBox;
    private readonly ComboBox _expectedBooleanComboBox;

    private readonly Label _minimumLabel;
    private readonly Label _maximumLabel;
    private readonly Label _expectedBooleanLabel;

    private readonly NumericUpDown _historyLimitInput;
    private readonly NumericUpDown _seedCountInput;

    private readonly Button _submitButton;
    private readonly Button _seedButton;
    private readonly Button _refreshButton;

    private readonly Label _apiStatusLabel;
    private readonly Label _summaryLabel;
    private readonly Label _resultStatusLabel;

    private readonly DataGridView _historyGrid;
    private readonly TelemetryChartControl _chart;

    private bool _isBusy;

    public TelemetryWorkspaceForm()
    {
        _apiClient = new ApiClient();

        _sensorService =
            new SensorApiService(
                _apiClient);

        _telemetryService =
            new TelemetryApiService(
                _apiClient);

        Text = "NexaGrid / Telemetry Workspace";

        AutoScaleMode =
            AutoScaleMode.Dpi;

        StartPosition =
            FormStartPosition.CenterScreen;

        WindowState =
            FormWindowState.Maximized;

        MinimumSize =
            new Size(1250, 820);

        BackColor =
            BackgroundColour;

        ForeColor =
            PrimaryColour;

        Font =
            new Font(
                "Segoe UI",
                10,
                FontStyle.Regular);

        _sensorComboBox =
            CreateComboBox();

        _dataTypeComboBox =
            CreateComboBox();

        _valueTextBox =
            CreateTextBox();

        _unitTextBox =
            CreateTextBox();

        _minimumTextBox =
            CreateTextBox();

        _maximumTextBox =
            CreateTextBox();

        _expectedBooleanComboBox =
            CreateComboBox();

        _minimumLabel =
            CreateFieldLabel(
                "MINIMUM ACCEPTABLE VALUE");

        _maximumLabel =
            CreateFieldLabel(
                "MAXIMUM ACCEPTABLE VALUE");

        _expectedBooleanLabel =
            CreateFieldLabel(
                "EXPECTED BOOLEAN VALUE");

        _historyLimitInput =
            CreateNumericInput(
                1,
                1000,
                50);

        _seedCountInput =
            CreateNumericInput(
                1,
                10000,
                1000);

        _submitButton =
            CreatePrimaryButton(
                "SUBMIT TELEMETRY");

        _seedButton =
            CreateSecondaryButton(
                "GENERATE READINGS");

        _refreshButton =
            CreateSecondaryButton(
                "REFRESH");

        _apiStatusLabel =
            new Label
            {
                AutoSize = true,
                Text = "API CHECKING",
                ForeColor = SecondaryColour,
                Font = new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Bold),
                Anchor =
                    AnchorStyles.Top
                    | AnchorStyles.Right
            };

        _summaryLabel =
            new Label
            {
                AutoSize = true,
                Text = "0 READINGS",
                ForeColor = SecondaryColour,
                Font = new Font(
                    "Bahnschrift",
                    9,
                    FontStyle.Bold)
            };

        _resultStatusLabel =
            new Label
            {
                Dock = DockStyle.Fill,
                Text =
                    "SELECT A SENSOR TO INSPECT TELEMETRY.",
                TextAlign =
                    ContentAlignment.MiddleLeft,
                ForeColor =
                    SecondaryColour,
                BackColor =
                    SurfaceColour,
                Padding =
                    new Padding(14, 0, 14, 0),
                Font =
                    new Font(
                        "Segoe UI",
                        9,
                        FontStyle.Bold)
            };

        _chart =
            new TelemetryChartControl
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };

        _historyGrid =
            CreateHistoryGrid();

        BuildInterface();
        ConfigureInputs();
        ConnectEvents();
    }

    private void BuildInterface()
    {
        var root =
            new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = BackgroundColour,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(42, 0, 42, 30)
            };

        root.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                38));

        root.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                142));

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
            BuildWorkspace(),
            0,
            2);

        Controls.Add(root);
    }

    private Control BuildTopStrip()
    {
        var panel =
            new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = PrimaryColour,
                Margin = new Padding(-42, 0, -42, 0)
            };

        panel.Controls.Add(
            new Label
            {
                Dock = DockStyle.Fill,
                Text =
                    "NEXAGRID / TELEMETRY OPERATIONS",
                ForeColor = Color.White,
                TextAlign =
                    ContentAlignment.MiddleLeft,
                Padding =
                    new Padding(42, 0, 0, 0),
                Font =
                    new Font(
                        "Bahnschrift",
                        9,
                        FontStyle.Bold)
            });

        return panel;
    }

    private Control BuildHeader()
    {
        var layout =
            new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor =
                    BackgroundColour,
                Padding =
                    new Padding(0, 24, 0, 16)
            };

        layout.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Percent,
                75));

        layout.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Percent,
                25));

        var textPanel =
            new Panel
            {
                Dock = DockStyle.Fill
            };

        textPanel.Controls.Add(
            new Label
            {
                AutoSize = true,
                Location = new Point(0, 0),
                Text = "TELEMETRY MONITOR",
                ForeColor = PrimaryColour,
                Font = new Font(
                    "Bahnschrift",
                    26,
                    FontStyle.Bold)
            });

        textPanel.Controls.Add(
            new Label
            {
                AutoSize = true,
                Location = new Point(2, 54),
                Text =
                    "Submit strongly typed readings, monitor thresholds and investigate anomalies.",
                ForeColor =
                    SecondaryColour,
                Font =
                    new Font(
                        "Segoe UI",
                        10,
                        FontStyle.Regular)
            });

        var statusPanel =
            new Panel
            {
                Dock = DockStyle.Fill
            };

        _apiStatusLabel.Location =
            new Point(20, 12);

        statusPanel.Controls.Add(
            _apiStatusLabel);

        layout.Controls.Add(
            textPanel,
            0,
            0);

        layout.Controls.Add(
            statusPanel,
            1,
            0);

        return layout;
    }

    private Control BuildWorkspace()
    {
        var layout =
            new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor =
                    BackgroundColour
            };

        layout.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Absolute,
                385));

        layout.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Percent,
                100));

        layout.Controls.Add(
            BuildInputPanel(),
            0,
            0);

        layout.Controls.Add(
            BuildResultsPanel(),
            1,
            0);

        return layout;
    }

    private Control BuildInputPanel()
    {
        var border =
            new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BorderColour,
                Padding = new Padding(1),
                Margin =
                    new Padding(0, 0, 22, 0)
            };

        var scrollingPanel =
            new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SurfaceColour,
                AutoScroll = true,
                Padding =
                    new Padding(25, 20, 25, 20)
            };

        var fields =
            new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode =
                    AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 0,
                GrowStyle =
                    TableLayoutPanelGrowStyle.AddRows,
                BackColor =
                    SurfaceColour
            };

        AddField(
            fields,
            CreateFieldLabel(
                "SENSOR IDENTIFIER"),
            _sensorComboBox);

        AddField(
            fields,
            CreateFieldLabel(
                "DATA TYPE"),
            _dataTypeComboBox);

        AddField(
            fields,
            CreateFieldLabel(
                "READING VALUE"),
            _valueTextBox);

        AddField(
            fields,
            CreateFieldLabel(
                "UNIT"),
            _unitTextBox);

        AddField(
            fields,
            _minimumLabel,
            _minimumTextBox);

        AddField(
            fields,
            _maximumLabel,
            _maximumTextBox);

        AddField(
            fields,
            _expectedBooleanLabel,
            _expectedBooleanComboBox);

        AddField(
            fields,
            CreateFieldLabel(
                "HISTORY LIMIT"),
            _historyLimitInput);

        fields.Controls.Add(
            _submitButton);

        fields.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                58));

        _submitButton.Dock =
            DockStyle.Top;

        _submitButton.Margin =
            new Padding(0, 12, 0, 8);

        fields.Controls.Add(
            CreateDivider());

        fields.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                30));

        AddField(
            fields,
            CreateFieldLabel(
                "GENERATED READING COUNT"),
            _seedCountInput);

        fields.Controls.Add(
            _seedButton);

        fields.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                52));

        _seedButton.Dock =
            DockStyle.Top;

        _seedButton.Margin =
            new Padding(0, 8, 0, 0);

        scrollingPanel.Controls.Add(
            fields);

        border.Controls.Add(
            scrollingPanel);

        return border;
    }

    private Control BuildResultsPanel()
    {
        var border =
            new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BorderColour,
                Padding = new Padding(1)
            };

        var content =
            new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = SurfaceColour,
                ColumnCount = 1,
                RowCount = 5,
                Padding =
                    new Padding(24, 20, 24, 20)
            };

        content.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                45));

        content.RowStyles.Add(
            new RowStyle(
                SizeType.Percent,
                55));

        content.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                48));

        content.RowStyles.Add(
            new RowStyle(
                SizeType.Percent,
                45));

        content.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                44));

        content.Controls.Add(
            BuildResultsToolbar(),
            0,
            0);

        content.Controls.Add(
            _chart,
            0,
            1);

        content.Controls.Add(
            BuildSectionHeading(
                "RECENT TELEMETRY RECORDS"),
            0,
            2);

        content.Controls.Add(
            _historyGrid,
            0,
            3);

        content.Controls.Add(
            _resultStatusLabel,
            0,
            4);

        border.Controls.Add(
            content);

        return border;
    }

    private Control BuildResultsToolbar()
    {
        var layout =
            new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor =
                    SurfaceColour
            };

        layout.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Percent,
                100));

        layout.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Absolute,
                135));

        _summaryLabel.Anchor =
            AnchorStyles.Left;

        _refreshButton.Dock =
            DockStyle.Fill;

        _refreshButton.Margin =
            new Padding(0, 0, 0, 7);

        layout.Controls.Add(
            _summaryLabel,
            0,
            0);

        layout.Controls.Add(
            _refreshButton,
            1,
            0);

        return layout;
    }

    private static Control BuildSectionHeading(
        string text)
    {
        var label =
            new Label
            {
                Dock = DockStyle.Fill,
                Text = text,
                TextAlign =
                    ContentAlignment.MiddleLeft,
                ForeColor =
                    PrimaryColour,
                Font =
                    new Font(
                        "Bahnschrift",
                        9,
                        FontStyle.Bold),
                Padding =
                    new Padding(0, 10, 0, 0)
            };

        return label;
    }

    private void ConfigureInputs()
    {
        _dataTypeComboBox.DataSource =
            Enum.GetValues<TelemetryDataType>();

        _expectedBooleanComboBox.Items.AddRange(
            ["True", "False"]);

        _expectedBooleanComboBox.SelectedIndex = 0;

        _unitTextBox.Text = "Celsius";

        _minimumTextBox.Text = "20";
        _maximumTextBox.Text = "35";

        UpdateDataTypeFields();
    }

    private void ConnectEvents()
    {
        Shown +=
            async (_, _) =>
                await InitialiseAsync();

        _sensorComboBox.SelectedIndexChanged +=
            async (_, _) =>
                await HandleSensorSelectionChangedAsync();

        _dataTypeComboBox.SelectedIndexChanged +=
            (_, _) =>
                UpdateDataTypeFields();

        _refreshButton.Click +=
            async (_, _) =>
                await RefreshWorkspaceAsync();

        _submitButton.Click +=
            async (_, _) =>
                await SubmitTelemetryAsync();

        _seedButton.Click +=
            async (_, _) =>
                await SeedTelemetryAsync();

        FormClosed +=
            (_, _) =>
                _apiClient.Dispose();
    }

    private async Task InitialiseAsync()
    {
        await RunBusyOperationAsync(
            async () =>
            {
                bool healthy =
                    await _telemetryService
                        .IsApiHealthyAsync();

                UpdateApiStatus(healthy);

                if (!healthy)
                {
                    throw new InvalidOperationException(
                        "The NexaGrid API is offline. Start the API on http://localhost:5211.");
                }

                await LoadSensorsAsync();
                await LoadTelemetryAsync();
            });
    }

    private async Task HandleSensorSelectionChangedAsync()
    {
        if (_isBusy || IsDisposed || Disposing)
        {
            return;
        }

        await RunBusyOperationAsync(
            LoadTelemetryAsync);
    }

    private async Task RefreshWorkspaceAsync()
    {
        await RunBusyOperationAsync(
            async () =>
            {
                await LoadSensorsAsync();
                await LoadTelemetryAsync();
            });
    }

    private async Task LoadSensorsAsync()
    {
        string? previousIdentifier =
            GetSelectedSensorIdentifier();

        IReadOnlyList<SensorResponse> sensors =
            await _sensorService.GetAllAsync();

        _sensorComboBox.DataSource = null;

        _sensorComboBox.DisplayMember =
            nameof(SensorResponse.UniqueIdentifier);

        _sensorComboBox.ValueMember =
            nameof(SensorResponse.UniqueIdentifier);

        _sensorComboBox.DataSource =
            sensors.ToList();

        if (!string.IsNullOrWhiteSpace(
                previousIdentifier))
        {
            SensorResponse? matchingSensor =
                sensors.FirstOrDefault(
                    sensor =>
                        sensor.UniqueIdentifier.Equals(
                            previousIdentifier,
                            StringComparison.OrdinalIgnoreCase));

            if (matchingSensor is not null)
            {
                _sensorComboBox.SelectedItem =
                    matchingSensor;
            }
        }

        if (sensors.Count == 0)
        {
            _resultStatusLabel.Text =
                "NO SENSORS ARE REGISTERED. USE THE SENSOR REGISTRY FIRST.";
        }
    }

    private async Task LoadTelemetryAsync()
    {
        string? sensorIdentifier =
            GetSelectedSensorIdentifier();

        if (string.IsNullOrWhiteSpace(
                sensorIdentifier))
        {
            ClearResults();
            return;
        }

        int limit =
            decimal.ToInt32(
                _historyLimitInput.Value);

        IReadOnlyList<TelemetryResponse> readings =
            await _telemetryService.GetRecentAsync(
                sensorIdentifier,
                limit);

        DisplayReadings(readings);
    }

    private async Task SubmitTelemetryAsync()
    {
        await RunBusyOperationAsync(
            async () =>
            {
                IngestTelemetryRequest request =
                    BuildTelemetryRequest();

                TelemetryResponse response =
                    await _telemetryService.IngestAsync(
                        request);

                _resultStatusLabel.ForeColor =
                    response.IsAnomaly
                        ? WarningColour
                        : SuccessColour;

                _resultStatusLabel.Text =
                    response.IsAnomaly
                        ? $"ANOMALY DETECTED - {response.SensorIdentifier}: {response.Value} {response.Unit}"
                        : $"READING ACCEPTED - {response.SensorIdentifier}: {response.Value} {response.Unit}";

                await LoadTelemetryAsync();

                if (response.IsAnomaly)
                {
                    MessageBox.Show(
                        $"Anomaly detected for {response.SensorIdentifier}."
                        + Environment.NewLine
                        + $"Value: {response.Value} {response.Unit}"
                        + Environment.NewLine
                        + response.StatusMessage,
                        "NexaGrid anomaly alert",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            });
    }

    private async Task SeedTelemetryAsync()
    {
        string? sensorIdentifier =
            GetSelectedSensorIdentifier();

        if (string.IsNullOrWhiteSpace(
                sensorIdentifier))
        {
            ShowWarning(
                "Select a sensor before generating telemetry.");

            return;
        }

        int count =
            decimal.ToInt32(
                _seedCountInput.Value);

        DialogResult confirmation =
            MessageBox.Show(
                $"Generate {count:N0} readings for {sensorIdentifier}?",
                "Generate readings",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        await RunBusyOperationAsync(
            async () =>
            {
                TelemetrySeedResponse result =
                    await _telemetryService.SeedAsync(
                        sensorIdentifier,
                        count);

                _resultStatusLabel.ForeColor =
                    result.AnomalyCount > 0
                        ? WarningColour
                        : SuccessColour;

                _resultStatusLabel.Text =
                    $"{result.CreatedReadingCount:N0} READINGS GENERATED / "
                    + $"{result.BatchCount:N0} BATCHES / "
                    + $"{result.AnomalyCount:N0} ANOMALIES / "
                    + $"{result.ProcessingTimeMilliseconds:N2} MS";

                await LoadTelemetryAsync();

                MessageBox.Show(
                    $"Sensor: {result.SensorIdentifier}"
                    + Environment.NewLine
                    + $"Created: {result.CreatedReadingCount:N0}"
                    + Environment.NewLine
                    + $"Batches: {result.BatchCount:N0}"
                    + Environment.NewLine
                    + $"Batch size: {result.BatchSize:N0}"
                    + Environment.NewLine
                    + $"Anomalies: {result.AnomalyCount:N0}"
                    + Environment.NewLine
                    + $"Processing time: {result.ProcessingTimeMilliseconds:N2} ms",
                    "Telemetry generation complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            });
    }

    private IngestTelemetryRequest BuildTelemetryRequest()
    {
        string? sensorIdentifier =
            GetSelectedSensorIdentifier();

        if (string.IsNullOrWhiteSpace(
                sensorIdentifier))
        {
            throw new ArgumentException(
                "Select a sensor identifier.");
        }

        if (_dataTypeComboBox.SelectedItem
            is not TelemetryDataType dataType)
        {
            throw new ArgumentException(
                "Select a telemetry data type.");
        }

        string rawValue =
            _valueTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(
                rawValue))
        {
            throw new ArgumentException(
                "Enter a telemetry reading value.");
        }

        JsonElement value =
            CreateJsonValue(
                dataType,
                rawValue);

        double? minimum = null;
        double? maximum = null;
        bool? expectedBoolean = null;

        if (dataType
            is TelemetryDataType.Float
            or TelemetryDataType.Integer)
        {
            minimum =
                ParseOptionalNumber(
                    _minimumTextBox.Text,
                    "minimum acceptable value");

            maximum =
                ParseOptionalNumber(
                    _maximumTextBox.Text,
                    "maximum acceptable value");

            if (minimum.HasValue
                && maximum.HasValue
                && minimum.Value > maximum.Value)
            {
                throw new ArgumentException(
                    "The minimum acceptable value cannot exceed the maximum.");
            }
        }
        else
        {
            expectedBoolean =
                string.Equals(
                    _expectedBooleanComboBox.Text,
                    "True",
                    StringComparison.OrdinalIgnoreCase);
        }

        return new IngestTelemetryRequest
        {
            SensorIdentifier =
                sensorIdentifier,

            DataType =
                dataType,

            Value =
                value,

            Unit =
                _unitTextBox.Text.Trim(),

            MinimumAcceptableValue =
                minimum,

            MaximumAcceptableValue =
                maximum,

            ExpectedBooleanValue =
                expectedBoolean,

            RecordedAtUtc =
                DateTime.UtcNow
        };
    }

    private static JsonElement CreateJsonValue(
        TelemetryDataType dataType,
        string rawValue)
    {
        return dataType switch
        {
            TelemetryDataType.Float =>
                JsonSerializer.SerializeToElement(
                    ParseFloat(rawValue)),

            TelemetryDataType.Integer =>
                JsonSerializer.SerializeToElement(
                    ParseInteger(rawValue)),

            TelemetryDataType.Boolean =>
                JsonSerializer.SerializeToElement(
                    ParseBoolean(rawValue)),

            _ =>
                throw new ArgumentOutOfRangeException(
                    nameof(dataType))
        };
    }

    private static float ParseFloat(
        string value)
    {
        if (!float.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float result))
        {
            throw new ArgumentException(
                "Enter a valid floating-point value, for example 24.5.");
        }

        return result;
    }

    private static int ParseInteger(
        string value)
    {
        if (!int.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int result))
        {
            throw new ArgumentException(
                "Enter a valid whole number.");
        }

        return result;
    }

    private static bool ParseBoolean(
        string value)
    {
        if (!bool.TryParse(
                value,
                out bool result))
        {
            throw new ArgumentException(
                "Enter either True or False for a Boolean reading.");
        }

        return result;
    }

    private static double? ParseOptionalNumber(
        string value,
        string fieldName)
    {
        if (string.IsNullOrWhiteSpace(
                value))
        {
            return null;
        }

        if (!double.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double result))
        {
            throw new ArgumentException(
                $"Enter a valid {fieldName}.");
        }

        return result;
    }

    private void DisplayReadings(
        IReadOnlyList<TelemetryResponse> readings)
    {
        _historyGrid.Rows.Clear();

        foreach (TelemetryResponse reading
                 in readings)
        {
            int rowIndex =
                _historyGrid.Rows.Add(
                    reading.RecordedAtUtc
                        .ToLocalTime()
                        .ToString("yyyy-MM-dd HH:mm:ss"),
                    reading.SensorIdentifier,
                    reading.DataType,
                    reading.Value,
                    reading.Unit,
                    reading.IsAnomaly
                        ? "ANOMALY"
                        : "NORMAL");

            DataGridViewRow row =
                _historyGrid.Rows[rowIndex];

            if (reading.IsAnomaly)
            {
                row.DefaultCellStyle.ForeColor =
                    WarningColour;

                row.DefaultCellStyle.Font =
                    new Font(
                        _historyGrid.Font,
                        FontStyle.Bold);
            }
        }

        _chart.SetReadings(
            readings);

        int anomalyCount =
            readings.Count(
                reading => reading.IsAnomaly);

        _summaryLabel.Text =
            $"{readings.Count:N0} READINGS / "
            + $"{anomalyCount:N0} ANOMALIES";

        if (readings.Count == 0)
        {
            _resultStatusLabel.ForeColor =
                SecondaryColour;

            _resultStatusLabel.Text =
                "NO TELEMETRY READINGS FOUND FOR THE SELECTED SENSOR.";
        }
        else
        {
            _resultStatusLabel.ForeColor =
                anomalyCount > 0
                    ? WarningColour
                    : SuccessColour;

            _resultStatusLabel.Text =
                anomalyCount > 0
                    ? $"{anomalyCount:N0} ANOMALOUS READING(S) REQUIRE REVIEW."
                    : "ALL DISPLAYED READINGS ARE WITHIN THE EXPECTED RANGE.";
        }
    }

    private void ClearResults()
    {
        _historyGrid.Rows.Clear();
        _chart.ClearReadings();

        _summaryLabel.Text =
            "0 READINGS";

        _resultStatusLabel.ForeColor =
            SecondaryColour;

        _resultStatusLabel.Text =
            "SELECT A SENSOR TO INSPECT TELEMETRY.";
    }

    private void UpdateDataTypeFields()
    {
        TelemetryDataType selectedType =
            _dataTypeComboBox.SelectedItem
                is TelemetryDataType type
                    ? type
                    : TelemetryDataType.Float;

        bool numeric =
            selectedType
            is TelemetryDataType.Float
            or TelemetryDataType.Integer;

        _minimumLabel.Visible = numeric;
        _minimumTextBox.Visible = numeric;

        _maximumLabel.Visible = numeric;
        _maximumTextBox.Visible = numeric;

        _expectedBooleanLabel.Visible = !numeric;
        _expectedBooleanComboBox.Visible = !numeric;

        if (selectedType == TelemetryDataType.Boolean)
        {
            _valueTextBox.Text = "True";
            _unitTextBox.Text = "State";
        }
        else if (selectedType == TelemetryDataType.Integer)
        {
            _valueTextBox.Clear();
            _unitTextBox.Text = "Count";
        }
        else
        {
            _valueTextBox.Clear();
            _unitTextBox.Text = "Celsius";
        }
    }

    private async Task RunBusyOperationAsync(
        Func<Task> operation)
    {
        if (_isBusy)
        {
            return;
        }

        try
        {
            SetBusyState(true);
            await operation();
        }
        catch (ApiException exception)
        {
            UpdateApiStatus(false);

            MessageBox.Show(
                exception.Message,
                "NexaGrid API error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch (ArgumentException exception)
        {
            ShowWarning(
                exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            MessageBox.Show(
                exception.Message,
                "NexaGrid",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch (HttpRequestException)
        {
            UpdateApiStatus(false);

            MessageBox.Show(
                "The NexaGrid API could not be reached. Confirm that it is running on http://localhost:5211.",
                "API unavailable",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch (TaskCanceledException)
        {
            MessageBox.Show(
                "The API request timed out.",
                "Request timeout",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
        catch (System.Net.Sockets.SocketException)
        {
            UpdateApiStatus(false);

            MessageBox.Show(
                "The telemetry connection was interrupted. Confirm that the API is running, then select Refresh.",
                "Connection interrupted",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
        finally
        {
            SetBusyState(false);
        }
    }

    private void SetBusyState(
        bool busy)
    {
        _isBusy = busy;

        _submitButton.Enabled = !busy;
        _seedButton.Enabled = !busy;
        _refreshButton.Enabled = !busy;

        _submitButton.Text =
            busy
                ? "PROCESSING..."
                : "SUBMIT TELEMETRY";

        UseWaitCursor = busy;
    }

    private void UpdateApiStatus(
        bool healthy)
    {
        _apiStatusLabel.Text =
            healthy
                ? "API ONLINE"
                : "API OFFLINE";

        _apiStatusLabel.ForeColor =
            healthy
                ? SuccessColour
                : WarningColour;
    }

    private string? GetSelectedSensorIdentifier()
    {
        return _sensorComboBox.SelectedItem
            is SensorResponse sensor
                ? sensor.UniqueIdentifier
                : null;
    }

    private static void ShowWarning(
        string message)
    {
        MessageBox.Show(
            message,
            "NexaGrid validation",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    private static TextBox CreateTextBox()
    {
        return new TextBox
        {
            Dock = DockStyle.Top,
            Height = 32,
            BorderStyle =
                BorderStyle.FixedSingle,
            BackColor = Color.White,
            ForeColor = PrimaryColour,
            Font = new Font(
                "Segoe UI",
                10,
                FontStyle.Regular)
        };
    }

    private static ComboBox CreateComboBox()
    {
        return new ComboBox
        {
            Dock = DockStyle.Top,
            Height = 34,
            DropDownStyle =
                ComboBoxStyle.DropDownList,
            FlatStyle =
                FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = PrimaryColour,
            Font = new Font(
                "Segoe UI",
                10,
                FontStyle.Regular)
        };
    }

    private static NumericUpDown CreateNumericInput(
        decimal minimum,
        decimal maximum,
        decimal value)
    {
        return new NumericUpDown
        {
            Dock = DockStyle.Top,
            Height = 32,
            Minimum = minimum,
            Maximum = maximum,
            Value = value,
            ThousandsSeparator = true,
            BorderStyle =
                BorderStyle.FixedSingle,
            BackColor = Color.White,
            ForeColor = PrimaryColour,
            Font = new Font(
                "Segoe UI",
                10,
                FontStyle.Regular)
        };
    }

    private static Label CreateFieldLabel(
        string text)
    {
        return new Label
        {
            Dock = DockStyle.Top,
            AutoSize = false,
            Height = 25,
            Text = text,
            TextAlign =
                ContentAlignment.BottomLeft,
            ForeColor = PrimaryColour,
            Font = new Font(
                "Bahnschrift",
                9,
                FontStyle.Bold)
        };
    }

    private static Button CreatePrimaryButton(
        string text)
    {
        return new Button
        {
            Text = text,
            Height = 40,
            FlatStyle = FlatStyle.Flat,
            BackColor = PrimaryColour,
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Font = new Font(
                "Bahnschrift",
                9,
                FontStyle.Bold),
            UseVisualStyleBackColor = false
        };
    }

    private static Button CreateSecondaryButton(
        string text)
    {
        var button =
            new Button
            {
                Text = text,
                Height = 38,
                FlatStyle =
                    FlatStyle.Flat,
                BackColor =
                    SurfaceColour,
                ForeColor =
                    PrimaryColour,
                Cursor =
                    Cursors.Hand,
                Font =
                    new Font(
                        "Bahnschrift",
                        9,
                        FontStyle.Bold),
                UseVisualStyleBackColor =
                    false
            };

        button.FlatAppearance.BorderColor =
            PrimaryColour;

        button.FlatAppearance.BorderSize = 1;

        return button;
    }

    private static Panel CreateDivider()
    {
        return new Panel
        {
            Dock = DockStyle.Top,
            Height = 1,
            BackColor = BorderColour,
            Margin =
                new Padding(0, 14, 0, 14)
        };
    }

    private static void AddField(
        TableLayoutPanel layout,
        Control label,
        Control input)
    {
        layout.Controls.Add(label);

        layout.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                28));

        layout.Controls.Add(input);

        layout.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                49));

        input.Margin =
            new Padding(0, 0, 0, 13);
    }

    private static DataGridView CreateHistoryGrid()
    {
        var grid =
            new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor =
                    SurfaceColour,
                BorderStyle =
                    BorderStyle.None,
                AllowUserToAddRows =
                    false,
                AllowUserToDeleteRows =
                    false,
                AllowUserToResizeRows =
                    false,
                ReadOnly = true,
                RowHeadersVisible =
                    false,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,
                MultiSelect =
                    false,
                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.None,
                ScrollBars =
                    ScrollBars.Both,
                EnableHeadersVisualStyles =
                    false,
                GridColor =
                    Color.FromArgb(
                        218,
                        215,
                        207)
            };

        grid.ColumnHeadersDefaultCellStyle =
            new DataGridViewCellStyle
            {
                BackColor =
                    PrimaryColour,
                ForeColor =
                    Color.White,
                Font =
                    new Font(
                        "Bahnschrift",
                        8,
                        FontStyle.Bold),
                Alignment =
                    DataGridViewContentAlignment
                        .MiddleLeft,
                Padding =
                    new Padding(6),
                WrapMode =
                    DataGridViewTriState.False
            };

        grid.DefaultCellStyle =
            new DataGridViewCellStyle
            {
                BackColor =
                    SurfaceColour,
                ForeColor =
                    PrimaryColour,
                SelectionBackColor =
                    Color.FromArgb(
                        218,
                        215,
                        207),
                SelectionForeColor =
                    PrimaryColour,
                Font =
                    new Font(
                        "Segoe UI",
                        9,
                        FontStyle.Regular),
                Padding =
                    new Padding(5),
                WrapMode =
                    DataGridViewTriState.False
            };

        grid.RowTemplate.Height = 36;
        grid.ColumnHeadersHeight = 42;
        grid.ColumnHeadersHeightSizeMode =
            DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

        grid.Columns.Add(
            "RecordedAt",
            "RECORDED AT");

        grid.Columns.Add(
            "SensorIdentifier",
            "SENSOR");

        grid.Columns.Add(
            "DataType",
            "TYPE");

        grid.Columns.Add(
            "Value",
            "VALUE");

        grid.Columns.Add(
            "Unit",
            "UNIT");

        grid.Columns.Add(
            "Status",
            "STATUS");

        grid.Columns["RecordedAt"]!.Width = 190;
        grid.Columns["SensorIdentifier"]!.Width = 145;
        grid.Columns["DataType"]!.Width = 115;
        grid.Columns["Value"]!.Width = 145;
        grid.Columns["Unit"]!.Width = 145;
        grid.Columns["Status"]!.Width = 180;

        foreach (DataGridViewColumn column
                 in grid.Columns)
        {
            column.MinimumWidth = 90;
            column.SortMode =
                DataGridViewColumnSortMode.NotSortable;
        }

        return grid;
    }
}
