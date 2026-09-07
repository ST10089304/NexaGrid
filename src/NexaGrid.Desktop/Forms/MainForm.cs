using FontAwesome.Sharp;

using NexaGrid.Desktop.Controls;
using NexaGrid.Desktop.Services;
using NexaGrid.Desktop.Theme;

namespace NexaGrid.Desktop.Forms;

public sealed class MainForm : Form
{
    private readonly Panel _pageHost;
    private readonly ApiClient _apiClient;

    private Label _apiStatusLabel = null!;
    private Button _attachmentsButton = null!;
    private bool _workspaceIsOpen;

    public MainForm()
    {
        _apiClient =
            new ApiClient();

        AutoScaleMode =
            AutoScaleMode.Dpi;

        Text =
            "NexaGrid IoT Operations Platform";

        StartPosition =
            FormStartPosition.CenterScreen;

        MinimumSize =
            new Size(1280, 800);

        Size =
            new Size(1440, 880);

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
                BackColor =
                    AppPalette.Background,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

        rootLayout.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                28));

        rootLayout.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                88));

        rootLayout.RowStyles.Add(
            new RowStyle(
                SizeType.Percent,
                100));

        rootLayout.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                64));

        _pageHost =
            new Panel
            {
                Dock = DockStyle.Fill,
                BackColor =
                    AppPalette.Background,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

        rootLayout.Controls.Add(
            BuildAnnouncementBar(),
            0,
            0);

        rootLayout.Controls.Add(
            BuildHeader(),
            0,
            1);

        rootLayout.Controls.Add(
            _pageHost,
            0,
            2);

        rootLayout.Controls.Add(
            BuildFooter(),
            0,
            3);

        Controls.Add(
            rootLayout);

        Shown +=
            async (_, _) =>
                await CheckApiStatusAsync();

        FormClosed +=
            (_, _) =>
                _apiClient.Dispose();

        ShowGatewayPage();
    }

    private static Panel BuildAnnouncementBar()
    {
        var panel =
            new Panel
            {
                Dock = DockStyle.Fill,
                BackColor =
                    AppPalette.Black,
                Margin =
                    Padding.Empty
            };

        var leftLabel =
            new Label
            {
                Text =
                    "NEXAGRID / DISTRIBUTED INTELLIGENCE",
                ForeColor =
                    AppPalette.White,
                Font =
                    new Font(
                        "Segoe UI",
                        7,
                        FontStyle.Bold),
                AutoSize = true,
                Location =
                    new Point(38, 8)
            };

        var rightLabel =
            new Label
            {
                Text =
                    "PART 1 / DATA INGESTION GATEWAY",
                ForeColor =
                    AppPalette.LightGray,
                Font =
                    new Font(
                        "Segoe UI",
                        7,
                        FontStyle.Regular),
                AutoSize = true,
                Anchor =
                    AnchorStyles.Top
                    | AnchorStyles.Right
            };

        panel.Controls.Add(
            leftLabel);

        panel.Controls.Add(
            rightLabel);

        panel.Resize +=
            (_, _) =>
            {
                rightLabel.Location =
                    new Point(
                        panel.ClientSize.Width
                        - rightLabel.Width
                        - 38,
                        8);
            };

        return panel;
    }

    private Panel BuildHeader()
    {
        var panel =
            new Panel
            {
                Dock = DockStyle.Fill,
                BackColor =
                    AppPalette.Background,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

        Button gatewayButton =
            CreateNavigationButton(
                "GATEWAY",
                42);

        gatewayButton.Click +=
            (_, _) =>
                ShowGatewayPage();

        Button sensorsButton =
            CreateNavigationButton(
                "SENSORS",
                130);

        sensorsButton.Click +=
            (_, _) =>
                OpenSensorWorkspace();

        Button telemetryButton =
            CreateNavigationButton(
                "TELEMETRY",
                215);

        telemetryButton.Click +=
            (_, _) =>
                OpenTelemetryWorkspace();

        var brandButton =
            new Button
            {
                Text = "NEXAGRID",
                ForeColor =
                    AppPalette.Black,
                BackColor =
                    AppPalette.Background,
                Font =
                    AppFonts.Brand,
                FlatStyle =
                    FlatStyle.Flat,
                AutoSize = true,
                Cursor =
                    Cursors.Hand,
                Anchor =
                    AnchorStyles.Top,
                Location =
                    new Point(620, 19),
                UseVisualStyleBackColor =
                    false
            };

        brandButton.FlatAppearance.BorderSize = 0;

        brandButton.Click +=
            (_, _) =>
                ShowGatewayPage();

        _attachmentsButton =
            CreateNavigationButton(
                "ATTACHMENTS",
                0);

        _attachmentsButton.Anchor =
            AnchorStyles.Top
            | AnchorStyles.Right;

        _attachmentsButton.Click +=
            (_, _) =>
                ShowAttachmentIntroduction();

        _apiStatusLabel =
            new Label
            {
                Text = "API CHECKING",
                ForeColor =
                    AppPalette.DarkGray,
                Font =
                    AppFonts.Uppercase,
                AutoSize = true,
                Anchor =
                    AnchorStyles.Top
                    | AnchorStyles.Right
            };

        panel.Controls.Add(
            gatewayButton);

        panel.Controls.Add(
            sensorsButton);

        panel.Controls.Add(
            telemetryButton);

        panel.Controls.Add(
            brandButton);

        panel.Controls.Add(
            _attachmentsButton);

        panel.Controls.Add(
            _apiStatusLabel);

        panel.Resize +=
            (_, _) =>
            {
                brandButton.Left =
                    (panel.ClientSize.Width
                     - brandButton.Width) / 2;

                PositionHeaderActions();
            };

        _apiStatusLabel.TextChanged +=
            (_, _) =>
                PositionHeaderActions();

        return panel;
    }

    private static Button CreateNavigationButton(
        string text,
        int left)
    {
        var button =
            new Button
            {
                Text = text,
                ForeColor =
                    AppPalette.Black,
                BackColor =
                    AppPalette.Background,
                Font =
                    AppFonts.Uppercase,
                FlatStyle =
                    FlatStyle.Flat,
                AutoSize = true,
                Location =
                    new Point(left, 24),
                Cursor =
                    Cursors.Hand,
                UseVisualStyleBackColor =
                    false
            };

        button.FlatAppearance.BorderSize = 0;

        button.FlatAppearance.MouseOverBackColor =
            AppPalette.LightGray;

        button.FlatAppearance.MouseDownBackColor =
            AppPalette.MidGray;

        return button;
    }

    private void ShowGatewayPage()
    {
        _pageHost.Controls.Clear();

        var gatewayLayout =
            new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor =
                    AppPalette.Background,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

        gatewayLayout.RowStyles.Add(
            new RowStyle(
                SizeType.Percent,
                100));

        gatewayLayout.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                230));

        gatewayLayout.Controls.Add(
            BuildHero(),
            0,
            0);

        gatewayLayout.Controls.Add(
            BuildPillars(),
            0,
            1);

        _pageHost.Controls.Add(
            gatewayLayout);
    }

    private Panel BuildHero()
    {
        var hero =
            new Panel
            {
                Dock = DockStyle.Fill,
                BackColor =
                    AppPalette.Background,
                Padding =
                    new Padding(
                        55,
                        25,
                        55,
                        25)
            };

        var layout =
            new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor =
                    AppPalette.Background,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

        layout.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Percent,
                52));

        layout.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Percent,
                48));

        var copyPanel =
            new Panel
            {
                Dock = DockStyle.Fill,
                BackColor =
                    AppPalette.Background
            };

        var eyebrowLabel =
            new Label
            {
                Text =
                    "CONNECTED INFRASTRUCTURE / 2026",
                ForeColor =
                    AppPalette.DarkGray,
                Font =
                    AppFonts.Uppercase,
                AutoSize = true,
                Location =
                    new Point(0, 22)
            };

        var titleLabel =
            new Label
            {
                Text = "NEXA\nGRID",
                ForeColor =
                    AppPalette.Black,
                Font =
                    AppFonts.Display,
                AutoSize = true,
                Location =
                    new Point(-6, 67)
            };

        var descriptionLabel =
            new Label
            {
                Text =
                    "A precise operational layer for connected devices,"
                    + Environment.NewLine
                    + "sensor telemetry and distributed environments.",
                ForeColor =
                    AppPalette.DarkGray,
                Font =
                    AppFonts.Body,
                AutoSize = true,
                Location =
                    new Point(5, 285)
            };

        var enterButton =
            new Button
            {
                Text =
                    "ENTER SENSOR GATEWAY",
                BackColor =
                    AppPalette.Black,
                ForeColor =
                    AppPalette.White,
                FlatStyle =
                    FlatStyle.Flat,
                Font =
                    AppFonts.Button,
                Size =
                    new Size(220, 44),
                Location =
                    new Point(5, 345),
                Cursor =
                    Cursors.Hand,
                UseVisualStyleBackColor =
                    false
            };

        enterButton.FlatAppearance.BorderSize = 0;

        enterButton.FlatAppearance.MouseOverBackColor =
            AppPalette.DarkGray;

        enterButton.Click +=
            (_, _) =>
                OpenSensorWorkspace();

        copyPanel.Controls.Add(
            eyebrowLabel);

        copyPanel.Controls.Add(
            titleLabel);

        copyPanel.Controls.Add(
            descriptionLabel);

        copyPanel.Controls.Add(
            enterButton);

        copyPanel.Resize +=
            (_, _) =>
            {
                int buttonTop =
                    Math.Min(
                        345,
                        Math.Max(
                            300,
                            copyPanel.ClientSize.Height - 50));

                descriptionLabel.Top =
                    Math.Min(
                        285,
                        buttonTop - 60);

                enterButton.Top = buttonTop;
            };

        var networkVisual =
            new NetworkHeroVisual
            {
                Dock = DockStyle.Fill,
                Margin =
                    new Padding(
                        25,
                        5,
                        25,
                        5)
            };

        layout.Controls.Add(
            copyPanel,
            0,
            0);

        layout.Controls.Add(
            networkVisual,
            1,
            0);

        hero.Controls.Add(
            layout);

        return hero;
    }

    private TableLayoutPanel BuildPillars()
    {
        var table =
            new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor =
                    AppPalette.Black,
                ColumnCount = 3,
                RowCount = 1,
                Padding =
                    new Padding(
                        45,
                        14,
                        45,
                        14),
                Margin =
                    Padding.Empty
            };

        table.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Percent,
                33.333f));

        table.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Percent,
                33.333f));

        table.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Percent,
                33.334f));

        table.RowStyles.Add(
            new RowStyle(
                SizeType.Percent,
                100));

        var ingestionCard =
            new PillarCard(
                IconChar.WaveSquare,
                "01 / ACTIVE",
                "Sensor ingestion",
                "Register devices, process telemetry and identify anomalous readings.",
                true)
            {
                Margin =
                    new Padding(
                        0,
                        0,
                        10,
                        0)
            };

        ingestionCard.OpenRequested +=
            (_, _) =>
                OpenSensorWorkspace();

        var commandCard =
            new PillarCard(
                IconChar.SatelliteDish,
                "02 / PART TWO",
                "Command stream",
                "Transmit device instructions and maintain an operational command history.",
                false)
            {
                Margin =
                    new Padding(
                        5,
                        0,
                        5,
                        0)
            };

        var topologyCard =
            new PillarCard(
                IconChar.DiagramProject,
                "03 / FINAL POE",
                "Network topology",
                "Inspect node hierarchies, gateways and distributed mesh connections.",
                false)
            {
                Margin =
                    new Padding(
                        10,
                        0,
                        0,
                        0)
            };

        table.Controls.Add(
            ingestionCard,
            0,
            0);

        table.Controls.Add(
            commandCard,
            1,
            0);

        table.Controls.Add(
            topologyCard,
            2,
            0);

        return table;
    }

    private void OpenSensorWorkspace()
    {
        OpenWorkspace(
            () => new SensorWorkspaceForm());
    }

    private void OpenTelemetryWorkspace()
    {
        OpenWorkspace(
            () => new TelemetryWorkspaceForm());
    }

    private void OpenWorkspace(
        Func<Form> createWorkspace)
    {
        if (_workspaceIsOpen)
        {
            return;
        }

        try
        {
            _workspaceIsOpen = true;

            using Form workspace =
                createWorkspace();

            workspace.ShowDialog(this);
        }
        finally
        {
            _workspaceIsOpen = false;

            _ =
                CheckApiStatusAsync();
        }
    }

    private void ShowAttachmentIntroduction()
    {
        ShowFeaturePage(
            "ATTACHMENTS",
            "SENSOR FILES AND DEPLOYMENT EVIDENCE",
            "Upload and retrieve configuration files, deployment photographs and hardware logs.");
    }

    private void ShowFeaturePage(
        string title,
        string subtitle,
        string description)
    {
        _pageHost.Controls.Clear();

        var panel =
            new Panel
            {
                Dock = DockStyle.Fill,
                BackColor =
                    AppPalette.Background,
                Padding =
                    new Padding(55)
            };

        var subtitleLabel =
            new Label
            {
                Text =
                    subtitle.ToUpperInvariant(),
                ForeColor =
                    AppPalette.DarkGray,
                Font =
                    AppFonts.Uppercase,
                AutoSize = true,
                Location =
                    new Point(55, 45)
            };

        var titleLabel =
            new Label
            {
                Text =
                    title.ToUpperInvariant(),
                ForeColor =
                    AppPalette.Black,
                Font =
                    AppFonts.PageTitle,
                AutoSize = true,
                Location =
                    new Point(51, 75)
            };

        var descriptionLabel =
            new Label
            {
                Text = description,
                ForeColor =
                    AppPalette.DarkGray,
                Font =
                    AppFonts.Body,
                AutoSize = true,
                MaximumSize =
                    new Size(650, 0),
                Location =
                    new Point(55, 140)
            };

        var statusLabel =
            new Label
            {
                Text =
                    "WORKSPACE INTERFACE PENDING",
                ForeColor =
                    AppPalette.DarkGray,
                Font =
                    AppFonts.Uppercase,
                AutoSize = true,
                Location =
                    new Point(55, 210)
            };

        var backButton =
            new Button
            {
                Text =
                    "RETURN TO GATEWAY",
                BackColor =
                    AppPalette.Black,
                ForeColor =
                    AppPalette.White,
                FlatStyle =
                    FlatStyle.Flat,
                Font =
                    AppFonts.Button,
                Size =
                    new Size(205, 42),
                Location =
                    new Point(55, 260),
                Cursor =
                    Cursors.Hand,
                UseVisualStyleBackColor =
                    false
            };

        backButton.FlatAppearance.BorderSize = 0;

        backButton.Click +=
            (_, _) =>
                ShowGatewayPage();

        panel.Controls.Add(
            subtitleLabel);

        panel.Controls.Add(
            titleLabel);

        panel.Controls.Add(
            descriptionLabel);

        panel.Controls.Add(
            statusLabel);

        panel.Controls.Add(
            backButton);

        _pageHost.Controls.Add(
            panel);
    }

    private static Panel BuildFooter()
    {
        var footer =
            new Panel
            {
                Dock = DockStyle.Fill,
                BackColor =
                    AppPalette.Background,
                Padding =
                    new Padding(
                        45,
                        8,
                        45,
                        8),
                Margin =
                    Padding.Empty
            };

        var topBorder =
            new Panel
            {
                Dock = DockStyle.Top,
                Height = 5,
                BackColor =
                    AppPalette.Black
            };

        var featureLayout =
            new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor =
                    AppPalette.Background,
                Padding =
                    new Padding(0, 8, 0, 0)
            };

        for (int index = 0;
             index < 4;
             index++)
        {
            featureLayout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    25));
        }

        featureLayout.Controls.Add(
            CreateFooterLabel(
                "01  STRONGLY TYPED DATA"),
            0,
            0);

        featureLayout.Controls.Add(
            CreateFooterLabel(
                "02  ANOMALY DETECTION"),
            1,
            0);

        featureLayout.Controls.Add(
            CreateFooterLabel(
                "03  SQL PERSISTENCE"),
            2,
            0);

        featureLayout.Controls.Add(
            CreateFooterLabel(
                "04  SECURE ATTACHMENTS"),
            3,
            0);

        footer.Controls.Add(
            featureLayout);

        footer.Controls.Add(
            topBorder);

        return footer;
    }

    private static Label CreateFooterLabel(
        string text)
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Text = text,
            TextAlign =
                ContentAlignment.TopLeft,
            ForeColor =
                AppPalette.Black,
            Font =
                new Font(
                    "Segoe UI",
                    7,
                    FontStyle.Bold),
            AutoSize = false
        };
    }

    private async Task CheckApiStatusAsync()
    {
        try
        {
            bool healthy =
                await _apiClient.IsHealthyAsync();

            if (IsDisposed)
            {
                return;
            }

            _apiStatusLabel.Text =
                healthy
                    ? "API ONLINE"
                    : "API OFFLINE";

            _apiStatusLabel.ForeColor =
                healthy
                    ? Color.FromArgb(
                        48,
                        104,
                        70)
                    : Color.FromArgb(
                        174,
                        54,
                        42);

            PositionApiStatusLabel();
        }
        catch
        {
            if (!IsDisposed)
            {
                _apiStatusLabel.Text =
                    "API OFFLINE";

                _apiStatusLabel.ForeColor =
                    Color.FromArgb(
                        174,
                        54,
                        42);

                PositionApiStatusLabel();
            }
        }
    }

    private void PositionApiStatusLabel()
    {
        PositionHeaderActions();
    }

    private void PositionHeaderActions()
    {
        if (_apiStatusLabel.Parent is not Control parent
            || _attachmentsButton is null)
        {
            return;
        }

        _apiStatusLabel.Location =
            new Point(
                parent.ClientSize.Width
                - _apiStatusLabel.Width
                - 38,
                36);

        _attachmentsButton.Location =
            new Point(
                _apiStatusLabel.Left
                - _attachmentsButton.Width
                - 28,
                24);
    }
}
