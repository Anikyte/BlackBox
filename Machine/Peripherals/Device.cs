using System.Buffers.Binary;

namespace BlackBox.Machine.Peripherals;

public class Device
{
    public static Int128 GUIDv4(Random rng)
    {
        Span<byte> bytes = stackalloc byte[16];
        rng.NextBytes(bytes);
        bytes[6] = (byte)((bytes[6] & 0x0F) | 0x40); // Version 4
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80); // Variant 10
        return BinaryPrimitives.ReadInt128BigEndian(bytes);
    }

    public static Int128 GUIDv7(Random rng)
    {
        Span<byte> bytes = stackalloc byte[16];

        // 48 bits: Unix timestamp in milliseconds
        bytes[0] = (byte)(World.ShipTime >> 40);
        bytes[1] = (byte)(World.ShipTime >> 32);
        bytes[2] = (byte)(World.ShipTime >> 24);
        bytes[3] = (byte)(World.ShipTime >> 16);
        bytes[4] = (byte)(World.ShipTime >> 8);
        bytes[5] = (byte)World.ShipTime;

        // 12 bits: Random sub-millisecond precision + 4 bits version (0111)
        rng.NextBytes(bytes[6..]);
        bytes[6] = (byte)((bytes[6] & 0x0F) | 0x70); // Version 7
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80); // Variant 10

        return BinaryPrimitives.ReadInt128BigEndian(bytes);
    }
}