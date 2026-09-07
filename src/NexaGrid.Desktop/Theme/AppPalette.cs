namespace NexaGrid.Desktop.Theme;
/*Refactoring Guru (2021) Factory and Theme Patterns in GUI Development*/
public static class AppPalette
{
    public static readonly Color Background =
        Color.FromArgb(235, 233, 228);

    public static readonly Color Paper =
        Color.FromArgb(245, 244, 240);

    public static readonly Color Black =
        Color.FromArgb(10, 10, 10);

    public static readonly Color Charcoal =
        Color.FromArgb(28, 28, 27);

    public static readonly Color DarkGray =
        Color.FromArgb(65, 65, 62);

    public static readonly Color MidGray =
        Color.FromArgb(125, 124, 119);

    public static readonly Color LightGray =
        Color.FromArgb(211, 209, 202);

    public static readonly Color White =
        Color.FromArgb(250, 249, 246);

    public static readonly Color Success =
        Color.FromArgb(52, 92, 67);

    public static readonly Color Warning =
        Color.FromArgb(166, 105, 25);

    public static readonly Color Danger =
        Color.FromArgb(146, 48, 42);

    // Compatibility with existing controls.
    /*Refactoring Guru (2021) Factory and Theme Patterns in GUI Development*/
    public static readonly Color Sidebar = Black;
    public static readonly Color Surface = Charcoal;
    public static readonly Color SurfaceLight = DarkGray;
    public static readonly Color Border = LightGray;
    public static readonly Color Primary = Black;
    public static readonly Color PrimaryDark = DarkGray;
    public static readonly Color Secondary = MidGray;
    public static readonly Color Disabled = MidGray;
    public static readonly Color TextPrimary = Black;
    public static readonly Color TextSecondary = DarkGray;
}