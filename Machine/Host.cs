using System.IO;
using System.Peripherals;
using System.Text.RegularExpressions;
using System.Utils;
using DateTime = System.DateTime;

namespace BlackBox.Machine;

public static class Host
{
	private static DateTime lastUpdateTime = DateTime.UtcNow;
	private static double deltaTime;

	public static Random Random = new(1569285326);
	
	private static Dictionary<SubProcess, string> KeyEventListeners = new();
	
	static Host()
	{
		Device.Initialize();
		Reactor.Initialize(Random, 5, 12, 4, 12);
		new Chronometer("Chronometer", "Timex", 0x11);
		
		// Execute Init.cs initialization as special init process
		Sandbox.SpawnInit(new Path("System/Programs/Init.cs").Read());
	}

	public static void Loop()
	{
		if (Process.Messages.TryDequeue(out var message))
		{
			if (message.Key == "RegisterKeyEvent" && message.SubProcess != null)
			{
				KeyEventListeners.Add(message.SubProcess, message.Value);
				Status.Throw(1, "Registered KeyEvent for "+message.SubProcess.GUID.ToString()+" for keys "+message.Value);
			}

			if (message.Key == "RegisterShellEvent" && message.Value == "Clear" && message.SubProcess != null)
			{
				Terminal.ClearEventListeners.Add(message.SubProcess);
				Status.Throw(1, "Registered ShellEvent for "+message.SubProcess.GUID.ToString()+" of type "+message.Value);
			}
		}

		int key = Input.GetCharPressed();
		if (key > 0)
		{
			char c = (char)key;
			foreach (KeyValuePair<SubProcess, string> kvp in KeyEventListeners)
			{
				if (Regex.IsMatch(c.ToString(), kvp.Value))
					Process.Send(kvp.Key, new Message("KeyEvent", c.ToString()));
			}
		}
		
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