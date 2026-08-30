using System.Runtime.InteropServices;

namespace Helix.Services;

public sealed class InputInjector
{
    private ushort _lastTouchX;
    private ushort _lastTouchY;
    private bool _haveTouch;
    private bool _mouseDown;

    public void ApplyTouchpad(bool click, bool active, ushort x, ushort y, bool asMouse)
    {
        if (!asMouse)
        {
            if (_mouseDown)
            {
                MouseButton(false);
            }

            _haveTouch = false;
            return;
        }

        if (active)
        {
            if (_haveTouch)
            {
                var dx = (x - _lastTouchX) * 1.15;
                var dy = (y - _lastTouchY) * 1.15;
                if (Math.Abs(dx) >= 0.5 || Math.Abs(dy) >= 0.5)
                {
                    MouseMove((int)Math.Round(dx), (int)Math.Round(dy));
                }
            }

            _lastTouchX = x;
            _lastTouchY = y;
            _haveTouch = true;
        }
        else
        {
            _haveTouch = false;
        }

        if (click != _mouseDown)
        {
            MouseButton(click);
        }
    }

    public void ApplyGyroMouse(short gyroYaw, short gyroPitch, double sensitivity, double dt)
    {
        var scale = 0.0042 * Math.Clamp(sensitivity, 0.15, 4.0);
        var dx = (int)Math.Round(gyroYaw * scale * (dt * 60.0));
        var dy = (int)Math.Round(-gyroPitch * scale * (dt * 60.0));
        if (dx != 0 || dy != 0)
        {
            MouseMove(dx, dy);
        }
    }

    public void Reset()
    {
        if (_mouseDown)
        {
            MouseButton(false);
        }

        _haveTouch = false;
    }

    private void MouseButton(bool down)
    {
        _mouseDown = down;
        if (OperatingSystem.IsWindows())
        {
            SendMouse(down ? 0x0002u : 0x0004u, 0, 0);
        }
    }

    private void MouseMove(int dx, int dy)
    {
        if (OperatingSystem.IsWindows())
        {
            SendMouse(0x0001u, dx, dy);
        }
    }

    private static void SendMouse(uint flags, int dx, int dy)
    {
        var input = new Input
        {
            Type = 0,
            U = new InputUnion
            {
                Mi = new MouseInput
                {
                    Dx = dx,
                    Dy = dy,
                    DwFlags = flags
                }
            }
        };
        SendInput(1, [input], Marshal.SizeOf<Input>());
    }

    private struct Input
    {
        public uint Type;
        public InputUnion U;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public MouseInput Mi;
    }

    private struct MouseInput
    {
        public int Dx;
        public int Dy;
        public uint MouseData;
        public uint DwFlags;
        public uint Time;
        public nint DwExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);
}
