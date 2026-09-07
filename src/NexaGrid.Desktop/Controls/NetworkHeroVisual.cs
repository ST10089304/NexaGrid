using System.Drawing.Drawing2D;
using NexaGrid.Desktop.Theme;

namespace NexaGrid.Desktop.Controls;
/*Tutlane (2019) C# User Control (UserControl) in Windows Forms.*/
public class NetworkHeroVisual : Control
{
    public NetworkHeroVisual()
    {
        DoubleBuffered = true;
        BackColor = AppPalette.Background;
        MinimumSize = new Size(340, 260);
    }
/*Tutlane (2019) C# User Control (UserControl) in Windows Forms.*/
    protected override void OnPaint(
        PaintEventArgs eventArgs)
    {
        base.OnPaint(eventArgs);

        Graphics graphics = eventArgs.Graphics;

        graphics.SmoothingMode =
            SmoothingMode.AntiAlias;

        int centreX = Width / 2;
        int centreY = Height / 2;

        Point[] nodes =
        [
            new Point(centreX, centreY - 90),
            new Point(centreX - 105, centreY - 20),
            new Point(centreX + 105, centreY - 20),
            new Point(centreX - 70, centreY + 85),
            new Point(centreX + 75, centreY + 82)
        ];

        using var linePen =
            new Pen(AppPalette.Black, 1.2f);

        linePen.DashStyle = DashStyle.Dot;

        DrawConnection(
            graphics,
            linePen,
            nodes[0],
            nodes[1]);

        DrawConnection(
            graphics,
            linePen,
            nodes[0],
            nodes[2]);

        DrawConnection(
            graphics,
            linePen,
            nodes[1],
            nodes[3]);

        DrawConnection(
            graphics,
            linePen,
            nodes[2],
            nodes[4]);

        DrawConnection(
            graphics,
            linePen,
            nodes[3],
            nodes[4]);

        using var outerPen =
            new Pen(AppPalette.LightGray, 1);

        graphics.DrawEllipse(
            outerPen,
            centreX - 145,
            centreY - 145,
            290,
            290);

        for (int index = 0;
             index < nodes.Length;
             index++)
        {
            int size = index == 0 ? 22 : 14;

            Rectangle node =
                new(
                    nodes[index].X - size / 2,
                    nodes[index].Y - size / 2,
                    size,
                    size);

            using var brush =
                new SolidBrush(
                    index == 0
                        ? AppPalette.Black
                        : AppPalette.Paper);

            graphics.FillEllipse(brush, node);
            graphics.DrawEllipse(
                Pens.Black,
                node);
        }

        using var labelFont =
            new Font(
                "Segoe UI",
                7.5f,
                FontStyle.Bold);

        using var labelBrush =
            new SolidBrush(AppPalette.DarkGray);

        graphics.DrawString(
            "GATEWAY",
            labelFont,
            labelBrush,
            centreX - 30,
            centreY - 123);

        graphics.DrawString(
            "SENSOR NETWORK / 001",
            labelFont,
            labelBrush,
            18,
            Height - 28);
    }
/*Tutlane (2019) C# User Control (UserControl) in Windows Forms.*/
    private static void DrawConnection(
        Graphics graphics,
        Pen pen,
        Point start,
        Point end)
    {
        graphics.DrawLine(
            pen,
            start,
            end);
    }
}