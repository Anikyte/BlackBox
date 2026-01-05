using System.Peripherals;
using BlackBox.Machine.Peripherals;
using DateTime = System.DateTime;

namespace BlackBox.Machine;

public static class Host
{
	private static DateTime lastUpdateTime = DateTime.UtcNow;
	private static long deltaTime;
	
	static Host()
	{
		Shell.ShowPrompt();
		Reactor.Initialize(new Random(1569285326), 5, 12, 4, 12);
		Console.WriteLine(GUID.GUIDv4(new Random()));
		Console.WriteLine(GUID.GUIDv7(new Random()));
	}

	public static void Loop()
	{
		Shell.ProcessInput();
		Reactor.Loop(deltaTime);

		deltaTime = (DateTime.UtcNow - lastUpdateTime).Milliseconds;
		lastUpdateTime = DateTime.UtcNow;

		World.ShipTime += deltaTime;
		
		//todo:
		//check sandbox status and error/crash handling
		//update timed events
		//update peripherals/filesystem
		//update serial
		//Sandbox.Run() - now uses continuous execution loop
	}
}