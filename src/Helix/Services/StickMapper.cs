namespace Helix.Services;

public static class StickMapper
{
    public static (short X, short Y) ToXInput(byte rawX, byte rawY, double deadzone, bool invertY)
    {
        var x = Normalize(rawX);
        var y = invertY ? -Normalize(rawY) : Normalize(rawY);
        var length = Math.Sqrt(x * x + y * y);
        if (length < deadzone)
        {
            return (0, 0);
        }

        if (length > 1.0)
        {
            x /= length;
            y /= length;
            length = 1.0;
        }

        var scaled = (length - deadzone) / (1.0 - deadzone);
        x = x / length * scaled;
        y = y / length * scaled;
        return (ToAxis(x), ToAxis(y));
    }

    public static double Normalize(byte value)
    {
        if (value == 128)
        {
            return 0;
        }

        return value >= 128
            ? (value - 128) / 127.0
            : (value - 128) / 128.0;
    }

    private static short ToAxis(double n) =>
        (short)Math.Clamp((int)Math.Round(n * 32767.0), short.MinValue, short.MaxValue);
}
