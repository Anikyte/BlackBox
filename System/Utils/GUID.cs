using BlackBox.Machine;

namespace System.Utils;

public class GUID
{
    private readonly byte[] _bytes;

    private GUID(byte[] bytes) => _bytes = bytes;

    public static GUID V4(Random rng)
    {
        byte[] bytes = new byte[16];
        rng.NextBytes(bytes);
        bytes[6] = (byte)((bytes[6] & 0x0F) | 0x40); //version
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80); //variant
        return new GUID(bytes);
    }

    public static GUID V7(Random rng)
    {
        byte[] bytes = new byte[16];
        bytes[0] = (byte)(World.ShipTime >> 40);
        bytes[1] = (byte)(World.ShipTime >> 32);
        bytes[2] = (byte)(World.ShipTime >> 24);
        bytes[3] = (byte)(World.ShipTime >> 16);
        bytes[4] = (byte)(World.ShipTime >> 8);
        bytes[5] = (byte)World.ShipTime;
        rng.NextBytes(bytes.AsSpan(6));
        bytes[6] = (byte)((bytes[6] & 0x0F) | 0x70); //version
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80); //variant
        return new GUID(bytes);
    }

    /// <summary>
    /// Creates a GUIDv8 object
    /// </summary>
    /// <param name="rng">Instance of a Random class</param>
    /// <param name="high">4 byte value</param>
    /// <param name="low">2 byte value</param>
    /// <param name="family">Byte value</param>
    /// <param name="node">Byte value</param>
    public static GUID V8(Random rng, uint high, ushort low, byte family, byte node)
    {
        byte[] bytes = new byte[16];

        bytes[0] = (byte)(high >> 24);
        bytes[1] = (byte)(high >> 16);
        bytes[2] = (byte)(high >> 8);
        bytes[3] = (byte)high;

        bytes[4] = (byte)(low >> 8);
        bytes[5] = (byte)low;

        bytes[6] = (byte)((bytes[6] & 0x0F) | 0x80); //version
        bytes[7] = family;

        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80); //variant
        bytes[9] = node;

        rng.NextBytes(bytes.AsSpan(10));

        return new GUID(bytes);
    }

    public Int128 ToUInt() => System.Buffers.Binary.BinaryPrimitives.ReadInt128BigEndian(_bytes);
    public string ToHex() => $"{_bytes[0]:x2}{_bytes[1]:x2}{_bytes[2]:x2}{_bytes[3]:x2}-{_bytes[4]:x2}{_bytes[5]:x2}-{_bytes[6]:x2}{_bytes[7]:x2}-{_bytes[8]:x2}{_bytes[9]:x2}-{_bytes[10]:x2}{_bytes[11]:x2}{_bytes[12]:x2}{_bytes[13]:x2}{_bytes[14]:x2}{_bytes[15]:x2}";
    public byte[] ToByteArray() => (byte[])_bytes.Clone();
    public override string ToString() => ToHex();

    public int Version => (_bytes[6] >> 4) & 0x0F;
    public int Variant => _bytes[8] >> 6;

    public uint High => System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(_bytes);
    public ushort Low => System.Buffers.Binary.BinaryPrimitives.ReadUInt16BigEndian(_bytes.AsSpan(4));
    public byte Family => _bytes[7];
    public byte Node => _bytes[9];

    public string HighHex => $"{_bytes[0]:x2}{_bytes[1]:x2}{_bytes[2]:x2}{_bytes[3]:x2}";
    public string LowHex => $"{_bytes[4]:x2}{_bytes[5]:x2}";
    public string FamilyHex => $"{_bytes[7]:x2}";
    public string NodeHex => $"{_bytes[9]:x2}";
}