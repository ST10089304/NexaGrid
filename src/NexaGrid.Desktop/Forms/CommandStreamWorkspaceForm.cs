using NexaGrid.Desktop.Services;

using NexaGrid.Desktop.Theme;

using NexaGrid.Shared.DTOs.CommandStream;

using NexaGrid.Shared.DTOs.Predictive;

using NexaGrid.Shared.Enums;

namespace NexaGrid.Desktop.Forms;

public sealed class CommandStreamWorkspaceForm : Form

{

    private const string OperatorIdentifier = "operator-desktop";

    private readonly ApiClient _apiClient;

    private readonly DeviceCommandApiService _commandService;

    private readonly PredictiveActionApiService _predictiveService;

    private readonly TextBox _targetTextBox;

    private readonly ComboBox _commandTypeComboBox;

    private readonly ComboBox _priorityComboBox;

    private readonly TextBox _argumentTextBox;

    private readonly NumericUpDown _activeAlertInput;

    private readonly DataGridView _pendingGrid;

    private readonly DataGridView _historyGrid;

    private readonly FlowLayoutPanel _suggestionsPanel;

    private readonly Label _pendingCountLabel;

    private readonly Label _historyCountLabel;

    private readonly Label _statusLabel;

    private readonly Button _queueButton;

    private readonly Button _processButton;

    private readonly Button _undoButton;

    private readonly Button _refreshButton;

    private bool _operationInProgress;

    public CommandStreamWorkspaceForm()

    {

        _apiClient = new ApiClient();

        _commandService =

            new DeviceCommandApiService(_apiClient);

        _predictiveService =

            new PredictiveActionApiService(_apiClient);

        Text = "NexaGrid Command Stream";

        StartPosition =

            FormStartPosition.CenterParent;

        MinimumSize =

            new Size(1280, 820);

        Size =

            new Size(1480, 920);

        BackColor =

            AppPalette.Background;

        ForeColor =

            AppPalette.Black;

        Font =

            AppFonts.Body;

        var rootLayout =

            new TableLayoutPanel

            {

                Dock = DockStyle.Fill,

                ColumnCount = 1,

                RowCount = 4,

                BackColor = AppPalette.Background,

                Padding = Padding.Empty,

                Margin = Padding.Empty

            };

        rootLayout.RowStyles.Add(

            new RowStyle(SizeType.Absolute, 28));

        rootLayout.RowStyles.Add(

            new RowStyle(SizeType.Absolute, 106));

        rootLayout.RowStyles.Add(

            new RowStyle(SizeType.Percent, 100));

        rootLayout.RowStyles.Add(

            new RowStyle(SizeType.Absolute, 48));

        rootLayout.Controls.Add(

            BuildAnnouncementBar(),

            0,

            0);

        rootLayout.Controls.Add(

            BuildHeader(),

            0,

            1);

        var contentLayout =

            new TableLayoutPanel

            {

                Dock = DockStyle.Fill,

                ColumnCount = 2,

                RowCount = 1,

                BackColor = AppPalette.Background,

                Padding = new Padding(38, 12, 38, 18),

                Margin = Padding.Empty

            };

        contentLayout.ColumnStyles.Add(

            new ColumnStyle(SizeType.Percent, 34));

        contentLayout.ColumnStyles.Add(

            new ColumnStyle(SizeType.Percent, 66));

        _targetTextBox =

            CreateTextBox("HUM-001");

        _commandTypeComboBox =

            CreateEnumComboBox<DeviceCommandType>();

        _priorityComboBox =

            CreateEnumComboBox<CommandPriority>();

        _priorityComboBox.SelectedItem =

            CommandPriority.Normal;

        _argumentTextBox =

            CreateTextBox(

                "Enter an optional command argument");

        _activeAlertInput =

            new NumericUpDown

            {

                Dock = DockStyle.Fill,

                Minimum = 0,

                Maximum = 1000,

                Value = 0,

                Font = AppFonts.Body,

                BackColor = AppPalette.White,

                ForeColor = AppPalette.Black,

                BorderStyle = BorderStyle.FixedSingle,

                Margin = new Padding(0, 3, 0, 12)

            };

        _queueButton =

            CreatePrimaryButton("QUEUE COMMAND");

        _processButton =

            CreateSecondaryButton("PROCESS NEXT");

        _undoButton =

            CreateSecondaryButton("UNDO LAST");

        _refreshButton =

            CreateSecondaryButton("REFRESH");

        _pendingGrid =

            CreateCommandGrid();

        _historyGrid =

            CreateCommandGrid();

        _suggestionsPanel =

            new FlowLayoutPanel

            {

                Dock = DockStyle.Fill,

                FlowDirection = FlowDirection.TopDown,

                WrapContents = false,

                AutoScroll = true,

                BackColor = AppPalette.Paper,

                Padding = new Padding(12),

                Margin = Padding.Empty

            };

        _pendingCountLabel =

            CreateCountLabel("0 PENDING");

        _historyCountLabel =

            CreateCountLabel("0 HISTORY");

        contentLayout.Controls.Add(

            BuildControlPanel(),

            0,

            0);

        contentLayout.Controls.Add(

            BuildMonitoringPanel(),

            1,

            0);

        rootLayout.Controls.Add(

            contentLayout,

            0,

            2);

        _statusLabel =

            new Label

            {

                Dock = DockStyle.Fill,

                Text = "READY / WAITING FOR OPERATOR INPUT",

                TextAlign = ContentAlignment.MiddleLeft,

                BackColor = AppPalette.Black,

                ForeColor = AppPalette.White,

                Font = AppFonts.Uppercase,

                Padding = new Padding(38, 0, 0, 0)

            };

        rootLayout.Controls.Add(

            _statusLabel,

            0,

            3);

        Controls.Add(rootLayout);

        _queueButton.Click +=

            async (_, _) =>

                await QueueCommandAsync();

        _processButton.Click +=

            async (_, _) =>

                await ProcessNextAsync();

        _undoButton.Click +=

            async (_, _) =>

                await UndoLastAsync();

        _refreshButton.Click +=

            async (_, _) =>

                await RefreshWorkspaceAsync();

        _activeAlertInput.ValueChanged +=

            async (_, _) =>

                await LoadSuggestionsAsync();

        Shown +=

            async (_, _) =>

                await RefreshWorkspaceAsync();

        FormClosed +=

            (_, _) =>

                _apiClient.Dispose();

    }

    private static Panel BuildAnnouncementBar()

    {

        var panel =

            new Panel

            {

                Dock = DockStyle.Fill,

                BackColor = AppPalette.Black,

                Margin = Padding.Empty

            };

        panel.Controls.Add(

            new Label

            {

                Text =

                    "NEXAGRID / REAL-TIME OPERATIONS",

                ForeColor = AppPalette.White,

                Font = new Font(

                    "Segoe UI",

                    7,

                    FontStyle.Bold),

                AutoSize = true,

                Location = new Point(38, 8)

            });

        return panel;

    }

    private static Panel BuildHeader()

    {

        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppPalette.Background,
            Padding = new Padding(38, 18, 38, 8),
            Margin = Padding.Empty
        };

        var titleLabel = new Label
        {
            AutoSize = true,
            Location = new Point(50, 16),
            Font = AppFonts.PageTitle,
            ForeColor = AppPalette.Black,
            BackColor = Color.Transparent,
            Text = "COMMAND STREAM"
        };

        var subtitleLabel = new Label
        {
            AutoSize = false,
            Location = new Point(52, 66),
            Size = new Size(1_180, 30),
            Font = AppFonts.Body,
            ForeColor = AppPalette.DarkGray,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
            Text = "Prioritised device instructions, command history, undo operations and predictive guidance."
        };

        panel.Controls.Add(titleLabel);
        panel.Controls.Add(subtitleLabel);

        titleLabel.BringToFront();
        subtitleLabel.BringToFront();

        return panel;

    }

    private Control BuildControlPanel()

    {

        var container =

            new Panel

            {

                Dock = DockStyle.Fill,

                BackColor = AppPalette.Paper,

                Padding = new Padding(22),

                Margin = new Padding(0, 0, 14, 0)

            };

        var layout =

            new TableLayoutPanel

            {

                Dock = DockStyle.Fill,

                ColumnCount = 1,

                RowCount = 13,

                BackColor = AppPalette.Paper,

                Margin = Padding.Empty,

                Padding = Padding.Empty

            };

        layout.RowStyles.Add(

            new RowStyle(SizeType.Absolute, 42));

        for (int index = 1; index < 10; index++)

        {

            layout.RowStyles.Add(

                new RowStyle(

                    index % 2 == 0

                        ? SizeType.Absolute

                        : SizeType.Absolute,

                    index % 2 == 0 ? 48 : 25));

        }

        layout.RowStyles.Add(

            new RowStyle(SizeType.Absolute, 54));

        layout.RowStyles.Add(

            new RowStyle(SizeType.Absolute, 54));

        layout.RowStyles.Add(

            new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(

            CreateSectionTitle(

                "ISSUE DEVICE COMMAND"),

            0,

            0);

        layout.Controls.Add(

            CreateFieldLabel("TARGET IDENTIFIER"),

            0,

            1);

        layout.Controls.Add(

            _targetTextBox,

            0,

            2);

        layout.Controls.Add(

            CreateFieldLabel("COMMAND TYPE"),

            0,

            3);

        layout.Controls.Add(

            _commandTypeComboBox,

            0,

            4);

        layout.Controls.Add(

            CreateFieldLabel("PRIORITY"),

            0,

            5);

        layout.Controls.Add(

            _priorityComboBox,

            0,

            6);

        layout.Controls.Add(

            CreateFieldLabel("ARGUMENT"),

            0,

            7);

        layout.Controls.Add(

            _argumentTextBox,

            0,

            8);

        layout.Controls.Add(

            CreateFieldLabel("ACTIVE ALERT COUNT"),

            0,

            9);

        layout.Controls.Add(

            _activeAlertInput,

            0,

            10);

        layout.Controls.Add(

            _queueButton,

            0,

            11);

        var actionLayout =

            new TableLayoutPanel

            {

                Dock = DockStyle.Fill,

                ColumnCount = 3,

                RowCount = 1,

                Margin = new Padding(0, 8, 0, 0)

            };

        actionLayout.ColumnStyles.Add(

            new ColumnStyle(SizeType.Percent, 34));

        actionLayout.ColumnStyles.Add(

            new ColumnStyle(SizeType.Percent, 33));

        actionLayout.ColumnStyles.Add(

            new ColumnStyle(SizeType.Percent, 33));

        _processButton.Margin =

            new Padding(0, 0, 4, 0);

        _undoButton.Margin =

            new Padding(4, 0, 4, 0);

        _refreshButton.Margin =

            new Padding(4, 0, 0, 0);

        actionLayout.Controls.Add(

            _processButton,

            0,

            0);

        actionLayout.Controls.Add(

            _undoButton,

            1,

            0);

        actionLayout.Controls.Add(

            _refreshButton,

            2,

            0);

        layout.Controls.Add(

            actionLayout,

            0,

            12);

        container.Controls.Add(layout);

        return container;

    }

    private Control BuildMonitoringPanel()

    {

        var tabs =

            new TabControl

            {

                Dock = DockStyle.Fill,

                Font = AppFonts.BodyMedium,

                Appearance = TabAppearance.Normal,

                Margin = Padding.Empty

            };

        var pendingPage =

            CreateTabPage("PENDING COMMANDS");

        pendingPage.Controls.Add(

            BuildGridPage(

                _pendingGrid,

                _pendingCountLabel));

        var historyPage =

            CreateTabPage("COMMAND HISTORY");

        historyPage.Controls.Add(

            BuildGridPage(

                _historyGrid,

                _historyCountLabel));

        var suggestionsPage =

            CreateTabPage("PREDICTIVE ACTIONS");

        suggestionsPage.Controls.Add(

            _suggestionsPanel);

        tabs.TabPages.Add(pendingPage);

        tabs.TabPages.Add(historyPage);

        tabs.TabPages.Add(suggestionsPage);

        return tabs;

    }

    private static Control BuildGridPage(

        DataGridView grid,

        Label countLabel)

    {

        var layout =

            new TableLayoutPanel

            {

                Dock = DockStyle.Fill,

                ColumnCount = 1,

                RowCount = 2,

                BackColor = AppPalette.Paper,

                Padding = new Padding(12)

            };

        layout.RowStyles.Add(

            new RowStyle(SizeType.Absolute, 38));

        layout.RowStyles.Add(

            new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(

            countLabel,

            0,

            0);

        layout.Controls.Add(

            grid,

            0,

            1);

        return layout;

    }

    private static TabPage CreateTabPage(string text)

    {

        return new TabPage

        {

            Text = text,

            BackColor = AppPalette.Paper,

            ForeColor = AppPalette.Black,

            Padding = new Padding(4)

        };

    }

    private static DataGridView CreateCommandGrid()

    {

        var grid =

            new DataGridView

            {

                Dock = DockStyle.Fill,

                AutoGenerateColumns = false,

                AllowUserToAddRows = false,

                AllowUserToDeleteRows = false,

                AllowUserToResizeRows = false,

                ReadOnly = true,

                MultiSelect = false,

                SelectionMode =

                    DataGridViewSelectionMode.FullRowSelect,

                RowHeadersVisible = false,

                BackgroundColor = AppPalette.Paper,

                BorderStyle = BorderStyle.None,

                GridColor = AppPalette.LightGray,

                EnableHeadersVisualStyles = false,

                AutoSizeColumnsMode =

                    DataGridViewAutoSizeColumnsMode.Fill

            };

        grid.ColumnHeadersDefaultCellStyle =

            new DataGridViewCellStyle

            {

                BackColor = AppPalette.Black,

                ForeColor = AppPalette.White,

                Font = AppFonts.Uppercase,

                SelectionBackColor = AppPalette.Black,

                SelectionForeColor = AppPalette.White,

                Padding = new Padding(4)

            };

        grid.DefaultCellStyle =

            new DataGridViewCellStyle

            {

                BackColor = AppPalette.White,

                ForeColor = AppPalette.Black,

                SelectionBackColor = AppPalette.LightGray,

                SelectionForeColor = AppPalette.Black,

                Font = AppFonts.Body,

                Padding = new Padding(4)

            };

        grid.RowTemplate.Height = 34;

        grid.Columns.Add(

            CreateTextColumn(

                "TargetIdentifier",

                "TARGET",

                19));

        grid.Columns.Add(

            CreateTextColumn(

                "CommandType",

                "COMMAND",

                15));

        grid.Columns.Add(

            CreateTextColumn(

                "Priority",

                "PRIORITY",

                12));

        grid.Columns.Add(

            CreateTextColumn(

                "Status",

                "STATUS",

                13));

        grid.Columns.Add(

            CreateTextColumn(

                "Argument",

                "ARGUMENT",

                23));

        grid.Columns.Add(

            CreateTextColumn(

                "RequestedAtUtc",

                "REQUESTED UTC",

                18,

                "yyyy-MM-dd HH:mm:ss"));

        return grid;

    }

    private static DataGridViewTextBoxColumn CreateTextColumn(

        string propertyName,

        string heading,

        float fillWeight,

        string? format = null)

    {

        var column =

            new DataGridViewTextBoxColumn

            {

                DataPropertyName = propertyName,

                HeaderText = heading,

                FillWeight = fillWeight,

                SortMode =

                    DataGridViewColumnSortMode.Automatic

            };

        if (!string.IsNullOrWhiteSpace(format))

        {

            column.DefaultCellStyle.Format = format;

        }

        return column;

    }

    private static ComboBox CreateEnumComboBox<TEnum>()

        where TEnum : struct, Enum

    {

        var comboBox =

            new ComboBox

            {

                Dock = DockStyle.Fill,

                DropDownStyle =

                    ComboBoxStyle.DropDownList,

                Font = AppFonts.Body,

                BackColor = AppPalette.White,

                ForeColor = AppPalette.Black,

                Margin = new Padding(0, 3, 0, 10)

            };

        comboBox.DataSource =

            Enum.GetValues<TEnum>();

        return comboBox;

    }

    private static TextBox CreateTextBox(

        string placeholderText)

    {

        return new TextBox

        {

            Dock = DockStyle.Fill,

            PlaceholderText = placeholderText,

            Font = AppFonts.Body,

            BackColor = AppPalette.White,

            ForeColor = AppPalette.Black,

            BorderStyle = BorderStyle.FixedSingle,

            Margin = new Padding(0, 3, 0, 10)

        };

    }

    private static Label CreateSectionTitle(string text)

    {

        return new Label

        {

            Dock = DockStyle.Fill,

            Text = text,

            ForeColor = AppPalette.Black,

            Font = AppFonts.Heading,

            TextAlign = ContentAlignment.MiddleLeft

        };

    }

    private static Label CreateFieldLabel(string text)

    {

        return new Label

        {

            Dock = DockStyle.Fill,

            Text = text,

            ForeColor = AppPalette.DarkGray,

            Font = AppFonts.Uppercase,

            TextAlign = ContentAlignment.BottomLeft

        };

    }

    private static Label CreateCountLabel(string text)

    {

        return new Label

        {

            Dock = DockStyle.Fill,

            Text = text,

            ForeColor = AppPalette.DarkGray,

            Font = AppFonts.Uppercase,

            TextAlign = ContentAlignment.MiddleLeft

        };

    }

    private static Button CreatePrimaryButton(string text)

    {

        var button =

            new Button

            {

                Dock = DockStyle.Fill,

                Text = text,

                BackColor = AppPalette.Black,

                ForeColor = AppPalette.White,

                Font = AppFonts.Button,

                FlatStyle = FlatStyle.Flat,

                Cursor = Cursors.Hand,

                UseVisualStyleBackColor = false,

                Margin = new Padding(0, 8, 0, 0)

            };

        button.FlatAppearance.BorderSize = 0;

        button.FlatAppearance.MouseOverBackColor =

            AppPalette.DarkGray;

        return button;

    }

    private static Button CreateSecondaryButton(string text)

    {

        var button =

            new Button

            {

                Dock = DockStyle.Fill,

                Text = text,

                BackColor = AppPalette.Paper,

                ForeColor = AppPalette.Black,

                Font = AppFonts.Button,

                FlatStyle = FlatStyle.Flat,

                Cursor = Cursors.Hand,

                UseVisualStyleBackColor = false

            };

        button.FlatAppearance.BorderColor =

            AppPalette.Black;

        button.FlatAppearance.BorderSize = 1;

        button.FlatAppearance.MouseOverBackColor =

            AppPalette.LightGray;

        return button;

    }

    private async Task QueueCommandAsync()

    {

        if (_operationInProgress)

        {

            return;

        }

        string targetIdentifier =

            _targetTextBox.Text.Trim();

        if (targetIdentifier.Length < 3)

        {

            ShowError(

                "Enter a target identifier containing at least three characters.");

            _targetTextBox.Focus();

            return;

        }

        if (_commandTypeComboBox.SelectedItem

            is not DeviceCommandType commandType)

        {

            ShowError("Select a valid command type.");

            return;

        }

        if (_priorityComboBox.SelectedItem

            is not CommandPriority priority)

        {

            ShowError("Select a valid command priority.");

            return;

        }

        var request =

            new IssueDeviceCommandRequest

            {

                TargetIdentifier =

                    targetIdentifier,

                CommandType =

                    commandType,

                Priority =

                    priority,

                Argument =

                    string.IsNullOrWhiteSpace(

                        _argumentTextBox.Text)

                        ? null

                        : _argumentTextBox.Text.Trim()

            };

        await RunOperationAsync(

            "QUEUEING DEVICE COMMAND",

            async () =>

            {

                DeviceCommandResponse result =

                    await _commandService.IssueAsync(

                        request,

                        OperatorIdentifier);

                _statusLabel.Text =

                    $"QUEUED / {result.TargetIdentifier} / "

                    + $"{result.CommandType} / {result.Priority}";

                _argumentTextBox.Clear();

                await RefreshDataAsync();

                await LoadSuggestionsAsync();

            });

    }

    private async Task ProcessNextAsync()

    {

        await RunOperationAsync(

            "PROCESSING NEXT COMMAND",

            async () =>

            {

                DeviceCommandResponse result =

                    await _commandService.ProcessNextAsync();

                _statusLabel.Text =

                    $"COMPLETED / {result.TargetIdentifier} / "

                    + $"{result.CommandType}";

                await RefreshDataAsync();

            });

    }

    private async Task UndoLastAsync()

    {

        await RunOperationAsync(

            "UNDOING LAST COMMAND",

            async () =>

            {

                DeviceCommandResponse result =

                    await _commandService.UndoLastAsync();

                _statusLabel.Text =

                    $"UNDONE / {result.TargetIdentifier} / "

                    + $"{result.CommandType}";

                await RefreshDataAsync();

            });

    }

    private async Task RefreshWorkspaceAsync()

    {

        await RunOperationAsync(

            "REFRESHING COMMAND STREAM",

            async () =>

            {

                await RefreshDataAsync();

                await LoadSuggestionsAsync();

                _statusLabel.Text =

                    "READY / COMMAND STREAM SYNCHRONISED";

            });

    }

    private async Task RefreshDataAsync()

    {

        IReadOnlyList<DeviceCommandResponse> pending =

            await _commandService.GetPendingAsync();

        IReadOnlyList<DeviceCommandResponse> history =

            await _commandService.GetHistoryAsync();

        _pendingGrid.DataSource =

            pending.ToList();

        _historyGrid.DataSource =

            history.ToList();

        _pendingCountLabel.Text =

            $"{pending.Count} PENDING COMMAND(S)";

        _historyCountLabel.Text =

            $"{history.Count} HISTORY RECORD(S)";

        _processButton.Enabled =

            pending.Count > 0;

        _undoButton.Enabled =

            history.Count > 0;

    }

    private async Task LoadSuggestionsAsync()

    {

        if (_operationInProgress

            || IsDisposed)

        {

            return;

        }

        try

        {

            IReadOnlyList<PredictiveSuggestionResponse>

                suggestions =

                    await _predictiveService.GetSuggestionsAsync(

                        OperatorIdentifier,

                        (int)_activeAlertInput.Value);

            RenderSuggestions(suggestions);

        }

        catch (Exception exception)

        {

            RenderSuggestionError(exception.Message);

        }

    }

    private void RenderSuggestions(

        IReadOnlyList<PredictiveSuggestionResponse> suggestions)

    {

        _suggestionsPanel.SuspendLayout();

        _suggestionsPanel.Controls.Clear();

        if (suggestions.Count == 0)

        {

            _suggestionsPanel.Controls.Add(

                CreateEmptySuggestionLabel());

            _suggestionsPanel.ResumeLayout();

            return;

        }

        foreach (PredictiveSuggestionResponse suggestion

                 in suggestions)

        {

            _suggestionsPanel.Controls.Add(

                CreateSuggestionCard(suggestion));

        }

        _suggestionsPanel.ResumeLayout();

    }

    private Control CreateSuggestionCard(

        PredictiveSuggestionResponse suggestion)

    {

        var card =

            new Panel

            {

                Width = Math.Max(

                    500,

                    _suggestionsPanel.ClientSize.Width - 48),

                Height = 160,

                BackColor = AppPalette.White,

                Margin = new Padding(0, 0, 0, 12),

                Padding = new Padding(16)

            };

        var priorityLabel =

            new Label

            {

                Text =

                    $"{suggestion.RecommendedPriority}"

                    + $" / {suggestion.ConfidenceScore:P0}",

                ForeColor =

                    suggestion.RecommendedPriority

                        is CommandPriority.Critical

                        or CommandPriority.High

                        ? AppPalette.Danger

                        : AppPalette.Success,

                Font = AppFonts.Uppercase,

                AutoSize = true,

                Location = new Point(16, 14)

            };

        var titleLabel =

            new Label

            {

                Text = suggestion.Title,

                ForeColor = AppPalette.Black,

                Font = AppFonts.Heading,

                AutoSize = true,

                Location = new Point(16, 38)

            };

        var descriptionLabel =

            new Label

            {

                Text = suggestion.Description,

                ForeColor = AppPalette.DarkGray,

                Font = AppFonts.Body,

                AutoSize = true,

                MaximumSize =

                    new Size(card.Width - 32, 0),

                Location = new Point(16, 72)

            };

        var reasonLabel =

            new Label

            {

                Text =

                    $"REASON / {suggestion.Reason}",

                ForeColor = AppPalette.MidGray,

                Font = AppFonts.Caption,

                AutoSize = true,

                MaximumSize =

                    new Size(card.Width - 32, 0),

                Location = new Point(16, 116)

            };

        card.Controls.Add(priorityLabel);

        card.Controls.Add(titleLabel);

        card.Controls.Add(descriptionLabel);

        card.Controls.Add(reasonLabel);

        card.Resize +=

            (_, _) =>

            {

                descriptionLabel.MaximumSize =

                    new Size(card.ClientSize.Width - 32, 0);

                reasonLabel.MaximumSize =

                    new Size(card.ClientSize.Width - 32, 0);

            };

        return card;

    }

    private static Label CreateEmptySuggestionLabel()

    {

        return new Label

        {

            Text =

                "NO PREDICTIVE ACTIONS AVAILABLE"

                + Environment.NewLine

                + Environment.NewLine

                + "Repeated searches, issued commands and active "

                + "alerts will generate personalised operational guidance.",

            ForeColor = AppPalette.DarkGray,

            Font = AppFonts.Body,

            AutoSize = true,

            MaximumSize = new Size(650, 0),

            Padding = new Padding(12)

        };

    }

    private void RenderSuggestionError(string message)

    {

        _suggestionsPanel.Controls.Clear();

        _suggestionsPanel.Controls.Add(

            new Label

            {

                Text =

                    "PREDICTIVE SERVICE UNAVAILABLE"

                    + Environment.NewLine

                    + message,

                ForeColor = AppPalette.Danger,

                Font = AppFonts.Body,

                AutoSize = true,

                MaximumSize = new Size(650, 0),

                Padding = new Padding(12)

            });

    }

    private async Task RunOperationAsync(

        string operationMessage,

        Func<Task> operation)

    {

        if (_operationInProgress)

        {

            return;

        }

        try

        {

            SetBusyState(true);

            _statusLabel.Text = operationMessage;

            await operation();

        }

        catch (ApiException exception)

        {

            ShowError(exception.Message);

        }

        catch (Exception exception)

        {

            ShowError(

                "The operation could not be completed. "

                + exception.Message);

        }

        finally

        {

            SetBusyState(false);

        }

    }

    private void SetBusyState(bool isBusy)

    {

        _operationInProgress = isBusy;

        _queueButton.Enabled = !isBusy;

        _refreshButton.Enabled = !isBusy;

        if (isBusy)

        {

            _processButton.Enabled = false;

            _undoButton.Enabled = false;

        }

        UseWaitCursor = isBusy;

    }

    private void ShowError(string message)

    {

        _statusLabel.Text =

            $"ERROR / {message.ToUpperInvariant()}";

        MessageBox.Show(

            this,

            message,

            "NexaGrid Command Stream",

            MessageBoxButtons.OK,

            MessageBoxIcon.Warning);

    }

}
