using Helix.Models;
using Helix.Services;

namespace Helix.Tests;

public class SettingsStoreTests
{
    [Fact]
    public void RoundTripsSettingsJson()
    {
        var path = Path.Combine(Path.GetTempPath(), "helix-tests-" + Guid.NewGuid().ToString("n"), "settings.json");
        try
        {
            var settings = new AppSettings
            {
                Mode = EmulationMode.Xbox,
                StickDeadzone = 0.12,
                LeftPaddle = XboxFace.A,
                Touchpad = TouchpadMode.ClickOnly,
                Gyro = GyroMode.Off,
                LeftTrigger = new TriggerSideSettings { Preset = TriggerPreset.Rifle, Intensity = 6 }
            };
            SettingsStore.SaveTo(path, settings);
            var loaded = SettingsStore.LoadFrom(path);
            Assert.Equal(EmulationMode.Xbox, loaded.Mode);
            Assert.Equal(0.12, loaded.StickDeadzone);
            Assert.Equal(XboxFace.A, loaded.LeftPaddle);
            Assert.Equal(TouchpadMode.ClickOnly, loaded.Touchpad);
            Assert.Equal(GyroMode.Off, loaded.Gyro);
            Assert.Equal(TriggerPreset.Rifle, loaded.LeftTrigger.Preset);
            Assert.Equal(6, loaded.LeftTrigger.Intensity);
        }
        finally
        {
            var dir = Path.GetDirectoryName(path);
            if (dir is not null)
            {
                try { Directory.Delete(dir, true); } catch { /* leftover temp is fine */ }
            }
        }
    }
}
