using System.Buffers.Binary;

namespace BlackBox.Machine.Peripherals;

public class GUID
{
    private static string BytesToGUID(ReadOnlySpan<byte> bytes) => //horrific but it works
        $"{bytes[0]:x2}{bytes[1]:x2}{bytes[2]:x2}{bytes[3]:x2}-{bytes[4]:x2}{bytes[5]:x2}-{bytes[6]:x2}{bytes[7]:x2}-{bytes[8]:x2}{bytes[9]:x2}-{bytes[10]:x2}{bytes[11]:x2}{bytes[12]:x2}{bytes[13]:x2}{bytes[14]:x2}{bytes[15]:x2}";

    public static string GUIDv4(Random rng)
    {
        Span<byte> bytes = stackalloc byte[16];
        
        rng.NextBytes(bytes);
        bytes[6] = (byte)((bytes[6] & 0x0F) | 0x40); // Version 4
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80); // Variant 10
        
        return BytesToGUID(bytes);
    }

    public static string GUIDv7(Random rng)
    {
        Span<byte> bytes = stackalloc byte[16];

        bytes[0] = (byte)(World.ShipTime >> 40);
        bytes[1] = (byte)(World.ShipTime >> 32);
        bytes[2] = (byte)(World.ShipTime >> 24);
        bytes[3] = (byte)(World.ShipTime >> 16);
        bytes[4] = (byte)(World.ShipTime >> 8);
        bytes[5] = (byte)World.ShipTime;

        rng.NextBytes(bytes[6..]);
        bytes[6] = (byte)((bytes[6] & 0x0F) | 0x70); // Version 7
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80); // Variant 10

        return BytesToGUID(bytes);
    }

    public static string GUIDv8(Random rng, Span<byte> four, Span<byte> twoOne, Span<byte> twoTwo, Span<byte> twoThree)
    {
        Span<byte> bytes = stackalloc byte[16];

        bytes[0] = four[0];
        bytes[1] = four[1];
        bytes[2] = four[2];
        bytes[3] = four[3];
        
        bytes[4] = twoOne[0];
        bytes[5] = twoOne[1];
        
        bytes[6] = twoTwo[0];
        bytes[7] = twoTwo[1];
        
        bytes[8] = twoThree[0];
        bytes[9] = twoThree[1];
        
        rng.NextBytes(bytes[10..]);
        bytes[6] = (byte)((bytes[6] & 0x0F) | 0x80); // Version 8
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80); // Variant 10
        
        return BytesToGUID(bytes);
    }
}