namespace Helix.Hid;

public readonly record struct TriggerEffect(byte Mode, byte P1, byte P2, byte P3)
{
    public static TriggerEffect Off { get; } = new(DualSenseConstants.TriggerOff, 0, 0, 0);

    public void Write(Span<byte> dest)
    {
        dest.Clear();
        dest[0] = Mode;
        dest[1] = P1;
        dest[2] = P2;
        dest[3] = P3;
    }
}
