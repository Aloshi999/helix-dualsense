# Helix

A free DualSense / DualSense Edge companion for Windows. One window. Two modes. No subscription.

Helix talks to the controller over USB HID (Sony VID `054C`, DualSense `0CE6`, Edge `0DF2`). It does not hide or disable the physical device.

**DualSense** — native HID. Games that already support DualSense see the real pad. Helix still drives adaptive triggers, rumble, and the lightbar.

**Xbox** — a Nefarius ViGEm Xbox 360 virtual pad. That is the path Red Dead Redemption 2 (Rockstar Games Launcher, no Steam) can see. Game rumble comes back through ViGEm and is played on the DualSense motors. Edge paddles remap to XInput.

Bluetooth is rumble-only. Adaptive triggers need USB-C.

## Requirements

- Windows 10/11 x64
- DualSense or DualSense Edge over USB-C
- [ViGEmBus](https://github.com/nefarius/ViGEmBus/releases) if you want Xbox mode (free). Helix runs DualSense mode without it.

No Steam. No DS4Windows. ViGEmBus.sys is not bundled.

## Run

See `INSTALL.txt`. Unzip `Helix-win-x64.zip` and start `Helix.exe`.

Close goes to the tray. The `...` menu has Start with Windows, stick deadzone, the log, and Quit.

Settings live in `%AppData%\Helix\settings.json`. `Helix.log` is written next to the executable.

## Build

```bash
dotnet restore Helix.sln
dotnet test Helix.sln
dotnet publish src/Helix/Helix.csproj -c Release -r win-x64 --self-contained true -o dist/win-x64
```

Developed with Avalonia 11 and C# (works on Linux; HID output and ViGEm are for Windows).

## What Helix does not do

It does not call `CM_Disable_DevNode` on the DualSense composite. It does not ship PlayStation artwork. Face-button marks on the silhouette are original geometric glyphs.
