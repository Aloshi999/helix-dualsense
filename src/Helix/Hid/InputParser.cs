using Helix.Models;

namespace Helix.Hid;

public static class InputParser
{
    public static bool TryParse(ReadOnlySpan<byte> report, bool isEdge, bool treatAsBluetooth, out PadSnapshot snapshot)
    {
        snapshot = PadSnapshot.Empty;
        if (report.IsEmpty)
        {
            return false;
        }

        var bluetooth = treatAsBluetooth || report[0] == DualSenseConstants.BtReportId;
        var payloadOffset = bluetooth ? 2 : (report[0] == DualSenseConstants.UsbInputReportId ? 1 : 0);
        if (report.Length < payloadOffset + 10)
        {
            return false;
        }

        var p = report[payloadOffset..];
        var buttons = ParseButtons(p);
        var gyroX = ReadI16(p, 16);
        var gyroY = ReadI16(p, 18);
        var gyroZ = ReadI16(p, 20);
        var touchActive = false;
        ushort touchX = 0;
        ushort touchY = 0;
        if (p.Length >= 37)
        {
            touchActive = (p[33] & 0x80) == 0;
            touchX = (ushort)(p[34] | ((p[35] & 0x0F) << 8));
            touchY = (ushort)((p[35] >> 4) | (p[36] << 4));
        }

        var kind = bluetooth
            ? (isEdge ? ConnectionKind.BluetoothDualSenseEdge : ConnectionKind.BluetoothDualSense)
            : (isEdge ? ConnectionKind.UsbDualSenseEdge : ConnectionKind.UsbDualSense);

        snapshot = new PadSnapshot(
            true,
            kind,
            p[0], p[1], p[2], p[3], p[4], p[5],
            buttons,
            gyroX, gyroY, gyroZ,
            touchActive, touchX, touchY);
        return true;
    }

    public static PadButtons ParseButtons(ReadOnlySpan<byte> p)
    {
        var buttons = PadButtons.None;
        var face = p[7];
        var hat = face & 0x0F;
        if ((face & 0x10) != 0) buttons |= PadButtons.Square;
        if ((face & 0x20) != 0) buttons |= PadButtons.Cross;
        if ((face & 0x40) != 0) buttons |= PadButtons.Circle;
        if ((face & 0x80) != 0) buttons |= PadButtons.Triangle;

        var shoulders = p[8];
        if ((shoulders & 0x01) != 0) buttons |= PadButtons.L1;
        if ((shoulders & 0x02) != 0) buttons |= PadButtons.R1;
        if ((shoulders & 0x04) != 0) buttons |= PadButtons.L2;
        if ((shoulders & 0x08) != 0) buttons |= PadButtons.R2;
        if ((shoulders & 0x10) != 0) buttons |= PadButtons.Create;
        if ((shoulders & 0x20) != 0) buttons |= PadButtons.Options;
        if ((shoulders & 0x40) != 0) buttons |= PadButtons.L3;
        if ((shoulders & 0x80) != 0) buttons |= PadButtons.R3;

        if (p.Length > 9)
        {
            var extra = p[9];
            if ((extra & 0x01) != 0) buttons |= PadButtons.Ps;
            if ((extra & 0x02) != 0) buttons |= PadButtons.Touchpad;
            if ((extra & 0x04) != 0) buttons |= PadButtons.Mute;
            if ((extra & 0x10) != 0) buttons |= PadButtons.FnLeft;
            if ((extra & 0x20) != 0) buttons |= PadButtons.FnRight;
            if ((extra & 0x40) != 0) buttons |= PadButtons.PaddleLeft;
            if ((extra & 0x80) != 0) buttons |= PadButtons.PaddleRight;
        }

        return buttons | HatToDpad(hat);
    }

    public static PadButtons HatToDpad(int hat) => hat switch
    {
        0 => PadButtons.DpadUp,
        1 => PadButtons.DpadUp | PadButtons.DpadRight,
        2 => PadButtons.DpadRight,
        3 => PadButtons.DpadRight | PadButtons.DpadDown,
        4 => PadButtons.DpadDown,
        5 => PadButtons.DpadDown | PadButtons.DpadLeft,
        6 => PadButtons.DpadLeft,
        7 => PadButtons.DpadUp | PadButtons.DpadLeft,
        _ => PadButtons.None
    };

    private static short ReadI16(ReadOnlySpan<byte> p, int offset)
    {
        if (p.Length < offset + 2)
        {
            return 0;
        }

        return (short)(p[offset] | (p[offset + 1] << 8));
    }
}
