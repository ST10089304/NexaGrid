namespace NexaGrid.Desktop.Theme;

public static class AppFonts
{
    public static Font Brand =>
        new("Bahnschrift", 25, FontStyle.Bold);

    public static Font Display =>
        new("Bahnschrift", 54, FontStyle.Bold);

    public static Font PageTitle =>
        new("Bahnschrift", 28, FontStyle.Bold);

    public static Font Heading =>
        new("Bahnschrift", 15, FontStyle.Bold);

    public static Font Logo => Brand;

    public static Font Body =>
        new("Segoe UI", 10, FontStyle.Regular);

    public static Font BodyMedium =>
        new("Segoe UI", 10, FontStyle.Bold);

    public static Font Caption =>
        new("Segoe UI", 8.5f, FontStyle.Regular);

    public static Font Uppercase =>
        new("Segoe UI", 8, FontStyle.Bold);

    public static Font Button =>
        new("Segoe UI", 9, FontStyle.Bold);
}