namespace Helix.Models;

public sealed class TriggerSideSettings
{
    public TriggerPreset Preset { get; set; }
    public int Intensity { get; set; } = 5;
    public int CustomStart { get; set; }
    public int CustomEnd { get; set; } = 8;
    public int CustomForce { get; set; } = 5;
}
