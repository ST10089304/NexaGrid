using FontAwesome.Sharp;
using NexaGrid.Desktop.Theme;

namespace NexaGrid.Desktop.Controls;

public class PillarCard : RoundedPanel
{
    private readonly bool _isAvailable;

    public event EventHandler? OpenRequested;

    public PillarCard(
        IconChar icon,
        string number,
        string title,
        string description,
        bool isAvailable)
    {
        _isAvailable = isAvailable;

        Dock = DockStyle.Fill;
        MinimumSize = new Size(270, 205);
        BackColor = AppPalette.Black;
        BorderColor = AppPalette.DarkGray;
        CornerRadius = 1;
        BorderSize = 1;
        Padding = new Padding(20);

        BuildLayout(
            icon,
            number,
            title,
            description);
    }

    private void BuildLayout(
        IconChar icon,
        string number,
        string title,
        string description)
    {
        var layout =
            new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 2,
                RowCount = 4,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

        layout.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Absolute,
                42));

        layout.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Percent,
                100));

        layout.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                35));

        layout.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                38));

        layout.RowStyles.Add(
            new RowStyle(
                SizeType.Percent,
                100));

        layout.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                46));

        var iconControl =
            new IconPictureBox
            {
                IconChar = icon,
                IconColor = _isAvailable
                    ? AppPalette.White
                    : AppPalette.MidGray,
                IconSize = 21,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 2, 8, 2),
                TabStop = false
            };

        var numberLabel =
            new Label
            {
                Text = number,
                ForeColor = AppPalette.MidGray,
                Font = AppFonts.Uppercase,
                Dock = DockStyle.Fill,
                TextAlign =
                    ContentAlignment.MiddleLeft,
                Margin = Padding.Empty
            };

        var titleLabel =
            new Label
            {
                Text = title.ToUpperInvariant(),
                ForeColor = _isAvailable
                    ? AppPalette.White
                    : AppPalette.LightGray,
                Font = AppFonts.Heading,
                Dock = DockStyle.Fill,
                TextAlign =
                    ContentAlignment.MiddleLeft,
                AutoEllipsis = true,
                Margin = Padding.Empty
            };

        var descriptionLabel =
            new Label
            {
                Text = description,
                ForeColor = AppPalette.LightGray,
                Font = new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Regular),
                Dock = DockStyle.Fill,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.TopLeft,
                Margin = new Padding(0, 6, 8, 8),
                Padding = new Padding(0, 2, 0, 0)
            };

        var actionPanel =
            new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = Padding.Empty
            };

        var actionButton =
            new Button
            {
                Text = _isAvailable
                    ? "ENTER"
                    : "LOCKED",
                Font = AppFonts.Button,
                BackColor = _isAvailable
                    ? AppPalette.White
                    : AppPalette.DarkGray,
                ForeColor = _isAvailable
                    ? AppPalette.Black
                    : AppPalette.MidGray,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(110, 38),
                Dock = DockStyle.Right,
                Cursor = _isAvailable
                    ? Cursors.Hand
                    : Cursors.Default,
                Enabled = _isAvailable,
                UseVisualStyleBackColor = false,
                TabStop = _isAvailable,
                Margin = Padding.Empty
            };

        actionButton.FlatAppearance.BorderSize = 0;

        actionButton.FlatAppearance.MouseOverBackColor =
            AppPalette.LightGray;

        actionButton.FlatAppearance.MouseDownBackColor =
            AppPalette.MidGray;

        actionButton.Click += (_, _) =>
        {
            if (_isAvailable)
            {
                OpenRequested?.Invoke(
                    this,
                    EventArgs.Empty);
            }
        };

        if (_isAvailable)
        {
            Cursor = Cursors.Hand;

            Click += (_, _) =>
            {
                OpenRequested?.Invoke(
                    this,
                    EventArgs.Empty);
            };

            titleLabel.Click += (_, _) =>
            {
                OpenRequested?.Invoke(
                    this,
                    EventArgs.Empty);
            };

            descriptionLabel.Click += (_, _) =>
            {
                OpenRequested?.Invoke(
                    this,
                    EventArgs.Empty);
            };
        }

        actionPanel.Controls.Add(actionButton);

        layout.Controls.Add(iconControl, 0, 0);
        layout.Controls.Add(numberLabel, 1, 0);

        layout.Controls.Add(titleLabel, 0, 1);
        layout.SetColumnSpan(titleLabel, 2);

        layout.Controls.Add(descriptionLabel, 0, 2);
        layout.SetColumnSpan(descriptionLabel, 2);

        layout.Controls.Add(actionPanel, 0, 3);
        layout.SetColumnSpan(actionPanel, 2);

        Controls.Add(layout);
    }
}


