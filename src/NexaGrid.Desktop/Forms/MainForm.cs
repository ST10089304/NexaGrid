using FontAwesome.Sharp;
using NexaGrid.Desktop.Controls;
using NexaGrid.Desktop.Theme;

namespace NexaGrid.Desktop.Forms;

public class MainForm : Form
{
    private readonly Panel _pageHost;
    private Label _apiStatusLabel = null!;

    public MainForm()
    {
        Text = "NexaGrid IoT Operations Platform";
        StartPosition =
            FormStartPosition.CenterScreen;
        MinimumSize =
            new Size(1180, 760);
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

        Controls.Add(rootLayout);

        ShowGatewayPage();
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
                    AnchorStyles.Top |
                    AnchorStyles.Right
            };

        panel.Controls.Add(leftLabel);
        panel.Controls.Add(rightLabel);

        panel.Resize += (_, _) =>
        {
            rightLabel.Location =
                new Point(
                    panel.ClientSize.Width -
                    rightLabel.Width -
                    38,
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

        gatewayButton.Click += (_, _) =>
        {
            ShowGatewayPage();
        };

        Button sensorsButton =
            CreateNavigationButton(
                "SENSORS",
                130);

        sensorsButton.Click += (_, _) =>
        {
            OpenSensorWorkspace();
        };

        Button telemetryButton =
            CreateNavigationButton(
                "TELEMETRY",
                215);

        telemetryButton.Click += (_, _) =>
        {
            ShowFeaturePage(
                "TELEMETRY",
                "REAL-TIME READINGS AND ANOMALY MONITORING",
                "Submit strongly typed sensor readings, inspect historical data and identify anomalies.");
        };

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
                Cursor = Cursors.Hand,
                Anchor =
                    AnchorStyles.Top,
                Location =
                    new Point(620, 19),
                UseVisualStyleBackColor =
                    false
            };

        brandButton.FlatAppearance.BorderSize = 0;

        brandButton.Click += (_, _) =>
        {
            ShowGatewayPage();
        };

        Button attachmentsButton =
            CreateNavigationButton(
                "ATTACHMENTS",
                0);

        attachmentsButton.Anchor =
            AnchorStyles.Top |
            AnchorStyles.Right;

        attachmentsButton.Click += (_, _) =>
        {
            ShowFeaturePage(
                "ATTACHMENTS",
                "SENSOR FILES AND DEPLOYMENT EVIDENCE",
                "Upload and retrieve configuration files, deployment photographs and hardware logs.");
        };

        _apiStatusLabel =
            new Label
            {
                Text = "API  ●",
                ForeColor =
                    AppPalette.DarkGray,
                Font =
                    AppFonts.Uppercase,
                AutoSize = true,
                Anchor =
                    AnchorStyles.Top |
                    AnchorStyles.Right
            };

        panel.Controls.Add(gatewayButton);
        panel.Controls.Add(sensorsButton);
        panel.Controls.Add(telemetryButton);
        panel.Controls.Add(brandButton);
        panel.Controls.Add(attachmentsButton);
        panel.Controls.Add(_apiStatusLabel);

        panel.Resize += (_, _) =>
        {
            brandButton.Left =
                (panel.ClientSize.Width -
                 brandButton.Width) / 2;

            attachmentsButton.Location =
                new Point(
                    panel.ClientSize.Width -
                    attachmentsButton.Width -
                    115,
                    24);

            _apiStatusLabel.Location =
                new Point(
                    panel.ClientSize.Width - 75,
                    36);
        };

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
                Cursor = Cursors.Hand,
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
                195));

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
                    "A precise operational layer for connected devices,\n" +
                    "sensor telemetry and distributed environments.",
                ForeColor =
                    AppPalette.DarkGray,
                Font =
                    AppFonts.Body,
                AutoSize = true,
                Location =
                    new Point(5, 245)
            };

        var enterButton =
            new Button
            {
                Text =
                    "ENTER SENSOR GATEWAY  →",
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
                    new Point(5, 310),
                Cursor =
                    Cursors.Hand,
                UseVisualStyleBackColor =
                    false
            };

        enterButton.FlatAppearance.BorderSize = 0;

        enterButton.FlatAppearance.MouseOverBackColor =
            AppPalette.DarkGray;

        enterButton.Click += (_, _) =>
        {
            OpenSensorWorkspace();
        };

        copyPanel.Controls.Add(
            eyebrowLabel);

        copyPanel.Controls.Add(
            titleLabel);

        copyPanel.Controls.Add(
            descriptionLabel);

        copyPanel.Controls.Add(
            enterButton);

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

        hero.Controls.Add(layout);

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
                        10,
                        45,
                        10),
                Margin = Padding.Empty
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

        ingestionCard.OpenRequested += (_, _) =>
        {
            OpenSensorWorkspace();
        };

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
        using var sensorWorkspace =
            new SensorWorkspaceForm();

        sensorWorkspace.ShowDialog(this);
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
                Text = title,
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
                Location =
                    new Point(55, 130)
            };

        var backButton =
            new Button
            {
                Text =
                    "←  BACK TO GATEWAY",
                BackColor =
                    AppPalette.Black,
                ForeColor =
                    AppPalette.White,
                FlatStyle =
                    FlatStyle.Flat,
                Font =
                    AppFonts.Button,
                Size =
                    new Size(180, 42),
                Location =
                    new Point(55, 200),
                Cursor =
                    Cursors.Hand,
                UseVisualStyleBackColor =
                    false
            };

        backButton.FlatAppearance.BorderSize = 0;

        backButton.Click += (_, _) =>
        {
            ShowGatewayPage();
        };

        panel.Controls.Add(
            subtitleLabel);

        panel.Controls.Add(
            titleLabel);

        panel.Controls.Add(
            descriptionLabel);

        panel.Controls.Add(
            backButton);

        _pageHost.Controls.Add(panel);
    }

    private static Panel BuildFooter()
    {
        var footer =
            new Panel
            {
                Dock = DockStyle.Fill,
                BackColor =
                    AppPalette.Background
            };

        string[] items =
        [
            "01  STRONGLY TYPED DATA",
            "02  ANOMALY DETECTION",
            "03  SQL PERSISTENCE",
            "04  SECURE ATTACHMENTS"
        ];

        for (int index = 0;
             index < items.Length;
             index++)
        {
            var label =
                new Label
                {
                    Text = items[index],
                    ForeColor =
                        AppPalette.Black,
                    Font =
                        AppFonts.Uppercase,
                    AutoSize = true,
                    Location =
                        new Point(
                            55 + index * 320,
                            25)
                };

            footer.Controls.Add(label);
        }

        return footer;
    }
}