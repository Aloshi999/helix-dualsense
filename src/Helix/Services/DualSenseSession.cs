using Helix.Hid;
using Helix.Models;
using HidSharp;

namespace Helix.Services;

public sealed class DualSenseSession : IDisposable
{
    private HidStream? _stream;
    private HidDevice? _device;
    private bool _loggedBluetooth;
    private byte _btSeq;
    private bool _resetLights = true;
    private DateTime _lastWrite = DateTime.MinValue;

    public bool IsOpen => _stream is not null;
    public bool IsEdge { get; private set; }
    public bool IsBluetooth { get; private set; }

    public bool TryOpen()
    {
        Close();
        foreach (var device in DeviceList.Local.GetHidDevices(DualSenseConstants.VendorId))
        {
            if (device.ProductID is not (DualSenseConstants.DualSensePid or DualSenseConstants.DualSenseEdgePid))
            {
                continue;
            }

            int maxInput;
            try
            {
                maxInput = device.GetMaxInputReportLength();
            }
            catch
            {
                continue;
            }

            if (maxInput < DualSenseConstants.UsbInputSize)
            {
                continue;
            }

            try
            {
                var config = new OpenConfiguration();
                config.SetOption(OpenOption.Exclusive, false);
                var stream = device.Open(config);
                stream.ReadTimeout = 40;
                stream.WriteTimeout = 40;
                _device = device;
                _stream = stream;
                IsEdge = device.ProductID == DualSenseConstants.DualSenseEdgePid;
                IsBluetooth = maxInput >= DualSenseConstants.BtInputSize || LooksBluetooth(device.DevicePath);
                _resetLights = true;
                _loggedBluetooth = false;
                AppLog.Info($"Opened {(IsEdge ? "DualSense Edge" : "DualSense")} via {(IsBluetooth ? "Bluetooth" : "USB")} ({device.DevicePath}).");
                if (IsBluetooth && !_loggedBluetooth)
                {
                    AppLog.Info("Bluetooth connection: adaptive triggers require USB. Rumble-only output.");
                    _loggedBluetooth = true;
                }

                return true;
            }
            catch (Exception ex)
            {
                AppLog.Warn("Could not open " + device.DevicePath + ": " + ex.Message);
            }
        }

        return false;
    }

    public bool TryRead(out PadSnapshot snapshot)
    {
        snapshot = PadSnapshot.Empty;
        if (_stream is null || _device is null)
        {
            return false;
        }

        try
        {
            var buffer = new byte[Math.Max(_device.GetMaxInputReportLength(), DualSenseConstants.UsbInputSize)];
            var read = _stream.Read(buffer);
            return read > 0 && InputParser.TryParse(buffer.AsSpan(0, read), IsEdge, IsBluetooth, out snapshot);
        }
        catch (TimeoutException)
        {
            return false;
        }
        catch (IOException)
        {
            AppLog.Warn("Controller I/O lost.");
            Close();
            return false;
        }
        catch (Exception ex)
        {
            AppLog.Warn("Read failed: " + ex.Message);
            Close();
            return false;
        }
    }

    public void Write(AppSettings settings, byte motorLeft, byte motorRight, bool force = false)
    {
        if (_stream is null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        if (!force && now - _lastWrite < TimeSpan.FromMilliseconds(12))
        {
            return;
        }

        try
        {
            byte[] report;
            if (IsBluetooth)
            {
                report = OutputReportBuilder.BuildBluetooth(CloneRumbleOnly(settings), motorLeft, motorRight, _resetLights, _btSeq);
                _btSeq = (byte)((_btSeq + 1) & 0x0F);
            }
            else
            {
                report = OutputReportBuilder.BuildUsb(settings, motorLeft, motorRight, _resetLights);
            }

            _stream.Write(report);
            _lastWrite = now;
            _resetLights = false;
        }
        catch (Exception ex)
        {
            AppLog.Warn("Output report failed: " + ex.Message);
            Close();
        }
    }

    private static AppSettings CloneRumbleOnly(AppSettings settings) => new()
    {
        Mode = settings.Mode,
        LeftTrigger = new TriggerSideSettings { Preset = TriggerPreset.Off },
        RightTrigger = new TriggerSideSettings { Preset = TriggerPreset.Off },
        LightbarHex = settings.LightbarHex,
        LightbarBrightness = settings.LightbarBrightness,
        HapticsStrength = settings.HapticsStrength
    };

    private static bool LooksBluetooth(string? path) =>
        !string.IsNullOrEmpty(path) &&
        (path.Contains("BTHENUM", StringComparison.OrdinalIgnoreCase)
         || path.Contains("00001124", StringComparison.OrdinalIgnoreCase)
         || path.Contains("Bluetooth", StringComparison.OrdinalIgnoreCase));

    public void Close()
    {
        try
        {
            _stream?.Dispose();
        }
        catch
        {
            // Device already gone.
        }

        _stream = null;
        _device = null;
        IsEdge = false;
        IsBluetooth = false;
    }

    public void Dispose() => Close();
}
