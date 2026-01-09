using System.Utils;
using BlackBox.Machine;

namespace System.Peripherals;

public class Device
{
    public static List<Device> Devices { get; } = new();
    private static Dictionary<byte, byte> NodeCounters { get; } = new();

    public static void Initialize() { }
    
    public readonly GUID GUID;
    
    //human readable metadata
    public string Name;
    public string Manufacturer;

    internal Device(string name, string manufacturer, byte type)
    {
        Devices.Add(this);

        Name = name;
        Manufacturer = manufacturer;
        
        byte node = NodeCounters.TryGetValue(type, out byte count) ? count : (byte)0;
        NodeCounters[type] = (byte)(node + 1);

        GUID = GUID.V8(Host.Random, 0, 0, type, node); //todo: device high/low values (potentially name+manufacturer hash?)
    }

    internal virtual void Loop(double deltaTime) {}

    public byte Slot => GUID.Node;
    public byte Type => GUID.Family;
    public int ID => BitConverter.ToUInt16([Type, Slot], 0);
}