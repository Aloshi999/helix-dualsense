namespace Helix.Services;

public static class AppLog
{
    private static readonly object Gate = new();
    private static string? _path;

    public static string Path
    {
        get
        {
            EnsurePath();
            return _path!;
        }
    }

    public static void Info(string message) => Write("INF", message);
    public static void Warn(string message) => Write("WRN", message);

    public static void Error(string message, Exception? ex = null) =>
        Write("ERR", ex is null ? message : $"{message} {ex}");

    private static void Write(string level, string message)
    {
        try
        {
            EnsurePath();
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {level} {message}{Environment.NewLine}";
            lock (Gate)
            {
                File.AppendAllText(_path!, line);
            }
        }
        catch
        {
            // Logging must never take the app down.
        }
    }

    private static void EnsurePath()
    {
        if (_path is not null)
        {
            return;
        }

        lock (Gate)
        {
            if (_path is not null)
            {
                return;
            }

            var nextToExe = System.IO.Path.Combine(AppContext.BaseDirectory, "Helix.log");
            try
            {
                File.AppendAllText(nextToExe, string.Empty);
                _path = nextToExe;
            }
            catch
            {
                Directory.CreateDirectory(SettingsStore.DirectoryPath);
                _path = System.IO.Path.Combine(SettingsStore.DirectoryPath, "Helix.log");
            }
        }
    }
}
