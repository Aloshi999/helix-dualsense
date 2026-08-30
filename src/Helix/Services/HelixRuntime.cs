using System.Runtime.CompilerServices;
using Avalonia.Threading;
using Helix.Models;
using Helix.ViewModels;

namespace Helix.Services;

public sealed class HelixRuntime : IDisposable
{
    private readonly MainViewModel _vm;
    private readonly DualSenseSession _hid = new();
    private readonly InputInjector _inject = new();
    private readonly AudioHaptics _audio = new();
    private readonly CancellationTokenSource _cts = new();
    private IDisposable? _xbox;
    private Action<PadSnapshot, AppSettings>? _xboxUpdate;
    private byte _rumbleLarge;
    private byte _rumbleSmall;
    private DateTime _lastUi = DateTime.MinValue;
    private DateTime _lastGyro = DateTime.UtcNow;
    private PadSnapshot _lastPad = PadSnapshot.Empty;

    public HelixRuntime(MainViewModel vm)
    {
        _vm = vm;
        _vm.SettingsChanged += OnSettingsChanged;
        _vm.ModeChanged += OnModeChanged;
    }

    public void Start()
    {
        OnModeChanged(_vm.Mode);
        _audio.SetEnabled(_vm.Settings.AudioToHaptics);
        _ = Task.Run(LoopAsync);
        AppLog.Info("Helix runtime started.");
    }

    private async Task LoopAsync()
    {
        var token = _cts.Token;
        while (!token.IsCancellationRequested)
        {
            try
            {
                Tick();
            }
            catch (Exception ex)
            {
                AppLog.Error("Runtime tick failed.", ex);
            }

            try
            {
                await Task.Delay(_hid.IsOpen ? 4 : 700, token);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private void Tick()
    {
        if (!_hid.IsOpen && !_hid.TryOpen())
        {
            Publish(PadSnapshot.Empty);
            _xboxUpdate?.Invoke(PadSnapshot.Empty, _vm.Settings);
            return;
        }

        PadSnapshot pad;
        if (_hid.TryRead(out var snapshot))
        {
            pad = snapshot;
            _lastPad = snapshot;
        }
        else
        {
            if (!_hid.IsOpen)
            {
                Publish(PadSnapshot.Empty);
                return;
            }

            pad = _lastPad;
            if (!pad.Connected)
            {
                return;
            }
        }

        var settings = _vm.SnapshotSettings();
        var now = DateTime.UtcNow;
        var dt = Math.Clamp((now - _lastGyro).TotalSeconds, 0.001, 0.05);
        _lastGyro = now;

        if (settings.Touchpad == TouchpadMode.Mouse)
        {
            _inject.ApplyTouchpad(pad.Down(PadButtons.Touchpad), pad.TouchActive, pad.TouchX, pad.TouchY, true);
        }
        else
        {
            _inject.ApplyTouchpad(false, false, 0, 0, false);
        }

        if (settings.Gyro == GyroMode.Mouse)
        {
            _inject.ApplyGyroMouse(pad.GyroY, pad.GyroX, settings.GyroSensitivity, dt);
        }

        _xboxUpdate?.Invoke(pad, settings);

        var scale = Math.Clamp(settings.HapticsStrength / 100.0, 0, 1);
        var large = (byte)(_rumbleLarge * scale);
        var small = (byte)(_rumbleSmall * scale);
        if (settings.AudioToHaptics)
        {
            var audio = _audio.Sample();
            large = Math.Max(large, (byte)(audio.Large * scale));
            small = Math.Max(small, (byte)(audio.Small * scale));
        }

        _hid.Write(settings, large, small);
        Publish(pad);
    }

    private void Publish(PadSnapshot pad)
    {
        var now = DateTime.UtcNow;
        if (now - _lastUi < TimeSpan.FromMilliseconds(16) && pad.Connected == _vm.Pad.Connected)
        {
            return;
        }

        _lastUi = now;
        Dispatcher.UIThread.Post(() => _vm.ApplyPad(pad));
    }

    private void OnSettingsChanged()
    {
        _audio.SetEnabled(_vm.Settings.AudioToHaptics);
        StartupRegistration.Apply(_vm.Settings.StartWithWindows);
        if (_hid.IsOpen)
        {
            var scale = _vm.Settings.HapticsStrength / 100.0;
            _hid.Write(_vm.SnapshotSettings(), (byte)(_rumbleLarge * scale), (byte)(_rumbleSmall * scale), force: true);
        }
    }

    private void OnModeChanged(EmulationMode mode)
    {
        if (mode != EmulationMode.Xbox)
        {
            TearDownXbox();
            return;
        }

        if (!OperatingSystem.IsWindows())
        {
            Dispatcher.UIThread.Post(() => _vm.SetVigem(false, "ViGEmBus is a Windows driver."));
            return;
        }

        try
        {
            EnsureXbox();
        }
        catch (Exception ex)
        {
            AppLog.Warn("Xbox mode failed to start: " + ex.Message);
            TearDownXbox();
            Dispatcher.UIThread.Post(() => _vm.SetVigem(false, ex.Message));
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void EnsureXbox()
    {
        if (_xbox is not null)
        {
            return;
        }

        var bridge = new XboxBridge();
        bridge.RumbleReceived += (large, small) =>
        {
            _rumbleLarge = large;
            _rumbleSmall = small;
        };

        if (bridge.TryConnect(out var error))
        {
            _xbox = bridge;
            _xboxUpdate = bridge.Update;
            Dispatcher.UIThread.Post(() => _vm.SetVigem(true, null));
        }
        else
        {
            bridge.Dispose();
            Dispatcher.UIThread.Post(() => _vm.SetVigem(false, error));
        }
    }

    private void TearDownXbox()
    {
        _xboxUpdate = null;
        try
        {
            _xbox?.Dispose();
        }
        catch (Exception ex)
        {
            AppLog.Warn("ViGEm teardown: " + ex.Message);
        }

        _xbox = null;
        _rumbleLarge = 0;
        _rumbleSmall = 0;
        Dispatcher.UIThread.Post(() => _vm.SetVigem(false, null));
    }

    public void Dispose()
    {
        _cts.Cancel();
        _vm.SettingsChanged -= OnSettingsChanged;
        _vm.ModeChanged -= OnModeChanged;
        TearDownXbox();
        _hid.Dispose();
        _audio.Dispose();
        _inject.Reset();
        _cts.Dispose();
    }
}
