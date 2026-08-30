using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Helix.Services;
using Helix.ViewModels;
using Helix.Views;

namespace Helix;

public partial class App : Application
{
    private HelixRuntime? _runtime;

    public static MainViewModel ViewModel { get; } = new();
    public static bool ExitRequested { get; set; }

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            _runtime = new HelixRuntime(ViewModel);
            _runtime.Start();
            desktop.MainWindow = new MainWindow { DataContext = ViewModel };
            desktop.Exit += (_, _) =>
            {
                _runtime?.Dispose();
                SettingsStore.Save(ViewModel.SnapshotSettings());
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnTrayClicked(object? sender, EventArgs e) => ShowMain();
    private void OnOpenClicked(object? sender, EventArgs e) => ShowMain();
    private void OnQuitClicked(object? sender, EventArgs e) => Quit();

    public static void ShowMain()
    {
        if (Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        desktop.MainWindow ??= new MainWindow { DataContext = ViewModel };
        desktop.MainWindow.Show();
        desktop.MainWindow.WindowState = WindowState.Normal;
        desktop.MainWindow.Activate();
    }

    public static void Quit()
    {
        ExitRequested = true;
        if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown(0);
        }
    }
}
