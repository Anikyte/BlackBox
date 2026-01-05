using System.Utils;
using BlackBox.Machine;

namespace System.Peripherals;

public class Device
{
    public static List<Device> Devices { get; } = new();

    public static void Initialize()
    {
        
    }
    
    public readonly GUID GUID;
    
    //human readable metadata
    public string Name;
    public string Manufacturer;

    internal Device(string name, string manufacturer, byte type)
    {
        Devices.Add(this);
        
        Name = name;
        Manufacturer = manufacturer;
        byte node = (byte)(Devices.Count - 1);
        
        GUID = GUID.V8(Host.Random, 0, 0, type, node);
    }

    public int Slot => GUID.Node;
    public int Type => GUID.Family;
}