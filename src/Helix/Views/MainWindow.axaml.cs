using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Helix.ViewModels;

namespace Helix.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Closing += OnClosing;
        PointerPressed += (_, e) =>
        {
            if (DataContext is MainViewModel { MenuOpen: true } vm
                && (e.Source is not Control || !Overflow.IsVisible || !Overflow.IsPointerOver))
            {
                vm.MenuOpen = false;
            }
        };
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (!App.ExitRequested)
        {
            e.Cancel = true;
            Hide();
        }
    }

    private void OnQuit(object? sender, RoutedEventArgs e) => App.Quit();
}
