using Helix.Hid;
using Helix.Models;

namespace Helix.Tests;

public class InputParserTests
{
    [Fact]
    public void ParsesUsbFaceButtonsAndDpad()
    {
        var report = new byte[64];
        report[0] = DualSenseConstants.UsbInputReportId;
        report[1] = 200;
        report[2] = 10;
        report[8] = 0x20 | 0x02;
        report[9] = 0x01 | 0x20;
        report[10] = 0x40;

        Assert.True(InputParser.TryParse(report, isEdge: true, treatAsBluetooth: false, out var pad));
        Assert.True(pad.Connected);
        Assert.Equal(ConnectionKind.UsbDualSenseEdge, pad.Kind);
        Assert.Equal((byte)200, pad.LeftX);
        Assert.True(pad.Down(PadButtons.Cross));
        Assert.True(pad.Down(PadButtons.DpadRight));
        Assert.True(pad.Down(PadButtons.L1));
        Assert.True(pad.Down(PadButtons.Options));
        Assert.True(pad.Down(PadButtons.PaddleLeft));
    }

    [Fact]
    public void HatToDpadCoversDiagonals()
    {
        Assert.Equal(PadButtons.DpadUp, InputParser.HatToDpad(0));
        Assert.Equal(PadButtons.DpadUp | PadButtons.DpadRight, InputParser.HatToDpad(1));
        Assert.Equal(PadButtons.None, InputParser.HatToDpad(8));
    }
}
