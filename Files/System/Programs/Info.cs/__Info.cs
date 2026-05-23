using System.Peripherals;

public static class Info
{
	public static void Reactor()
	{
		Terminal.Clear();
		Terminal.WriteLine("Fuel Rods:");
		foreach (Reactor.FuelRod rod in System.Peripherals.Reactor.FuelRods) { Terminal.WriteLine(rod.GUID.ToString()); }
		Terminal.WriteLine("Control Rods:");
		foreach (Reactor.ControlRod rod in System.Peripherals.Reactor.ControlRods) { Terminal.WriteLine(rod.GUID.ToString()); }
		Terminal.WriteLine("Pumps:");
		foreach (Reactor.Pump pump in System.Peripherals.Reactor.Pumps) { Terminal.WriteLine(pump.GUID.ToString()); }
		Terminal.WriteLine("RTGs:");
		foreach (Reactor.RTG rtg in System.Peripherals.Reactor.RTGs) { Terminal.WriteLine(rtg.GUID.ToString()); }
	}
}