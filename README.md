# Helix

A free DualSense / DualSense Edge companion for Windows. One window. Two modes. No subscription.

**Download:** [Helix-win-x64.zip](https://github.com/Aloshi999/helix-dualsense/releases/latest/download/Helix-win-x64.zip) from [Releases](https://github.com/Aloshi999/helix-dualsense/releases/tag/v1.0.0).

Helix talks to the controller over USB HID (Sony VID `054C`, DualSense `0CE6`, Edge `0DF2`). It does not hide or disable the physical device.

**DualSense** — native HID. Games that already support DualSense see the real pad. Helix still drives adaptive triggers, rumble, and the lightbar.

**Xbox** — a Nefarius ViGEm Xbox 360 virtual pad. That is the path Red Dead Redemption 2 (Rockstar Games Launcher, no Steam) can see. Game rumble comes back through ViGEm and is played on the DualSense motors. Edge paddles remap to XInput.

Bluetooth is rumble-only. Adaptive triggers need USB-C.

## Install

1. Install [ViGEmBus](https://github.com/nefarius/ViGEmBus/releases) (free). Needed for Xbox mode. Helix still runs DualSense mode if ViGEmBus is missing.
2. Download `Helix-win-x64.zip` from [Releases](https://github.com/Aloshi999/helix-dualsense/releases/tag/v1.0.0) and unzip. Start `Helix.exe`.
3. Plug in the DualSense Edge over USB-C.
4. Pick **Xbox** for RDR2 (Rockstar Games Launcher). DualSense mode is native HID — no virtual pad.

No Steam. No DS4Windows. `ViGEmBus.sys` is not bundled.

Close goes to the tray. The `···` menu has Start with Windows, stick deadzone, the log, and Quit.

Settings live in `%AppData%\Helix\settings.json`. `Helix.log` is written next to the executable.

## Build

```bash
dotnet restore Helix.sln
dotnet test Helix.sln
dotnet publish src/Helix/Helix.csproj -c Release -r win-x64 --self-contained -o dist/win-x64
```

The tagged `v*` GitHub Actions workflow publishes `Helix-win-x64.zip` (root contains `Helix.exe`) onto the matching Release.

Developed with Avalonia 11 and C# (works on Linux; HID output and ViGEm are for Windows).

## What Helix does not do

It does not call `CM_Disable_DevNode` on the DualSense composite. It does not ship PlayStation artwork. Face-button marks on the silhouette are original geometric glyphs.
