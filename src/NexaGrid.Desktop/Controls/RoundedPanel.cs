using System.ComponentModel;
using System.Drawing.Drawing2D;
using NexaGrid.Desktop.Theme;

namespace NexaGrid.Desktop.Controls;

public class RoundedPanel : Panel
{
    private int _cornerRadius = 18;
    private Color _borderColor = AppPalette.Border;
    private int _borderSize = 1;

    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public int CornerRadius
    {
        get => _cornerRadius;
        set
        {
            _cornerRadius = Math.Max(1, value);
            UpdatePanelRegion();
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public Color BorderColor
    {
        get => _borderColor;
        set
        {
            _borderColor = value;
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public int BorderSize
    {
        get => _borderSize;
        set
        {
            _borderSize = Math.Max(0, value);
            Invalidate();
        }
    }

    protected override void OnResize(EventArgs eventArgs)
    {
        base.OnResize(eventArgs);
        UpdatePanelRegion();
    }

    protected override void OnPaint(PaintEventArgs eventArgs)
    {
        base.OnPaint(eventArgs);

        eventArgs.Graphics.SmoothingMode =
            SmoothingMode.AntiAlias;

        Rectangle rectangle =
            ClientRectangle;

        rectangle.Width -= 1;
        rectangle.Height -= 1;

        if (rectangle.Width <= 0 ||
            rectangle.Height <= 0)
        {
            return;
        }

        using GraphicsPath path =
            CreateRoundedPath(
                rectangle,
                CornerRadius);

        if (BorderSize > 0)
        {
            using var borderPen =
                new Pen(
                    BorderColor,
                    BorderSize);

            eventArgs.Graphics.DrawPath(
                borderPen,
                path);
        }
    }

    private void UpdatePanelRegion()
    {
        if (Width <= 0 || Height <= 0)
        {
            return;
        }

        using GraphicsPath path =
            CreateRoundedPath(
                ClientRectangle,
                CornerRadius);

        Region? previousRegion = Region;

        Region = new Region(path);

        previousRegion?.Dispose();
    }

    private static GraphicsPath CreateRoundedPath(
        Rectangle rectangle,
        int radius)
    {
        int diameter = Math.Min(
            radius * 2,
            Math.Min(
                rectangle.Width,
                rectangle.Height));

        var path = new GraphicsPath();

        if (diameter <= 0)
        {
            path.AddRectangle(rectangle);
            return path;
        }

        var arc = new Rectangle(
            rectangle.X,
            rectangle.Y,
            diameter,
            diameter);

        path.AddArc(arc, 180, 90);

        arc.X =
            rectangle.Right - diameter;

        path.AddArc(arc, 270, 90);

        arc.Y =
            rectangle.Bottom - diameter;

        path.AddArc(arc, 0, 90);

        arc.X = rectangle.Left;

        path.AddArc(arc, 90, 90);

        path.CloseFigure();

        return path;
    }
}