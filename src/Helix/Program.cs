using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Threading;
using Helix.Services;

namespace Helix;

internal static class Program
{
    private const string MutexName = "Local\\Helix.Allawi.SingleInstance";
    private const string PulseName = "Local\\Helix.Allawi.Show";

    [STAThread]
    public static void Main(string[] args)
    {
        using var mutex = new Mutex(true, MutexName, out var createdNew);
        if (!createdNew)
        {
            PulseExisting();
            return;
        }

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            AppLog.Error("Unhandled exception.", e.ExceptionObject as Exception);
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            AppLog.Error("Unobserved task exception.", e.Exception);
            e.SetObserved();
        };

        AppLog.Info("Helix starting.");
        try
        {
            if (OperatingSystem.IsWindows())
            {
                _ = Task.Run(WatchPulse);
            }

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            AppLog.Error("Fatal.", ex);
            throw;
        }
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    private static void PulseExisting()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        try
        {
            using var mmf = MemoryMappedFile.OpenExisting(PulseName);
            using var acc = mmf.CreateViewAccessor();
            acc.Write(0L, (byte)1);
        }
        catch
        {
            // The other instance may already be exiting.
        }
    }

    [SupportedOSPlatform("windows")]
    private static async Task WatchPulse()
    {
        try
        {
            using var mmf = MemoryMappedFile.CreateOrOpen(PulseName, 1);
            using var acc = mmf.CreateViewAccessor();
            while (true)
            {
                await Task.Delay(400);
                if (acc.ReadByte(0L) == 1)
                {
                    acc.Write(0L, (byte)0);
                    Dispatcher.UIThread.Post(App.ShowMain);
                }
            }
        }
        catch (Exception ex)
        {
            AppLog.Warn("Single-instance pulse: " + ex.Message);
        }
    }
}
