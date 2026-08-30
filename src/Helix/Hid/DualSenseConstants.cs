namespace Helix.Hid;

public static class DualSenseConstants
{
    public const int VendorId = 0x054C;
    public const int DualSensePid = 0x0CE6;
    public const int DualSenseEdgePid = 0x0DF2;

    public const byte UsbInputReportId = 0x01;
    public const byte UsbOutputReportId = 0x02;
    public const int UsbInputSize = 64;
    public const int UsbOutputSize = 63;

    public const byte BtReportId = 0x31;
    public const int BtInputSize = 78;
    public const int BtOutputSize = 78;
    public const byte BtOutputTag = 0x10;
    public const byte BtCrcSeed = 0xA2;

    public const byte Valid0CompatibleVibration = 0x01;
    public const byte Valid0HapticsSelect = 0x02;
    public const byte Valid0RightTrigger = 0x04;
    public const byte Valid0LeftTrigger = 0x08;

    public const byte Valid1Lightbar = 0x04;
    public const byte Valid1PlayerLeds = 0x10;

    public const byte Valid2LightbarSetup = 0x02;
    public const byte Valid2CompatibleVibration2 = 0x04;

    public const byte LightbarSetupOn = 0x01;
    public const byte LightbarSetupOut = 0x02;

    public const byte TriggerOff = 0x05;
    public const byte TriggerFeedback = 0x21;
    public const byte TriggerBow = 0x22;
    public const byte TriggerWeapon = 0x25;
    public const byte TriggerVibration = 0x26;
}
