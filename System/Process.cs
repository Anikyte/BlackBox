using System.Collections.Concurrent;
using BlackBox.Machine;

namespace System;

//process manager functions

public static class Process
{
	public static ConcurrentDictionary<int, SubProcess> Processes = Sandbox.Processes;
	
	public static int Spawn(string code)
	{
		return Sandbox.Spawn(code); //todo: return pid (or skip the pid crap and return SubProcess object)
	}

	public static int Spawn(Path path)
	{
		return Sandbox.Spawn(new Path(path).Read());
	}

	public static int Status(int pid)
	{
		return 0;
	}

	public static int Kill(int pid)
	{
		return Sandbox.Kill(pid) ? 0 : 255;
	}

	public static List<int> List()
	{
		return null;
	}

	public static void Await(int pid)
	{
		//await a pid's completion	
	}

	public static void Send(int pid, string message)
	{
		Send((SubProcess)pid, message);
	}

	public static void Send(SubProcess process, string message)
	{
		process.MessageQueue.Add(message);
	}
}