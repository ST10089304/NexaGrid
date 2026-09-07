using System.Globalization;
using System.Drawing.Drawing2D;

using NexaGrid.Shared.DTOs;

namespace NexaGrid.Desktop.Controls;

public sealed class TelemetryChartControl : Control
{
    private static readonly Color BackgroundColour =
        Color.FromArgb(239, 237, 232);

    private static readonly Color PlotColour =
        Color.FromArgb(248, 247, 243);

    private static readonly Color PrimaryColour =
        Color.FromArgb(10, 10, 10);

    private static readonly Color SecondaryColour =
        Color.FromArgb(100, 98, 92);

    private static readonly Color GridColour =
        Color.FromArgb(211, 208, 200);

    private static readonly Color AnomalyColour =
        Color.FromArgb(174, 54, 42);

    private readonly List<ChartPoint> _points = [];

    public TelemetryChartControl()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;

        BackColor = BackgroundColour;
        ForeColor = PrimaryColour;

        MinimumSize =
            new Size(400, 240);
    }

    public int ReadingCount =>
        _points.Count;

    public int AnomalyCount =>
        _points.Count(point => point.IsAnomaly);

    public void SetReadings(
        IEnumerable<TelemetryResponse>? readings)
    {
        _points.Clear();

        if (readings is not null)
        {
            foreach (TelemetryResponse reading
                     in readings.OrderBy(
                         item => item.RecordedAtUtc))
            {
                if (TryConvertValue(
                        reading.Value,
                        out double value))
                {
                    _points.Add(
                        new ChartPoint(
                            value,
                            reading.RecordedAtUtc,
                            reading.IsAnomaly));
                }
            }
        }

        Invalidate();
    }

    public void ClearReadings()
    {
        _points.Clear();
        Invalidate();
    }

    protected override void OnPaint(
        PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics graphics = e.Graphics;

        graphics.SmoothingMode =
            SmoothingMode.AntiAlias;

        graphics.TextRenderingHint =
            System.Drawing.Text.TextRenderingHint
                .ClearTypeGridFit;

        graphics.Clear(BackgroundColour);

        Rectangle chartBounds =
            new(
                62,
                34,
                Math.Max(1, ClientSize.Width - 88),
                Math.Max(1, ClientSize.Height - 82));

        using var plotBrush =
            new SolidBrush(PlotColour);

        using var borderPen =
            new Pen(PrimaryColour, 1);

        graphics.FillRectangle(
            plotBrush,
            chartBounds);

        graphics.DrawRectangle(
            borderPen,
            chartBounds);

        DrawHeading(
            graphics);

        if (_points.Count == 0)
        {
            DrawEmptyState(
                graphics,
                chartBounds);

            return;
        }

        DrawGrid(
            graphics,
            chartBounds);

        DrawTelemetryLine(
            graphics,
            chartBounds);

        DrawAxisLabels(
            graphics,
            chartBounds);

        DrawLegend(
            graphics,
            chartBounds);
    }

    private void DrawHeading(
        Graphics graphics)
    {
        using var headingFont =
            new Font(
                "Bahnschrift",
                10,
                FontStyle.Bold);

        using var headingBrush =
            new SolidBrush(PrimaryColour);

        graphics.DrawString(
            "READING HISTORY",
            headingFont,
            headingBrush,
            12,
            8);
    }

    private static void DrawEmptyState(
        Graphics graphics,
        Rectangle chartBounds)
    {
        using var font =
            new Font(
                "Segoe UI",
                10,
                FontStyle.Regular);

        using var brush =
            new SolidBrush(SecondaryColour);

        const string message =
            "NO TELEMETRY READINGS AVAILABLE";

        SizeF textSize =
            graphics.MeasureString(
                message,
                font);

        float x =
            chartBounds.Left
            + (chartBounds.Width - textSize.Width) / 2;

        float y =
            chartBounds.Top
            + (chartBounds.Height - textSize.Height) / 2;

        graphics.DrawString(
            message,
            font,
            brush,
            x,
            y);
    }

    private static void DrawGrid(
        Graphics graphics,
        Rectangle chartBounds)
    {
        using var gridPen =
            new Pen(GridColour, 1)
            {
                DashStyle = DashStyle.Dot
            };

        const int horizontalSections = 4;
        const int verticalSections = 6;

        for (int index = 1;
             index < horizontalSections;
             index++)
        {
            float y =
                chartBounds.Top
                + chartBounds.Height
                * index
                / (float)horizontalSections;

            graphics.DrawLine(
                gridPen,
                chartBounds.Left,
                y,
                chartBounds.Right,
                y);
        }

        for (int index = 1;
             index < verticalSections;
             index++)
        {
            float x =
                chartBounds.Left
                + chartBounds.Width
                * index
                / (float)verticalSections;

            graphics.DrawLine(
                gridPen,
                x,
                chartBounds.Top,
                x,
                chartBounds.Bottom);
        }
    }

    private void DrawTelemetryLine(
        Graphics graphics,
        Rectangle chartBounds)
    {
        double minimum =
            _points.Min(point => point.Value);

        double maximum =
            _points.Max(point => point.Value);

        if (Math.Abs(maximum - minimum) < 0.001)
        {
            minimum -= 1;
            maximum += 1;
        }

        double padding =
            (maximum - minimum) * 0.1;

        minimum -= padding;
        maximum += padding;

        var plottedPoints =
            new PointF[_points.Count];

        for (int index = 0;
             index < _points.Count;
             index++)
        {
            float x = _points.Count == 1
                ? chartBounds.Left
                  + chartBounds.Width / 2f
                : chartBounds.Left
                  + chartBounds.Width
                  * index
                  / (float)(_points.Count - 1);

            double normalised =
                (_points[index].Value - minimum)
                / (maximum - minimum);

            float y =
                chartBounds.Bottom
                - (float)(
                    normalised
                    * chartBounds.Height);

            plottedPoints[index] =
                new PointF(x, y);
        }

        using var linePen =
            new Pen(PrimaryColour, 2.2f)
            {
                LineJoin = LineJoin.Round
            };

        if (plottedPoints.Length > 1)
        {
            graphics.DrawLines(
                linePen,
                plottedPoints);
        }

        for (int index = 0;
             index < plottedPoints.Length;
             index++)
        {
            ChartPoint reading =
                _points[index];

            float markerSize =
                reading.IsAnomaly
                    ? 10f
                    : 6f;

            Color markerColour =
                reading.IsAnomaly
                    ? AnomalyColour
                    : PrimaryColour;

            using var markerBrush =
                new SolidBrush(markerColour);

            RectangleF marker =
                new(
                    plottedPoints[index].X
                    - markerSize / 2,
                    plottedPoints[index].Y
                    - markerSize / 2,
                    markerSize,
                    markerSize);

            graphics.FillEllipse(
                markerBrush,
                marker);
        }
    }

    private void DrawAxisLabels(
        Graphics graphics,
        Rectangle chartBounds)
    {
        double minimum =
            _points.Min(point => point.Value);

        double maximum =
            _points.Max(point => point.Value);

        using var font =
            new Font(
                "Segoe UI",
                8,
                FontStyle.Regular);

        using var brush =
            new SolidBrush(SecondaryColour);

        graphics.DrawString(
            maximum.ToString(
                "0.##",
                CultureInfo.InvariantCulture),
            font,
            brush,
            8,
            chartBounds.Top - 5);

        graphics.DrawString(
            minimum.ToString(
                "0.##",
                CultureInfo.InvariantCulture),
            font,
            brush,
            8,
            chartBounds.Bottom - 12);

        DateTime first =
            _points[0].RecordedAtUtc;

        DateTime last =
            _points[^1].RecordedAtUtc;

        string firstLabel =
            first.ToLocalTime().ToString(
                "HH:mm:ss");

        string lastLabel =
            last.ToLocalTime().ToString(
                "HH:mm:ss");

        graphics.DrawString(
            firstLabel,
            font,
            brush,
            chartBounds.Left,
            chartBounds.Bottom + 8);

        SizeF lastSize =
            graphics.MeasureString(
                lastLabel,
                font);

        graphics.DrawString(
            lastLabel,
            font,
            brush,
            chartBounds.Right - lastSize.Width,
            chartBounds.Bottom + 8);
    }

    private void DrawLegend(
        Graphics graphics,
        Rectangle chartBounds)
    {
        using var font =
            new Font(
                "Segoe UI",
                8,
                FontStyle.Bold);

        using var standardBrush =
            new SolidBrush(PrimaryColour);

        using var anomalyBrush =
            new SolidBrush(AnomalyColour);

        float y =
            chartBounds.Bottom + 30;

        graphics.FillEllipse(
            standardBrush,
            chartBounds.Left,
            y + 3,
            7,
            7);

        graphics.DrawString(
            $"{ReadingCount} READINGS",
            font,
            standardBrush,
            chartBounds.Left + 13,
            y);

        graphics.FillEllipse(
            anomalyBrush,
            chartBounds.Left + 120,
            y + 3,
            7,
            7);

        graphics.DrawString(
            $"{AnomalyCount} ANOMALIES",
            font,
            anomalyBrush,
            chartBounds.Left + 133,
            y);
    }

    private static bool TryConvertValue(
        string value,
        out double convertedValue)
    {
        if (double.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out convertedValue))
        {
            return true;
        }

        if (bool.TryParse(
                value,
                out bool booleanValue))
        {
            convertedValue =
                booleanValue ? 1d : 0d;

            return true;
        }

        convertedValue = 0;

        return false;
    }

    private sealed record ChartPoint(
        double Value,
        DateTime RecordedAtUtc,
        bool IsAnomaly);
}