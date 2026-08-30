using Microsoft.Win32;

namespace Helix.Services;

public static class StartupRegistration
{
    private const string ValueName = "Helix";
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public static void Apply(bool startWithWindows)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
            if (key is null)
            {
                return;
            }

            if (startWithWindows)
            {
                var processPath = Environment.ProcessPath;
                if (!string.IsNullOrWhiteSpace(processPath))
                {
                    key.SetValue(ValueName, "\"" + processPath + "\"");
                }
            }
            else if (key.GetValue(ValueName) is not null)
            {
                key.DeleteValue(ValueName, throwOnMissingValue: false);
            }
        }
        catch (Exception ex)
        {
            AppLog.Warn("Start with Windows: " + ex.Message);
        }
    }
}
