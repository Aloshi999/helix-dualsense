using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Helix.Models;

namespace Helix.Views;

public sealed class ControllerSilhouette : Control
{
    private enum Glyph
    {
        Triangle,
        Square,
        Circle,
        Cross
    }

    public static readonly StyledProperty<PadSnapshot> PadProperty =
        AvaloniaProperty.Register<ControllerSilhouette, PadSnapshot>(nameof(Pad), PadSnapshot.Empty);

    public static readonly StyledProperty<Color> LightbarProperty =
        AvaloniaProperty.Register<ControllerSilhouette, Color>(nameof(Lightbar), Color.FromRgb(46, 107, 158));

    public static readonly StyledProperty<double> LightbarBrightnessProperty =
        AvaloniaProperty.Register<ControllerSilhouette, double>(nameof(LightbarBrightness), 0.85);

    static ControllerSilhouette()
    {
        AffectsRender<ControllerSilhouette>(PadProperty, LightbarProperty, LightbarBrightnessProperty);
    }

    public PadSnapshot Pad
    {
        get => GetValue(PadProperty);
        set => SetValue(PadProperty, value);
    }

    public Color Lightbar
    {
        get => GetValue(LightbarProperty);
        set => SetValue(LightbarProperty, value);
    }

    public double LightbarBrightness
    {
        get => GetValue(LightbarBrightnessProperty);
        set => SetValue(LightbarBrightnessProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        var width = Bounds.Width;
        var height = Bounds.Height;
        if (width < 8 || height < 8)
        {
            return;
        }

        var pad = Pad;
        var connected = pad.Connected;
        using var opacity = context.PushOpacity(connected ? 1 : 0.32);
        var scale = Math.Min(width / 480.0, height / 400.0);
        var ox = (width - 480 * scale) / 2;
        var oy = (height - 400 * scale) / 2 + 6 * scale;
        using var transform = context.PushTransform(Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(ox, oy));

        var stroke = new Pen(Brush("#8AA0B2"), 1.35) { LineJoin = PenLineJoin.Round };
        var wellStroke = new Pen(Brush("#2C3944"), 1.1);
        var well = Brush("#0A0D10");
        var accent = Color.FromRgb(61, 154, 209);

        DrawPaddles(context, pad, connected);
        DrawTriggers(context, pad, stroke, accent);
        DrawBody(context, stroke);
        DrawTouchpad(context, pad, wellStroke, well);
        DrawLightbar(context, connected);
        DrawStick(context, 148, 158, pad.LeftX, pad.LeftY, pad.Down(PadButtons.L3), well, wellStroke, accent);
        DrawStick(context, 332, 248, pad.RightX, pad.RightY, pad.Down(PadButtons.R3), well, wellStroke, accent);
        DrawDpad(context, 148, 250, pad, accent);
        DrawFace(context, 332, 150, pad, accent);
        DrawSmall(context, 176, 118, 7, pad.Down(PadButtons.Create), accent, "create");
        DrawSmall(context, 304, 118, 7, pad.Down(PadButtons.Options), accent, "options");
        DrawSmall(context, 240, 204, 8.5, pad.Down(PadButtons.Ps), accent, "home");
        DrawSmall(context, 240, 178, 5.5, pad.Down(PadButtons.Mute), accent, "mute");
        DrawFn(context, 92, 210, pad.Down(PadButtons.FnLeft), accent);
        DrawFn(context, 388, 210, pad.Down(PadButtons.FnRight), accent);
    }

    private static void DrawBody(DrawingContext ctx, Pen stroke)
    {
        var body = new StreamGeometry();
        using (var g = body.Open())
        {
            g.BeginFigure(new Point(132, 78), true);
            g.CubicBezierTo(new Point(168, 62), new Point(210, 58), new Point(240, 58));
            g.CubicBezierTo(new Point(270, 58), new Point(312, 62), new Point(348, 78));
            g.CubicBezierTo(new Point(392, 96), new Point(424, 128), new Point(432, 168));
            g.CubicBezierTo(new Point(444, 228), new Point(448, 292), new Point(428, 338));
            g.CubicBezierTo(new Point(414, 370), new Point(378, 378), new Point(348, 360));
            g.CubicBezierTo(new Point(318, 342), new Point(292, 310), new Point(280, 286));
            g.LineTo(new Point(200, 286));
            g.CubicBezierTo(new Point(188, 310), new Point(162, 342), new Point(132, 360));
            g.CubicBezierTo(new Point(102, 378), new Point(66, 370), new Point(52, 338));
            g.CubicBezierTo(new Point(32, 292), new Point(36, 228), new Point(48, 168));
            g.CubicBezierTo(new Point(56, 128), new Point(88, 96), new Point(132, 78));
            g.EndFigure(true);
        }

        ctx.DrawGeometry(Brush("#12171C"), stroke, body);

        var seam = new StreamGeometry();
        using (var g = seam.Open())
        {
            g.BeginFigure(new Point(198, 286), false);
            g.CubicBezierTo(new Point(220, 304), new Point(260, 304), new Point(282, 286));
            g.EndFigure(false);
        }

        ctx.DrawGeometry(null, new Pen(Brush("#24303A"), 1), seam);
    }

    private static void DrawTriggers(DrawingContext ctx, PadSnapshot pad, Pen stroke, Color accent)
    {
        DrawTrigger(ctx, 118, 36, pad.L2, pad.Down(PadButtons.L1), stroke, accent);
        DrawTrigger(ctx, 362, 36, pad.R2, pad.Down(PadButtons.R1), stroke, accent);
    }

    private static void DrawTrigger(DrawingContext ctx, double x, double y, byte analog, bool bumper, Pen stroke, Color accent)
    {
        var drop = analog / 255.0 * 16.0;
        var fill = analog > 8 ? Glow(accent, 0.18 + analog / 255.0 * 0.45) : Brush("#161B20");
        ctx.DrawRectangle(fill, stroke, new Rect(x - 28, y + drop, 56, 22), 7, 7);
        ctx.DrawRectangle(bumper ? Glow(accent, 0.72) : Brush("#1A2128"), stroke, new Rect(x - 30, y + 24, 60, 14), 5, 5);
    }

    private static void DrawPaddles(DrawingContext ctx, PadSnapshot pad, bool connected)
    {
        DrawPaddle(ctx, 128, 318, pad.Down(PadButtons.PaddleLeft), true, connected);
        DrawPaddle(ctx, 352, 318, pad.Down(PadButtons.PaddleRight), false, connected);
    }

    private static void DrawPaddle(DrawingContext ctx, double x, double y, bool down, bool left, bool connected)
    {
        var degrees = down ? (left ? -11 : 11) : (left ? -3 : 3);
        var color = down ? Color.FromRgb(61, 154, 209) : Color.FromRgb(42, 54, 64);
        using var transform = ctx.PushTransform(Matrix.CreateRotation(degrees * Math.PI / 180) * Matrix.CreateTranslation(x, y));
        ctx.DrawRectangle(
            new SolidColorBrush(color, connected ? 0.95 : 0.5),
            new Pen(Brush("#1C262E"), 1),
            new Rect(-22, -7, 44, 16),
            8, 8);
    }

    private static void DrawTouchpad(DrawingContext ctx, PadSnapshot pad, Pen stroke, IBrush well)
    {
        var rect = new Rect(186, 92, 108, 58);
        var fill = pad.Down(PadButtons.Touchpad) ? Glow(Color.FromRgb(61, 154, 209), 0.28) : well;
        ctx.DrawRectangle(fill, stroke, rect, 10, 10);
        if (pad.TouchActive)
        {
            var tx = rect.X + 8 + pad.TouchX / 1920.0 * (rect.Width - 16);
            var ty = rect.Y + 8 + pad.TouchY / 1080.0 * (rect.Height - 16);
            ctx.DrawEllipse(Glow(Color.FromRgb(61, 154, 209), 0.9), null, new Point(tx, ty), 4.2, 4.2);
        }
    }

    private void DrawLightbar(DrawingContext ctx, bool connected)
    {
        var color = Lightbar;
        var dim = Math.Clamp(LightbarBrightness, 0, 1) * (connected ? 1 : 0.15);
        var lit = Color.FromRgb((byte)(color.R * dim), (byte)(color.G * dim), (byte)(color.B * dim));
        var bar = new Rect(176, 162, 128, 10);
        ctx.DrawRectangle(new SolidColorBrush(lit, 0.22), null, bar.Inflate(10), 10, 10);
        ctx.DrawRectangle(new SolidColorBrush(lit, 0.45), null, bar.Inflate(4), 8, 8);
        ctx.DrawRectangle(new SolidColorBrush(lit), null, bar, 5, 5);
    }

    private static void DrawStick(DrawingContext ctx, double x, double y, byte ax, byte ay, bool click, IBrush well, Pen stroke, Color accent)
    {
        ctx.DrawEllipse(well, stroke, new Point(x, y), 28, 28);
        ctx.DrawEllipse(null, new Pen(Brush("#1E2A33"), 1), new Point(x, y), 22, 22);
        var nx = (ax - 128) / 128.0;
        var ny = (ay - 128) / 128.0;
        var cx = x + nx * 11;
        var cy = y + ny * 11;
        ctx.DrawEllipse(click ? Glow(accent, 0.85) : Brush("#1C242C"), new Pen(Brush("#3A4A56"), 1.1), new Point(cx, cy), 13, 13);
        ctx.DrawEllipse(new SolidColorBrush(Color.FromRgb(61, 74, 85), 0.35), null, new Point(cx, cy), 4.5, 4.5);
    }

    private static void DrawDpad(DrawingContext ctx, double x, double y, PadSnapshot pad, Color accent)
    {
        DrawArm(ctx, new Rect(x - 7, y - 24, 14, 18), pad.Down(PadButtons.DpadUp), accent);
        DrawArm(ctx, new Rect(x - 7, y + 6, 14, 18), pad.Down(PadButtons.DpadDown), accent);
        DrawArm(ctx, new Rect(x - 24, y - 7, 18, 14), pad.Down(PadButtons.DpadLeft), accent);
        DrawArm(ctx, new Rect(x + 6, y - 7, 18, 14), pad.Down(PadButtons.DpadRight), accent);
        ctx.DrawEllipse(Brush("#101418"), null, new Point(x, y), 5, 5);
    }

    private static void DrawArm(DrawingContext ctx, Rect r, bool on, Color accent) =>
        ctx.DrawRectangle(on ? Glow(accent, 0.9) : Brush("#1A2128"), new Pen(Brush("#2A3640"), 1), r, 3, 3);

    private static void DrawFace(DrawingContext ctx, double x, double y, PadSnapshot pad, Color accent)
    {
        DrawGlyph(ctx, x, y - 22, pad.Down(PadButtons.Triangle), accent, Glyph.Triangle);
        DrawGlyph(ctx, x - 22, y, pad.Down(PadButtons.Square), accent, Glyph.Square);
        DrawGlyph(ctx, x + 22, y, pad.Down(PadButtons.Circle), accent, Glyph.Circle);
        DrawGlyph(ctx, x, y + 22, pad.Down(PadButtons.Cross), accent, Glyph.Cross);
    }

    private static void DrawGlyph(DrawingContext ctx, double x, double y, bool on, Color accent, Glyph glyph)
    {
        ctx.DrawEllipse(on ? Glow(accent, 0.88) : Brush("#171D22"), new Pen(Brush("#2C3944"), 1), new Point(x, y), 11.5, 11.5);
        var ink = on ? Brushes.White : Brush("#7E8D9A");
        var pen = new Pen(ink, 1.25) { LineJoin = PenLineJoin.Round, LineCap = PenLineCap.Round };
        switch (glyph)
        {
            case Glyph.Triangle:
                ctx.DrawLine(pen, new Point(x, y - 5.5), new Point(x + 5.2, y + 4.4));
                ctx.DrawLine(pen, new Point(x + 5.2, y + 4.4), new Point(x - 5.2, y + 4.4));
                ctx.DrawLine(pen, new Point(x - 5.2, y + 4.4), new Point(x, y - 5.5));
                break;
            case Glyph.Square:
                ctx.DrawRectangle(null, pen, new Rect(x - 4.4, y - 4.4, 8.8, 8.8), 1.2, 1.2);
                break;
            case Glyph.Circle:
                ctx.DrawEllipse(null, pen, new Point(x, y), 4.6, 4.6);
                break;
            case Glyph.Cross:
                ctx.DrawLine(pen, new Point(x - 4.2, y - 4.2), new Point(x + 4.2, y + 4.2));
                ctx.DrawLine(pen, new Point(x + 4.2, y - 4.2), new Point(x - 4.2, y + 4.2));
                break;
        }
    }

    private static void DrawSmall(DrawingContext ctx, double x, double y, double r, bool on, Color accent, string kind)
    {
        ctx.DrawEllipse(on ? Glow(accent, 0.85) : Brush("#161C21"), new Pen(Brush("#2A3640"), 1), new Point(x, y), r, r);
        var ink = on ? Brushes.White : Brush("#6E7C88");
        var pen = new Pen(ink, 1.05);
        if (kind == "home")
        {
            ctx.DrawEllipse(null, pen, new Point(x, y), r * 0.38, r * 0.38);
        }
        else if (kind == "mute")
        {
            ctx.DrawLine(pen, new Point(x - 3, y), new Point(x + 3, y));
        }
    }

    private static void DrawFn(DrawingContext ctx, double x, double y, bool on, Color accent) =>
        ctx.DrawRectangle(on ? Glow(accent, 0.8) : Brush("#141A1F"), new Pen(Brush("#2A3640"), 1), new Rect(x - 10, y - 6, 20, 12), 4, 4);

    private static IBrush Brush(string hex) => new SolidColorBrush(Color.Parse(hex));
    private static IBrush Glow(Color c, double a) => new SolidColorBrush(c, a);
}
