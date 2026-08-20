using System.IO;
using System.Peripherals;
using System.Text.RegularExpressions;
using System.Utils;
using BlackBox.Machine.World;
using Microsoft.Xna.Framework;
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
		new Pulsar(5.2f, 0.002f, 1200, 14000, 150, new Vector3(0,0,0));
		new Chronometer("Chronometer", "Timex", 0x11);
		
		// Execute Init.cs initialization as special init process
		Sandbox.SpawnInit(new Path("System/Init.cs").Read());
	}

	public static void Loop()
	{
		// IPC
		if (Process.Messages.TryDequeue(out var message))
		{
			// event registration
			if (message.Key == "RegisterKeyEvent" && message.SubProcess != null)
			{
				KeyEventListeners.Add(message.SubProcess, message.Value);
				Status.Throw(1, "Registered KeyEvent for "+message.SubProcess.GUID.ToString()+" for keys "+message.Value);
			}

			if (message.Key == "RegisterShellEvent" && message.SubProcess != null)
			{
				if (message.Value == "Clear") Terminal.ClearEventListeners.Add(message.SubProcess);
				else if (message.Value == "Write") Terminal.WriteEventListeners.Add(message.SubProcess);
				Status.Throw(1, "Registered ShellEvent for "+message.SubProcess.GUID.ToString()+" of type "+message.Value);
			}
			
			// broadcast events
			if (message.Key == "Broadcast" && message.SubProcess != null)
			{
				foreach (SubProcess process in Process.Processes)
				{
					Process.Send(process, message);
				}
				Status.Throw(1, "Sent broadcast IPC message from "+message.SubProcess.GUID.ToString()+" of type "+message.Value);
			}
		}

		// KeyEvent
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
		
		// device update
		foreach (Device device in Device.Devices)
		{
			device.Loop(deltaTime); //todo: consider fixed point decimal
		}

		deltaTime = (DateTime.UtcNow - lastUpdateTime).TotalSeconds;
		World.World.ShipTime += (DateTime.UtcNow - lastUpdateTime).Ticks;
		lastUpdateTime = DateTime.UtcNow;
		
		//todo:
		//check sandbox status and error/crash handling
		//update timed events
		//update peripherals/filesystem
		//update serial
		//Sandbox.Run() - now uses continuous execution loop
	}
}