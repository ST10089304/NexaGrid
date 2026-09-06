using NexaGrid.Desktop.Forms;

namespace NexaGrid.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        try
        {
            using var mainForm =
                new MainForm();

            Application.Run(mainForm);
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                exception.ToString(),
                "NexaGrid startup error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}