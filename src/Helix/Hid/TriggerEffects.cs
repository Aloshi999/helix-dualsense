using Helix.Models;

namespace Helix.Hid;

public static class TriggerEffects
{
    public static TriggerEffect From(TriggerSideSettings side)
    {
        var intensity = (byte)Math.Clamp(side.Intensity, 1, 8);
        return side.Preset switch
        {
            TriggerPreset.Off => TriggerEffect.Off,
            TriggerPreset.Pistol => new TriggerEffect(DualSenseConstants.TriggerWeapon, 2, 5, intensity),
            TriggerPreset.Shotgun => new TriggerEffect(DualSenseConstants.TriggerWeapon, 0, 6, intensity),
            TriggerPreset.Rifle => new TriggerEffect(DualSenseConstants.TriggerFeedback, 0, intensity, 0),
            TriggerPreset.Bow => new TriggerEffect(DualSenseConstants.TriggerBow, 1, 8, intensity),
            TriggerPreset.Racing => new TriggerEffect(DualSenseConstants.TriggerVibration, 0, intensity, (byte)(4 + intensity)),
            TriggerPreset.Custom => Custom(side),
            _ => TriggerEffect.Off
        };
    }

    private static TriggerEffect Custom(TriggerSideSettings side)
    {
        var start = (byte)Math.Clamp(side.CustomStart, 0, 8);
        var end = (byte)Math.Clamp(side.CustomEnd, 0, 8);
        var force = (byte)Math.Clamp(side.CustomForce, 0, 8);
        return end > start
            ? new TriggerEffect(DualSenseConstants.TriggerWeapon, start, end, force)
            : new TriggerEffect(DualSenseConstants.TriggerFeedback, start, force, 0);
    }
}
