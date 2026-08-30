using Helix.Hid;
using Helix.Models;

namespace Helix.Tests;

public class OutputReportBuilderTests
{
    [Fact]
    public void UsbReportIs63BytesWithId02()
    {
        var settings = new AppSettings
        {
            LightbarHex = "#2E6B9E",
            LightbarBrightness = 1,
            RightTrigger = new TriggerSideSettings { Preset = TriggerPreset.Pistol, Intensity = 6 },
            LeftTrigger = new TriggerSideSettings { Preset = TriggerPreset.Off }
        };

        var report = OutputReportBuilder.BuildUsb(settings, 40, 80, resetLights: false);
        Assert.Equal(63, report.Length);
        Assert.Equal(DualSenseConstants.UsbOutputReportId, report[0]);
        Assert.Equal((byte)(DualSenseConstants.Valid0CompatibleVibration
                            | DualSenseConstants.Valid0HapticsSelect
                            | DualSenseConstants.Valid0RightTrigger
                            | DualSenseConstants.Valid0LeftTrigger), report[1]);
        Assert.Equal(80, report[3]);
        Assert.Equal(40, report[4]);
        Assert.Equal(DualSenseConstants.TriggerWeapon, report[11]);
        Assert.Equal(DualSenseConstants.TriggerOff, report[22]);
        Assert.Equal(0x2E, report[45]);
        Assert.Equal(0x6B, report[46]);
        Assert.Equal(0x9E, report[47]);
    }

    [Fact]
    public void BluetoothReportHasCrcTrailer()
    {
        var report = OutputReportBuilder.BuildBluetooth(new AppSettings(), 0, 0, false, 3);
        Assert.Equal(78, report.Length);
        Assert.Equal(DualSenseConstants.BtReportId, report[0]);
        Assert.Equal((byte)(3 << 4), report[1]);
        Assert.True(report[^4] != 0 || report[^3] != 0 || report[^2] != 0 || report[^1] != 0);
    }
}
