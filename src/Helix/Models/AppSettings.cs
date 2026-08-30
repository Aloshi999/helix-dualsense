using Avalonia.Media;

namespace Helix.Models;

public sealed class AppSettings
{
    public EmulationMode Mode { get; set; }
    public TriggerSideSettings LeftTrigger { get; set; } = new();
    public TriggerSideSettings RightTrigger { get; set; } = new();
    public string LightbarHex { get; set; } = "#2E6B9E";
    public double LightbarBrightness { get; set; } = 0.85;
    public int HapticsStrength { get; set; } = 70;
    public bool AudioToHaptics { get; set; }
    public XboxFace LeftPaddle { get; set; } = XboxFace.LB;
    public XboxFace RightPaddle { get; set; } = XboxFace.RB;
    public TouchpadMode Touchpad { get; set; } = TouchpadMode.ClickOnly;
    public GyroMode Gyro { get; set; }
    public double GyroSensitivity { get; set; } = 1.0;
    public double StickDeadzone { get; set; } = 0.12;
    public bool StartWithWindows { get; set; }

    public Color LightbarColor()
    {
        try
        {
            return Color.Parse(LightbarHex);
        }
        catch
        {
            return Color.FromRgb(46, 107, 158);
        }
    }
}
