using System.Peripherals;

public static class Info
{
	public static class Reactor
	{
		public static void GUIDs()
		{
			Panel.Clear();
			int row = 0;
			void List(string header, IEnumerable<Device> devices)
			{
				Panel.SetRow(row++, header, 0, (0,0,0),(255,255,255));
				int i = 0;
				foreach (Device device in devices)
				{
					Panel.SetRow(row, device.GUID.ToString(), i % 2 * (Panel.Width / 2));
					row += i++ % 2; // advance only after the second GUID on a row
				}
				row += i % 2; // account for a half-filled final row
			}

			List("Fuel Rods:", System.Peripherals.Reactor.FuelRods);
			List("Control Rods:", System.Peripherals.Reactor.ControlRods);
			List("Pumps:", System.Peripherals.Reactor.Pumps);
			List("RTGs:", System.Peripherals.Reactor.RTGs);
		}
	}
}