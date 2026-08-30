using Helix.Models;

namespace Helix.Hid;

public static class OutputReportBuilder
{
    public static byte[] BuildUsb(AppSettings settings, byte motorLeft, byte motorRight, bool resetLights)
    {
        var report = new byte[DualSenseConstants.UsbOutputSize];
        report[0] = DualSenseConstants.UsbOutputReportId;
        WriteCommon(report.AsSpan(1), settings, motorLeft, motorRight, resetLights);
        return report;
    }

    public static byte[] BuildBluetooth(AppSettings settings, byte motorLeft, byte motorRight, bool resetLights, byte sequence)
    {
        var report = new byte[DualSenseConstants.BtOutputSize];
        report[0] = DualSenseConstants.BtReportId;
        report[1] = (byte)((sequence & 0x0F) << 4);
        report[2] = DualSenseConstants.BtOutputTag;
        WriteCommon(report.AsSpan(3), settings, motorLeft, motorRight, resetLights);
        WriteCrc(report);
        return report;
    }

    public static void WriteCommon(Span<byte> common, AppSettings settings, byte motorLeft, byte motorRight, bool resetLights)
    {
        common[0] = DualSenseConstants.Valid0CompatibleVibration
                    | DualSenseConstants.Valid0HapticsSelect
                    | DualSenseConstants.Valid0RightTrigger
                    | DualSenseConstants.Valid0LeftTrigger;
        common[1] = DualSenseConstants.Valid1Lightbar | DualSenseConstants.Valid1PlayerLeds;
        common[2] = motorRight;
        common[3] = motorLeft;

        TriggerEffects.From(settings.RightTrigger).Write(common.Slice(10, 11));
        TriggerEffects.From(settings.LeftTrigger).Write(common.Slice(21, 11));

        common[38] = DualSenseConstants.Valid2LightbarSetup | DualSenseConstants.Valid2CompatibleVibration2;
        common[41] = resetLights ? DualSenseConstants.LightbarSetupOut : DualSenseConstants.LightbarSetupOn;

        var color = settings.LightbarColor();
        var brightness = Math.Clamp(settings.LightbarBrightness, 0.0, 1.0);
        common[44] = Scale(color.R, brightness);
        common[45] = Scale(color.G, brightness);
        common[46] = Scale(color.B, brightness);
    }

    public static uint Crc32Le(uint crc, ReadOnlySpan<byte> data)
    {
        foreach (var value in data)
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++)
            {
                crc = (crc >> 1) ^ ((crc & 1) != 0 ? 0xEDB88320u : 0);
            }
        }

        return crc;
    }

    public static void WriteCrc(Span<byte> report)
    {
        var crc = Crc32Le(uint.MaxValue, [DualSenseConstants.BtCrcSeed]);
        crc = ~Crc32Le(crc, report[..^4]);
        BitConverter.TryWriteBytes(report[^4..], crc);
    }

    private static byte Scale(byte channel, double brightness) =>
        (byte)Math.Clamp(Math.Round(channel * brightness), 0, 255);
}
