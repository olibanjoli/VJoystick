using System.Runtime.CompilerServices;

public static class Mikado
{
    const int VBAR_STICK_MIN = 2048 - 1600;
    const int VBAR_STICK_MAX = 2048 + 1600;
    const int VBAR_RANGE = VBAR_STICK_MAX - VBAR_STICK_MIN;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int MapRange(int vbarValue, long maxRange)
    {
        var value = (((vbarValue - VBAR_STICK_MIN) * maxRange) / VBAR_RANGE) + 0;

        return (int)value;
    }
}

//
// struct ControlData
// {
//     ushort id;
//     char version;
//     char command;
//     uint switches; // 32 Switch bits
//     ushort[] analog; // maximal 10 Analog Channels
// }
//
// struct Identification
// {
//     public ushort id;
//     public char version;
//     public char command;
//     public UInt32 serial;
//     public char len;
//     public char[] name; // [41]
// }
//
// struct SimIdentification
// {
//     public ushort id;
//     public char version;
//     public char command;
//     public char sim_ident;
// }