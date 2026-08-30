namespace Helix.Models;

[Flags]
public enum PadButtons : uint
{
    None = 0,
    Square = 1,
    Cross = 2,
    Circle = 4,
    Triangle = 8,
    L1 = 0x10,
    R1 = 0x20,
    L2 = 0x40,
    R2 = 0x80,
    Create = 0x100,
    Options = 0x200,
    L3 = 0x400,
    R3 = 0x800,
    Ps = 0x1000,
    Touchpad = 0x2000,
    Mute = 0x4000,
    DpadUp = 0x8000,
    DpadRight = 0x10000,
    DpadDown = 0x20000,
    DpadLeft = 0x40000,
    FnLeft = 0x80000,
    FnRight = 0x100000,
    PaddleLeft = 0x200000,
    PaddleRight = 0x400000
}
