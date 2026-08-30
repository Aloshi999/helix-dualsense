using System.Diagnostics;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Helix.Models;
using Helix.Services;

namespace Helix.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly object _settingsGate = new();
    private CancellationTokenSource? _saveCts;

    [ObservableProperty] private PadSnapshot _pad = PadSnapshot.Empty;
    [ObservableProperty] private EmulationMode _mode;
    [ObservableProperty] private TriggerPreset _leftPreset;
    [ObservableProperty] private TriggerPreset _rightPreset;
    [ObservableProperty] private int _leftIntensity;
    [ObservableProperty] private int _rightIntensity;
    [ObservableProperty] private int _leftCustomStart;
    [ObservableProperty] private int _leftCustomEnd;
    [ObservableProperty] private int _leftCustomForce;
    [ObservableProperty] private int _rightCustomStart;
    [ObservableProperty] private int _rightCustomEnd;
    [ObservableProperty] private int _rightCustomForce;
    [ObservableProperty] private Color _lightbar = Color.FromRgb(46, 107, 158);
    [ObservableProperty] private double _lightbarBrightness;
    [ObservableProperty] private int _hapticsStrength;
    [ObservableProperty] private bool _audioToHaptics;
    [ObservableProperty] private XboxFace _leftPaddle;
    [ObservableProperty] private XboxFace _rightPaddle;
    [ObservableProperty] private TouchpadMode _touchpad;
    [ObservableProperty] private GyroMode _gyro;
    [ObservableProperty] private double _gyroSensitivity;
    [ObservableProperty] private double _stickDeadzone;
    [ObservableProperty] private bool _startWithWindows;
    [ObservableProperty] private bool _showVigemBanner;
    [ObservableProperty] private bool _menuOpen;

    public AppSettings Settings { get; }
    public IReadOnlyList<TriggerPreset> TriggerPresets { get; } = Enum.GetValues<TriggerPreset>();
    public IReadOnlyList<XboxFace> PaddleTargets { get; } = Enum.GetValues<XboxFace>();

    public bool IsDualSenseMode => Mode == EmulationMode.DualSense;
    public bool IsXboxMode => Mode == EmulationMode.Xbox;
    public bool LeftCustomVisible => LeftPreset == TriggerPreset.Custom;
    public bool RightCustomVisible => RightPreset == TriggerPreset.Custom;
    public bool GyroSensitivityVisible => Gyro != GyroMode.Off;
    public bool IsConnected => Pad.Connected;
    public string ConnectionLabel => Pad.ConnectionLabel;
    public IBrush ConnectionDot => new SolidColorBrush(IsConnected ? Color.FromRgb(61, 154, 209) : Color.FromRgb(74, 85, 96));
    public bool IsTouchOff => Touchpad == TouchpadMode.Off;
    public bool IsTouchClick => Touchpad == TouchpadMode.ClickOnly;
    public bool IsTouchMouse => Touchpad == TouchpadMode.Mouse;
    public bool IsGyroOff => Gyro == GyroMode.Off;
    public bool IsGyroMouse => Gyro == GyroMode.Mouse;
    public bool IsGyroStick => Gyro == GyroMode.RightStick;
    public string DisconnectHint => "Plug in DualSense over USB-C";
    public string VigemUrl => "https://github.com/nefarius/ViGEmBus/releases";
    public string LightbarHex => $"#{Lightbar.R:X2}{Lightbar.G:X2}{Lightbar.B:X2}";

    public event Action? SettingsChanged;
    public event Action<EmulationMode>? ModeChanged;

    public MainViewModel()
    {
        Settings = SettingsStore.Load();
        _mode = Settings.Mode;
        _leftPreset = Settings.LeftTrigger.Preset;
        _rightPreset = Settings.RightTrigger.Preset;
        _leftIntensity = Settings.LeftTrigger.Intensity;
        _rightIntensity = Settings.RightTrigger.Intensity;
        _leftCustomStart = Settings.LeftTrigger.CustomStart;
        _leftCustomEnd = Settings.LeftTrigger.CustomEnd;
        _leftCustomForce = Settings.LeftTrigger.CustomForce;
        _rightCustomStart = Settings.RightTrigger.CustomStart;
        _rightCustomEnd = Settings.RightTrigger.CustomEnd;
        _rightCustomForce = Settings.RightTrigger.CustomForce;
        _lightbar = Settings.LightbarColor();
        _lightbarBrightness = Settings.LightbarBrightness;
        _hapticsStrength = Settings.HapticsStrength;
        _audioToHaptics = Settings.AudioToHaptics;
        _leftPaddle = Settings.LeftPaddle;
        _rightPaddle = Settings.RightPaddle;
        _touchpad = Settings.Touchpad;
        _gyro = Settings.Gyro;
        _gyroSensitivity = Settings.GyroSensitivity;
        _stickDeadzone = Settings.StickDeadzone;
        _startWithWindows = Settings.StartWithWindows;
    }

    public void ApplyPad(PadSnapshot pad)
    {
        Pad = pad;
        OnPropertyChanged(nameof(IsConnected));
        OnPropertyChanged(nameof(ConnectionLabel));
        OnPropertyChanged(nameof(ConnectionDot));
    }

    public void SetVigem(bool available, string? error)
    {
        ShowVigemBanner = Mode == EmulationMode.Xbox && !available;
        if (error is not null && ShowVigemBanner)
        {
            AppLog.Warn("Xbox mode needs ViGEmBus: " + error);
        }
    }

    public AppSettings SnapshotSettings()
    {
        lock (_settingsGate)
        {
            return new AppSettings
            {
                Mode = Settings.Mode,
                LeftTrigger = new TriggerSideSettings
                {
                    Preset = Settings.LeftTrigger.Preset,
                    Intensity = Settings.LeftTrigger.Intensity,
                    CustomStart = Settings.LeftTrigger.CustomStart,
                    CustomEnd = Settings.LeftTrigger.CustomEnd,
                    CustomForce = Settings.LeftTrigger.CustomForce
                },
                RightTrigger = new TriggerSideSettings
                {
                    Preset = Settings.RightTrigger.Preset,
                    Intensity = Settings.RightTrigger.Intensity,
                    CustomStart = Settings.RightTrigger.CustomStart,
                    CustomEnd = Settings.RightTrigger.CustomEnd,
                    CustomForce = Settings.RightTrigger.CustomForce
                },
                LightbarHex = Settings.LightbarHex,
                LightbarBrightness = Settings.LightbarBrightness,
                HapticsStrength = Settings.HapticsStrength,
                AudioToHaptics = Settings.AudioToHaptics,
                LeftPaddle = Settings.LeftPaddle,
                RightPaddle = Settings.RightPaddle,
                Touchpad = Settings.Touchpad,
                Gyro = Settings.Gyro,
                GyroSensitivity = Settings.GyroSensitivity,
                StickDeadzone = Settings.StickDeadzone,
                StartWithWindows = Settings.StartWithWindows
            };
        }
    }

    [RelayCommand]
    private void SetMode(string mode) =>
        Mode = mode.Equals("Xbox", StringComparison.OrdinalIgnoreCase) ? EmulationMode.Xbox : EmulationMode.DualSense;

    [RelayCommand]
    private void SetTouchpad(string mode) =>
        Touchpad = mode switch
        {
            "Mouse" => TouchpadMode.Mouse,
            "Off" => TouchpadMode.Off,
            _ => TouchpadMode.ClickOnly
        };

    [RelayCommand]
    private void SetGyro(string mode) =>
        Gyro = mode switch
        {
            "Mouse" => GyroMode.Mouse,
            "RightStick" => GyroMode.RightStick,
            _ => GyroMode.Off
        };

    [RelayCommand]
    private void ToggleMenu() => MenuOpen = !MenuOpen;

    [RelayCommand]
    private void OpenLog()
    {
        try
        {
            var path = AppLog.Path;
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + path + "\"") { UseShellExecute = true });
            }
            else
            {
                Process.Start(new ProcessStartInfo("xdg-open", Path.GetDirectoryName(path) ?? ".") { UseShellExecute = true });
            }
        }
        catch (Exception ex)
        {
            AppLog.Warn("Open log: " + ex.Message);
        }
    }

    [RelayCommand]
    private void OpenVigem()
    {
        try
        {
            Process.Start(new ProcessStartInfo(VigemUrl) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            AppLog.Warn("Open ViGEm URL: " + ex.Message);
        }
    }

    private void Persist(Action<AppSettings> mutate)
    {
        lock (_settingsGate)
        {
            mutate(Settings);
        }

        SettingsChanged?.Invoke();
        _saveCts?.Cancel();
        _ = SaveSoon((_saveCts = new CancellationTokenSource()).Token);
    }

    private async Task SaveSoon(CancellationToken token)
    {
        try
        {
            await Task.Delay(250, token);
            SettingsStore.Save(SnapshotSettings());
        }
        catch (TaskCanceledException)
        {
        }
    }

    partial void OnModeChanged(EmulationMode value)
    {
        Persist(s => s.Mode = value);
        OnPropertyChanged(nameof(IsDualSenseMode));
        OnPropertyChanged(nameof(IsXboxMode));
        ModeChanged?.Invoke(value);
    }

    partial void OnLeftPresetChanged(TriggerPreset value)
    {
        Persist(s => s.LeftTrigger.Preset = value);
        OnPropertyChanged(nameof(LeftCustomVisible));
    }

    partial void OnRightPresetChanged(TriggerPreset value)
    {
        Persist(s => s.RightTrigger.Preset = value);
        OnPropertyChanged(nameof(RightCustomVisible));
    }

    partial void OnLeftIntensityChanged(int value) => Persist(s => s.LeftTrigger.Intensity = value);
    partial void OnRightIntensityChanged(int value) => Persist(s => s.RightTrigger.Intensity = value);
    partial void OnLeftCustomStartChanged(int value) => Persist(s => s.LeftTrigger.CustomStart = value);
    partial void OnLeftCustomEndChanged(int value) => Persist(s => s.LeftTrigger.CustomEnd = value);
    partial void OnLeftCustomForceChanged(int value) => Persist(s => s.LeftTrigger.CustomForce = value);
    partial void OnRightCustomStartChanged(int value) => Persist(s => s.RightTrigger.CustomStart = value);
    partial void OnRightCustomEndChanged(int value) => Persist(s => s.RightTrigger.CustomEnd = value);
    partial void OnRightCustomForceChanged(int value) => Persist(s => s.RightTrigger.CustomForce = value);

    partial void OnLightbarChanged(Color value)
    {
        Persist(s => s.LightbarHex = $"#{value.R:X2}{value.G:X2}{value.B:X2}");
        OnPropertyChanged(nameof(LightbarHex));
    }

    partial void OnLightbarBrightnessChanged(double value) => Persist(s => s.LightbarBrightness = value);
    partial void OnHapticsStrengthChanged(int value) => Persist(s => s.HapticsStrength = value);
    partial void OnAudioToHapticsChanged(bool value) => Persist(s => s.AudioToHaptics = value);
    partial void OnLeftPaddleChanged(XboxFace value) => Persist(s => s.LeftPaddle = value);
    partial void OnRightPaddleChanged(XboxFace value) => Persist(s => s.RightPaddle = value);

    partial void OnTouchpadChanged(TouchpadMode value)
    {
        Persist(s => s.Touchpad = value);
        OnPropertyChanged(nameof(IsTouchOff));
        OnPropertyChanged(nameof(IsTouchClick));
        OnPropertyChanged(nameof(IsTouchMouse));
    }

    partial void OnGyroChanged(GyroMode value)
    {
        Persist(s => s.Gyro = value);
        OnPropertyChanged(nameof(GyroSensitivityVisible));
        OnPropertyChanged(nameof(IsGyroOff));
        OnPropertyChanged(nameof(IsGyroMouse));
        OnPropertyChanged(nameof(IsGyroStick));
    }

    partial void OnGyroSensitivityChanged(double value) => Persist(s => s.GyroSensitivity = value);
    partial void OnStickDeadzoneChanged(double value) => Persist(s => s.StickDeadzone = value);
    partial void OnStartWithWindowsChanged(bool value) => Persist(s => s.StartWithWindows = value);
}
