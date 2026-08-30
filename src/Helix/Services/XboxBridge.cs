using Helix.Models;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace Helix.Services;

public sealed class XboxBridge : IDisposable
{
    private ViGEmClient? _client;
    private IXbox360Controller? _pad;

    public event Action<byte, byte>? RumbleReceived;

    public bool TryConnect(out string? error)
    {
        error = null;
        try
        {
            _client = new ViGEmClient();
            _pad = _client.CreateXbox360Controller();
            _pad.AutoSubmitReport = true;
            _pad.FeedbackReceived += OnFeedback;
            _pad.Connect();
            AppLog.Info("ViGEm Xbox 360 virtual pad connected.");
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            AppLog.Warn("ViGEm unavailable: " + ex.Message);
            Dispose();
            return false;
        }
    }

    public void Update(PadSnapshot pad, AppSettings settings)
    {
        if (_pad is null || !pad.Connected)
        {
            Neutral();
            return;
        }

        var (lx, ly) = StickMapper.ToXInput(pad.LeftX, pad.LeftY, settings.StickDeadzone, invertY: true);
        var (rx, ry) = StickMapper.ToXInput(pad.RightX, pad.RightY, settings.StickDeadzone, invertY: true);
        if (settings.Gyro == GyroMode.RightStick)
        {
            var addX = Math.Clamp(pad.GyroY / 12000.0 * settings.GyroSensitivity, -1.0, 1.0);
            var addY = Math.Clamp(-pad.GyroX / 12000.0 * settings.GyroSensitivity, -1.0, 1.0);
            rx = MixAxis(rx, addX);
            ry = MixAxis(ry, addY);
        }

        _pad.SetAxisValue(Xbox360Axis.LeftThumbX, lx);
        _pad.SetAxisValue(Xbox360Axis.LeftThumbY, ly);
        _pad.SetAxisValue(Xbox360Axis.RightThumbX, rx);
        _pad.SetAxisValue(Xbox360Axis.RightThumbY, ry);

        var lt = pad.L2;
        var rt = pad.R2;
        ApplyFace(_pad, XboxFace.A, pad.Down(PadButtons.Cross));
        ApplyFace(_pad, XboxFace.B, pad.Down(PadButtons.Circle));
        ApplyFace(_pad, XboxFace.X, pad.Down(PadButtons.Square));
        ApplyFace(_pad, XboxFace.Y, pad.Down(PadButtons.Triangle));
        ApplyFace(_pad, XboxFace.LB, pad.Down(PadButtons.L1));
        ApplyFace(_pad, XboxFace.RB, pad.Down(PadButtons.R1));
        ApplyFace(_pad, XboxFace.LS, pad.Down(PadButtons.L3));
        ApplyFace(_pad, XboxFace.RS, pad.Down(PadButtons.R3));
        ApplyFace(_pad, XboxFace.Back, pad.Down(PadButtons.Create) || (settings.Touchpad == TouchpadMode.ClickOnly && pad.Down(PadButtons.Touchpad)));
        ApplyFace(_pad, XboxFace.Start, pad.Down(PadButtons.Options));
        _pad.SetButtonState(Xbox360Button.Guide, pad.Down(PadButtons.Ps));
        _pad.SetButtonState(Xbox360Button.Up, pad.Down(PadButtons.DpadUp));
        _pad.SetButtonState(Xbox360Button.Down, pad.Down(PadButtons.DpadDown));
        _pad.SetButtonState(Xbox360Button.Left, pad.Down(PadButtons.DpadLeft));
        _pad.SetButtonState(Xbox360Button.Right, pad.Down(PadButtons.DpadRight));
        OverlayPaddle(settings.LeftPaddle, pad.Down(PadButtons.PaddleLeft), ref lt, ref rt);
        OverlayPaddle(settings.RightPaddle, pad.Down(PadButtons.PaddleRight), ref lt, ref rt);
        _pad.SetSliderValue(Xbox360Slider.LeftTrigger, lt);
        _pad.SetSliderValue(Xbox360Slider.RightTrigger, rt);
    }

    private void OverlayPaddle(XboxFace map, bool pressed, ref byte lt, ref byte rt)
    {
        if (!pressed)
        {
            return;
        }

        switch (map)
        {
            case XboxFace.LT:
                lt = byte.MaxValue;
                break;
            case XboxFace.RT:
                rt = byte.MaxValue;
                break;
            default:
                ApplyFace(_pad!, map, true);
                break;
        }
    }

    private static void ApplyFace(IXbox360Controller pad, XboxFace face, bool down)
    {
        switch (face)
        {
            case XboxFace.A: pad.SetButtonState(Xbox360Button.A, down); break;
            case XboxFace.B: pad.SetButtonState(Xbox360Button.B, down); break;
            case XboxFace.X: pad.SetButtonState(Xbox360Button.X, down); break;
            case XboxFace.Y: pad.SetButtonState(Xbox360Button.Y, down); break;
            case XboxFace.LB: pad.SetButtonState(Xbox360Button.LeftShoulder, down); break;
            case XboxFace.RB: pad.SetButtonState(Xbox360Button.RightShoulder, down); break;
            case XboxFace.LS: pad.SetButtonState(Xbox360Button.LeftThumb, down); break;
            case XboxFace.RS: pad.SetButtonState(Xbox360Button.RightThumb, down); break;
            case XboxFace.Back: pad.SetButtonState(Xbox360Button.Back, down); break;
            case XboxFace.Start: pad.SetButtonState(Xbox360Button.Start, down); break;
        }
    }

    private static short MixAxis(short current, double add) =>
        (short)Math.Clamp((int)Math.Round((current / 32767.0 + add) * 32767.0), short.MinValue, short.MaxValue);

    private void Neutral()
    {
        if (_pad is null)
        {
            return;
        }

        _pad.SetAxisValue(Xbox360Axis.LeftThumbX, 0);
        _pad.SetAxisValue(Xbox360Axis.LeftThumbY, 0);
        _pad.SetAxisValue(Xbox360Axis.RightThumbX, 0);
        _pad.SetAxisValue(Xbox360Axis.RightThumbY, 0);
        _pad.SetSliderValue(Xbox360Slider.LeftTrigger, 0);
        _pad.SetSliderValue(Xbox360Slider.RightTrigger, 0);
        Xbox360Button[] buttons =
        [
            Xbox360Button.A, Xbox360Button.B, Xbox360Button.X, Xbox360Button.Y,
            Xbox360Button.LeftShoulder, Xbox360Button.RightShoulder,
            Xbox360Button.LeftThumb, Xbox360Button.RightThumb,
            Xbox360Button.Back, Xbox360Button.Start, Xbox360Button.Guide,
            Xbox360Button.Up, Xbox360Button.Down, Xbox360Button.Left, Xbox360Button.Right
        ];
        foreach (var button in buttons)
        {
            _pad.SetButtonState(button, false);
        }
    }

    private void OnFeedback(object sender, Xbox360FeedbackReceivedEventArgs e) =>
        RumbleReceived?.Invoke(e.LargeMotor, e.SmallMotor);

    public void Dispose()
    {
        try
        {
            if (_pad is not null)
            {
                _pad.FeedbackReceived -= OnFeedback;
                Neutral();
                _pad.Disconnect();
            }
        }
        catch (Exception ex)
        {
            AppLog.Warn("ViGEm disconnect: " + ex.Message);
        }

        _pad = null;
        _client?.Dispose();
        _client = null;
    }
}
