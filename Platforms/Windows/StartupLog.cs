using System.Text;

namespace AttendanceUI.WinUI;

/// <summary>
/// Records startup failures for the Windows head, so a crash on the way up is
/// never silent.
/// </summary>
/// <remarks>
/// A WinUI 3 app that throws before the first frame fails fast: the process dies
/// without printing anything, so a launcher such as <c>dotnet run</c> just
/// returns to the prompt and the only trace is a generic "APPCRASH" entry in the
/// Windows event log — exception code 0xc000027b ("stowed exception") naming a
/// WinUI DLL rather than the line of code that failed. Logging the unhandled
/// exceptions (plus the startup steps below) turns that into a usable stack
/// trace.
///
/// The file lives under LocalAppData rather than next to the executable so it
/// still works when the app is installed somewhere a standard user cannot write,
/// and logging itself can never throw: diagnostics must not be able to turn a
/// bad startup into a worse one.
/// </remarks>
internal static class StartupLog
{
    // A crash loop must not grow the file without bound, and the whole point is
    // to have something small enough to paste into a bug report.
    private const long MaxBytes = 256 * 1024;

    private static readonly object Gate = new();

    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AttendanceUI",
        "logs",
        "startup.log");

    /// <summary>
    /// Appends <paramref name="exception"/>, tagged with the <paramref name="stage"/>
    /// of startup it escaped from.
    /// </summary>
    public static void Write(string stage, Exception exception)
    {
        var entry = new StringBuilder()
            .AppendLine($"==== {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [{stage}] ====")
            .AppendLine(exception.ToString())
            .ToString();

        Append(entry);
    }

    private static void Append(string entry)
    {
        try
        {
            lock (Gate)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);

                if (File.Exists(LogPath) && new FileInfo(LogPath).Length > MaxBytes)
                    File.Delete(LogPath);

                File.AppendAllText(LogPath, entry + Environment.NewLine);
            }
        }
        catch
        {
            // Never let diagnostics mask the original failure.
        }
    }
}
