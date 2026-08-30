using Helix.Hid;
using Helix.Models;

namespace Helix.Tests;

public class TriggerEffectsTests
{
    [Fact]
    public void OffUsesResetMode()
    {
        var effect = TriggerEffects.From(new TriggerSideSettings { Preset = TriggerPreset.Off });
        Assert.Equal(DualSenseConstants.TriggerOff, effect.Mode);
    }

    [Fact]
    public void IntensityClampsToOneThroughEight()
    {
        var high = TriggerEffects.From(new TriggerSideSettings { Preset = TriggerPreset.Pistol, Intensity = 99 });
        var low = TriggerEffects.From(new TriggerSideSettings { Preset = TriggerPreset.Pistol, Intensity = 0 });
        Assert.Equal(8, high.P3);
        Assert.Equal(1, low.P3);
    }

    [Fact]
    public void CustomUsesWeaponWhenEndAfterStart()
    {
        var effect = TriggerEffects.From(new TriggerSideSettings
        {
            Preset = TriggerPreset.Custom,
            CustomStart = 2,
            CustomEnd = 7,
            CustomForce = 4
        });
        Assert.Equal(DualSenseConstants.TriggerWeapon, effect.Mode);
        Assert.Equal(2, effect.P1);
        Assert.Equal(7, effect.P2);
        Assert.Equal(4, effect.P3);
    }
}
