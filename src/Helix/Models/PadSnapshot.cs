namespace Helix.Models;

public readonly record struct PadSnapshot(
    bool Connected,
    ConnectionKind Kind,
    byte LeftX,
    byte LeftY,
    byte RightX,
    byte RightY,
    byte L2,
    byte R2,
    PadButtons Buttons,
    short GyroX,
    short GyroY,
    short GyroZ,
    bool TouchActive,
    ushort TouchX,
    ushort TouchY)
{
    public static PadSnapshot Empty { get; } = new(
        false,
        ConnectionKind.Disconnected,
        128, 128, 128, 128,
        0, 0,
        PadButtons.None,
        0, 0, 0,
        false, 0, 0);

    public bool IsEdge =>
        Kind is ConnectionKind.UsbDualSenseEdge or ConnectionKind.BluetoothDualSenseEdge;

    public bool IsBluetooth =>
        Kind is ConnectionKind.BluetoothDualSense or ConnectionKind.BluetoothDualSenseEdge;

    public string ConnectionLabel => Kind switch
    {
        ConnectionKind.UsbDualSenseEdge => "USB DualSense Edge",
        ConnectionKind.UsbDualSense => "USB DualSense",
        ConnectionKind.BluetoothDualSenseEdge => "Bluetooth DualSense Edge",
        ConnectionKind.BluetoothDualSense => "Bluetooth DualSense",
        _ => "Disconnected"
    };

    public bool Down(PadButtons button) => (Buttons & button) != 0;
}
