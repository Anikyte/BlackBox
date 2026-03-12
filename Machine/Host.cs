using System.Peripherals;
using System.Utils;
using DateTime = System.DateTime;

namespace BlackBox.Machine;

public static class Host
{
	private static DateTime lastUpdateTime = DateTime.UtcNow;
	private static double deltaTime;

	public static Random Random = new(1569285326);
	
	static Host()
	{
		Device.Initialize();
		Reactor.Initialize(Random, 5, 12, 4, 12);
		new Chronometer("Chronometer", "Timex", 0x11);
		
		// Execute ShellRC.cs initialization
		var result = Sandbox.Execute(new Path("System/Programs/Init.cs").Read());
		if (result.Success)
		{
			if (result.ReturnValue != null)
			{
				System.Terminal.Write($"=> {result.ReturnValue}\n");
			}
		}
		else
		{
			System.Terminal.Write($"ShellRC Error: {result.ErrorMessage}\n");
		}
		// Console.WriteLine(GUID.V4(Random));
		// Console.WriteLine(GUID.V7(Random));
		// Console.WriteLine(GUID.V8(Random, 0, 0, 0, 1));
		
		Shell.ShowPrompt(); //currently a race condition but will be fixed later when repl is entirely programspace
	}

	public static void Loop()
	{
		Shell.ProcessInput();
		foreach (Device device in Device.Devices)
		{
			device.Loop(deltaTime); //todo: consider fixed point decimal
		}

		deltaTime = (DateTime.UtcNow - lastUpdateTime).TotalSeconds;
		World.ShipTime += (DateTime.UtcNow - lastUpdateTime).Ticks;
		lastUpdateTime = DateTime.UtcNow;
		
		//todo:
		//check sandbox status and error/crash handling
		//update timed events
		//update peripherals/filesystem
		//update serial
		//Sandbox.Run() - now uses continuous execution loop
	}
}