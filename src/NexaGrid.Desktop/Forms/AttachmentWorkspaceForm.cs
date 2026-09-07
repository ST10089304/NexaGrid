using System.Diagnostics;

using NexaGrid.Desktop.Services;
using NexaGrid.Shared.DTOs;

namespace NexaGrid.Desktop.Forms;

public sealed class AttachmentWorkspaceForm : Form
{
    private static readonly Color BackgroundColour = Color.FromArgb(239, 237, 232);
    private static readonly Color SurfaceColour = Color.FromArgb(248, 247, 243);
    private static readonly Color PrimaryColour = Color.FromArgb(10, 10, 10);
    private static readonly Color SecondaryColour = Color.FromArgb(94, 92, 87);
    private static readonly Color BorderColour = Color.FromArgb(110, 108, 102);
    private static readonly Color SuccessColour = Color.FromArgb(48, 104, 70);
    private static readonly Color DangerColour = Color.FromArgb(174, 54, 42);

    private readonly ApiClient _apiClient;
    private readonly SensorApiService _sensorService;
    private readonly AttachmentApiService _attachmentService;

    private readonly ComboBox _sensorComboBox;
    private readonly TextBox _filePathTextBox;
    private readonly Button _browseButton;
    private readonly Button _uploadButton;
    private readonly Button _refreshButton;
    private readonly Button _downloadButton;
    private readonly Button _openButton;
    private readonly Label _apiStatusLabel;
    private readonly Label _summaryLabel;
    private readonly Label _feedbackLabel;
    private readonly DataGridView _attachmentGrid;

    private bool _isBusy;
    private bool _loadingSensors;

    public AttachmentWorkspaceForm()
    {
        _apiClient = new ApiClient();
        _sensorService = new SensorApiService(_apiClient);
        _attachmentService = new AttachmentApiService(_apiClient);

        Text = "NexaGrid / Attachment Workspace";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(920, 620);
        Size = new Size(1380, 820);
        BackColor = BackgroundColour;
        ForeColor = PrimaryColour;
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = new Font("Segoe UI", 10F, FontStyle.Regular);

        _sensorComboBox = CreateComboBox();
        _filePathTextBox = CreateTextBox();
        _filePathTextBox.ReadOnly = true;

        _browseButton = CreateButton("BROWSE");
        _uploadButton = CreateButton("UPLOAD FILE", true);
        _refreshButton = CreateButton("REFRESH");
        _downloadButton = CreateButton("DOWNLOAD");
        _openButton = CreateButton("DOWNLOAD AND OPEN");

        _apiStatusLabel = CreateLabel("CHECKING API", true);
        _apiStatusLabel.TextAlign = ContentAlignment.MiddleRight;

        _summaryLabel = CreateLabel("0 ATTACHMENTS", true);
        _feedbackLabel = CreateLabel("SELECT A SENSOR TO VIEW ITS ATTACHMENTS.", true);
        _feedbackLabel.ForeColor = SecondaryColour;

        _attachmentGrid = CreateAttachmentGrid();

        Controls.Add(BuildPage());
        RegisterEvents();

        Shown += async (_, _) => await InitialiseAsync();
        FormClosed += (_, _) => _apiClient.Dispose();
    }

    private Control BuildPage()
    {
        var page = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = BackgroundColour,
            Padding = new Padding(32, 28, 32, 28),
            ColumnCount = 1,
            RowCount = 3
        };

        page.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));
        page.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        page.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));

        page.Controls.Add(BuildHeader(), 0, 0);
        page.Controls.Add(BuildContent(), 0, 1);
        page.Controls.Add(BuildFooter(), 0, 2);

        return page;
    }

    private Control BuildHeader()
    {
        var panel = new Panel { Dock = DockStyle.Fill };

        var strip = new Label
        {
            Dock = DockStyle.Top,
            Height = 38,
            Padding = new Padding(28, 0, 0, 0),
            BackColor = PrimaryColour,
            ForeColor = Color.White,
            Font = new Font("Bahnschrift", 10F, FontStyle.Bold),
            Text = "NEXAGRID / SECURE ATTACHMENTS",
            TextAlign = ContentAlignment.MiddleLeft
        };

        var heading = new Label
        {
            AutoSize = true,
            Location = new Point(10, 71),
            Font = new Font("Bahnschrift", 27F, FontStyle.Bold),
            Text = "ATTACHMENT WORKSPACE"
        };

        var subtitle = new Label
        {
            AutoSize = true,
            Location = new Point(12, 116),
            ForeColor = SecondaryColour,
            Text = "Upload approved sensor evidence and retrieve stored files securely."
        };

        _apiStatusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _apiStatusLabel.Location = new Point(panel.Width - 190, 80);
        _apiStatusLabel.Size = new Size(180, 28);

        panel.Controls.Add(strip);
        panel.Controls.Add(heading);
        panel.Controls.Add(subtitle);
        panel.Controls.Add(_apiStatusLabel);
        panel.Resize += (_, _) =>
        {
            _apiStatusLabel.Left = Math.Max(10, panel.ClientSize.Width - _apiStatusLabel.Width - 10);
        };

        return panel;
    }

    private Control BuildContent()
    {
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Size = new Size(1000, 500),
            BackColor = BackgroundColour,
            BorderStyle = BorderStyle.None,
            FixedPanel = FixedPanel.Panel1,
            IsSplitterFixed = false,
            Panel1MinSize = 310,
            Panel2MinSize = 480,
            SplitterDistance = 350,
            SplitterWidth = 22
        };

        split.Panel1.Controls.Add(BuildUploadPanel());
        split.Panel2.Controls.Add(BuildAttachmentPanel());

        return split;
    }

    private Control BuildUploadPanel()
    {
        var border = CreateBorderPanel();
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(24, 24, 24, 24),
            ColumnCount = 1,
            RowCount = 10
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        layout.Controls.Add(CreateFieldLabel("SENSOR IDENTIFIER"));
        layout.Controls.Add(_sensorComboBox);
        layout.Controls.Add(CreateSpacer(18));
        layout.Controls.Add(CreateFieldLabel("FILE TO UPLOAD"));
        layout.Controls.Add(_filePathTextBox);
        layout.Controls.Add(CreateSpacer(10));
        layout.Controls.Add(_browseButton);
        layout.Controls.Add(CreateSpacer(14));
        layout.Controls.Add(_uploadButton);

        var help = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(285, 0),
            Margin = new Padding(0, 26, 0, 0),
            ForeColor = SecondaryColour,
            Font = new Font("Segoe UI", 9F),
            Text = "MAXIMUM FILE SIZE: 10 MB\r\n\r\nALLOWED TYPES:\r\nJPG, JPEG, PNG, PDF, TXT, LOG, JSON, XML, CSV, YAML, YML AND CONF"
        };

        layout.Controls.Add(help);
        border.Controls.Add(layout);
        return border;
    }

    private Control BuildAttachmentPanel()
    {
        var border = CreateBorderPanel();
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            ColumnCount = 1,
            RowCount = 4
        };

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));

        var toolbar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
        toolbar.Controls.Add(_summaryLabel, 0, 0);
        toolbar.Controls.Add(_refreshButton, 1, 0);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 10, 0, 0)
        };
        _openButton.Width = 190;
        _downloadButton.Width = 130;
        actions.Controls.Add(_openButton);
        actions.Controls.Add(_downloadButton);

        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(_attachmentGrid, 0, 1);
        layout.Controls.Add(actions, 0, 2);
        layout.Controls.Add(_feedbackLabel, 0, 3);

        border.Controls.Add(layout);
        return border;
    }

    private Control BuildFooter()
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(6, 12, 0, 0),
            ForeColor = SecondaryColour,
            Font = new Font("Bahnschrift", 9F, FontStyle.Bold),
            Text = "SECURE FILE STORAGE / SENSOR EVIDENCE / CONTROLLED DOWNLOADS"
        };
    }

    private static Panel CreateBorderPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = SurfaceColour,
            Padding = new Padding(1)
        };
        panel.Paint += (_, e) =>
        {
            using var pen = new Pen(BorderColour);
            e.Graphics.DrawRectangle(pen, 0, 0, panel.ClientSize.Width - 1, panel.ClientSize.Height - 1);
        };
        return panel;
    }

    private static Label CreateFieldLabel(string text)
    {
        return new Label
        {
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 6),
            Font = new Font("Bahnschrift", 9.5F, FontStyle.Bold),
            Text = text
        };
    }

    private static Label CreateLabel(string text, bool bold)
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Text = text,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Bahnschrift", 9.5F, bold ? FontStyle.Bold : FontStyle.Regular)
        };
    }

    private static Control CreateSpacer(int height)
    {
        return new Panel { Height = height, Dock = DockStyle.Top };
    }

    private static TextBox CreateTextBox()
    {
        return new TextBox
        {
            Dock = DockStyle.Top,
            Height = 32,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.White,
            Font = new Font("Segoe UI", 10F)
        };
    }

    private static ComboBox CreateComboBox()
    {
        return new ComboBox
        {
            Dock = DockStyle.Top,
            Height = 34,
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            Font = new Font("Segoe UI", 10F)
        };
    }

    private static Button CreateButton(string text, bool primary = false)
    {
        return new Button
        {
            Dock = DockStyle.Top,
            Height = 40,
            Text = text,
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 1, BorderColor = PrimaryColour },
            BackColor = primary ? PrimaryColour : SurfaceColour,
            ForeColor = primary ? Color.White : PrimaryColour,
            Font = new Font("Bahnschrift", 9.5F, FontStyle.Bold),
            UseVisualStyleBackColor = false
        };
    }

    private static DataGridView CreateAttachmentGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = SurfaceColour,
            BorderStyle = BorderStyle.FixedSingle,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            ReadOnly = true,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false,
            AutoGenerateColumns = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
            ScrollBars = ScrollBars.Both,
            ColumnHeadersHeight = 40,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            RowTemplate = { Height = 38 },
            EnableHeadersVisualStyles = false
        };

        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = PrimaryColour,
            ForeColor = Color.White,
            Font = new Font("Bahnschrift", 9F, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            Padding = new Padding(6, 0, 6, 0)
        };
        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = SurfaceColour,
            ForeColor = PrimaryColour,
            SelectionBackColor = Color.FromArgb(217, 214, 207),
            SelectionForeColor = PrimaryColour,
            Font = new Font("Segoe UI", 9.5F),
            Padding = new Padding(6, 0, 6, 0)
        };
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(235, 233, 227);

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "FileName",
            HeaderText = "FILE NAME",
            DataPropertyName = nameof(SensorAttachmentResponse.OriginalFileName),
            Width = 260,
            MinimumWidth = 180
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Sensor",
            HeaderText = "SENSOR",
            DataPropertyName = nameof(SensorAttachmentResponse.SensorIdentifier),
            Width = 130
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Type",
            HeaderText = "CONTENT TYPE",
            DataPropertyName = nameof(SensorAttachmentResponse.ContentType),
            Width = 180
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Size",
            HeaderText = "FILE SIZE",
            DataPropertyName = nameof(SensorAttachmentResponse.FormattedFileSize),
            Width = 120
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Uploaded",
            HeaderText = "UPLOADED AT",
            DataPropertyName = nameof(SensorAttachmentResponse.UploadedAtUtc),
            Width = 190,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm:ss" }
        });

        return grid;
    }

    private void RegisterEvents()
    {
        _browseButton.Click += (_, _) => BrowseForFile();
        _uploadButton.Click += async (_, _) => await RunBusyOperationAsync(UploadAsync);
        _refreshButton.Click += async (_, _) => await RunBusyOperationAsync(LoadAttachmentsAsync);
        _downloadButton.Click += async (_, _) => await DownloadSelectedAsync(false);
        _openButton.Click += async (_, _) => await DownloadSelectedAsync(true);
        _sensorComboBox.SelectedIndexChanged += async (_, _) =>
        {
            if (!_loadingSensors && !_isBusy)
            {
                await RunBusyOperationAsync(LoadAttachmentsAsync);
            }
        };
        _attachmentGrid.SelectionChanged += (_, _) => UpdateActionState();
        _attachmentGrid.CellDoubleClick += async (_, e) =>
        {
            if (e.RowIndex >= 0)
            {
                await DownloadSelectedAsync(true);
            }
        };
    }

    private async Task InitialiseAsync()
    {
        await RunBusyOperationAsync(async () =>
        {
            bool healthy = await _apiClient.IsHealthyAsync();
            SetApiStatus(healthy);

            if (!healthy)
            {
                throw new InvalidOperationException("Start NexaGrid.API on http://localhost:5211 before using attachments.");
            }

            await LoadSensorsAsync();
            await LoadAttachmentsAsync();
        });
    }

    private async Task LoadSensorsAsync()
    {
        string? previous = (_sensorComboBox.SelectedItem as SensorResponse)?.UniqueIdentifier;
        IReadOnlyList<SensorResponse> sensors = await _sensorService.GetAllAsync();

        _loadingSensors = true;
        try
        {
            _sensorComboBox.DataSource = null;
            _sensorComboBox.DisplayMember = nameof(SensorResponse.UniqueIdentifier);
            _sensorComboBox.ValueMember = nameof(SensorResponse.Id);
            _sensorComboBox.DataSource = sensors.ToList();

            if (!string.IsNullOrWhiteSpace(previous))
            {
                SensorResponse? match = sensors.FirstOrDefault(sensor =>
                    sensor.UniqueIdentifier.Equals(previous, StringComparison.OrdinalIgnoreCase));
                if (match is not null)
                {
                    _sensorComboBox.SelectedItem = match;
                }
            }
        }
        finally
        {
            _loadingSensors = false;
        }

        if (sensors.Count == 0)
        {
            ShowFeedback("NO SENSORS ARE REGISTERED. USE THE SENSOR REGISTRY FIRST.", false);
        }
    }

    private async Task LoadAttachmentsAsync()
    {
        SensorResponse? sensor = GetSelectedSensor();
        if (sensor is null)
        {
            _attachmentGrid.DataSource = null;
            _summaryLabel.Text = "0 ATTACHMENTS";
            ShowFeedback("SELECT A SENSOR TO VIEW ITS ATTACHMENTS.", false);
            return;
        }

        IReadOnlyList<SensorAttachmentResponse> attachments =
            await _attachmentService.GetBySensorAsync(sensor.Id);

        _attachmentGrid.DataSource = attachments
            .OrderByDescending(item => item.UploadedAtUtc)
            .ToList();
        _summaryLabel.Text = $"{attachments.Count} ATTACHMENT(S) / {sensor.UniqueIdentifier}";
        ShowFeedback(attachments.Count == 0
            ? "NO ATTACHMENTS HAVE BEEN UPLOADED FOR THIS SENSOR."
            : "SELECT A FILE TO DOWNLOAD OR DOUBLE-CLICK A ROW TO OPEN IT.", false);
    }

    private void BrowseForFile()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Select a sensor attachment",
            Filter = "Approved files|*.jpg;*.jpeg;*.png;*.pdf;*.txt;*.log;*.json;*.xml;*.csv;*.yaml;*.yml;*.conf|All files|*.*",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _filePathTextBox.Text = dialog.FileName;
            ShowFeedback($"READY TO UPLOAD: {Path.GetFileName(dialog.FileName)}", false);
        }
    }

    private async Task UploadAsync()
    {
        SensorResponse? sensor = GetSelectedSensor();
        if (sensor is null)
        {
            throw new InvalidOperationException("Select a sensor before uploading a file.");
        }

        if (string.IsNullOrWhiteSpace(_filePathTextBox.Text))
        {
            throw new InvalidOperationException("Browse for a file before uploading.");
        }

        var fileInfo = new FileInfo(_filePathTextBox.Text);
        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException("The selected file could not be found.");
        }

        if (fileInfo.Length > 10L * 1024L * 1024L)
        {
            throw new InvalidOperationException("The selected file exceeds the 10 MB limit.");
        }

        SensorAttachmentResponse uploaded =
            await _attachmentService.UploadAsync(sensor.Id, fileInfo.FullName);

        _filePathTextBox.Clear();
        await LoadAttachmentsAsync();
        ShowFeedback($"UPLOAD COMPLETE: {uploaded.OriginalFileName}", true);
    }

    private async Task DownloadSelectedAsync(bool openAfterDownload)
    {
        if (_isBusy)
        {
            return;
        }

        SensorAttachmentResponse? attachment = GetSelectedAttachment();
        if (attachment is null)
        {
            MessageBox.Show(this, "Select an attachment first.", "No attachment selected",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        string destination;
        if (openAfterDownload)
        {
            string folder = Path.Combine(Path.GetTempPath(), "NexaGrid", "Attachments");
            Directory.CreateDirectory(folder);
            destination = Path.Combine(folder,
                $"{attachment.Id}_{Path.GetFileName(attachment.OriginalFileName)}");
        }
        else
        {
            using var dialog = new SaveFileDialog
            {
                Title = "Save sensor attachment",
                FileName = attachment.OriginalFileName,
                Filter = "All files|*.*"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            destination = dialog.FileName;
        }

        await RunBusyOperationAsync(async () =>
        {
            await _attachmentService.DownloadAsync(attachment, destination);
            ShowFeedback($"DOWNLOAD COMPLETE: {Path.GetFileName(destination)}", true);

            if (openAfterDownload)
            {
                Process.Start(new ProcessStartInfo(destination) { UseShellExecute = true });
            }
        });
    }

    private SensorResponse? GetSelectedSensor() => _sensorComboBox.SelectedItem as SensorResponse;

    private SensorAttachmentResponse? GetSelectedAttachment()
    {
        return _attachmentGrid.CurrentRow?.DataBoundItem as SensorAttachmentResponse;
    }

    private async Task RunBusyOperationAsync(Func<Task> operation)
    {
        if (_isBusy || IsDisposed || Disposing)
        {
            return;
        }

        SetBusy(true);
        try
        {
            await operation();
        }
        catch (OperationCanceledException)
        {
            ShowFeedback("THE OPERATION WAS CANCELLED.", false);
        }
        catch (Exception exception)
        {
            ShowFeedback(exception.Message.ToUpperInvariant(), false, true);
            MessageBox.Show(this, exception.Message, "Attachment workspace",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool busy)
    {
        _isBusy = busy;
        UseWaitCursor = busy;
        _sensorComboBox.Enabled = !busy;
        _browseButton.Enabled = !busy;
        _uploadButton.Enabled = !busy;
        _refreshButton.Enabled = !busy;
        UpdateActionState();
    }

    private void UpdateActionState()
    {
        bool enabled = !_isBusy && GetSelectedAttachment() is not null;
        _downloadButton.Enabled = enabled;
        _openButton.Enabled = enabled;
    }

    private void SetApiStatus(bool online)
    {
        _apiStatusLabel.Text = online ? "API ONLINE" : "API OFFLINE";
        _apiStatusLabel.ForeColor = online ? SuccessColour : DangerColour;
    }

    private void ShowFeedback(string message, bool success, bool error = false)
    {
        _feedbackLabel.Text = message;
        _feedbackLabel.ForeColor = error ? DangerColour : success ? SuccessColour : SecondaryColour;
    }
}
