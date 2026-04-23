using System.Collections.Concurrent;
using BlackBox.Machine;
using System.Utils;

namespace System;

public static class Process
{
	//kernal message buffer
	public static ConcurrentQueue<Message> Messages = new();
	
	public static List<SubProcess> Processes = new();
	public static SubProcess? Self => SubProcess.Current;

	public static SubProcess Spawn(string name, string code) => Sandbox.Spawn(name, code);
	public static SubProcess Spawn(string name, Path path) => Sandbox.Spawn(name, new Path(path).Read());
	internal static SubProcess SpawnInit(string code) => Sandbox.SpawnInit(code);
	internal static SubProcess SpawnInit(Path path) => Sandbox.SpawnInit(new Path(path).Read());
	public static SubProcess? Get(GUID guid) => Processes.Find(p => p.GUID == guid);
	public static SubProcess? Get(string guid) => Processes.Find(p => p.GUID.ToString() == guid);

	public static void Send(SubProcess process, Message message) => process.Messages.Enqueue(message);
	public static void Send(Message message) => Process.Messages.Enqueue(message); //kernal messages
	
	public static List<int> List() => null;
	public static void Await(int pid)
	{
		//await a pid's completion	
	}
}

public class Message(string key, string value, SubProcess? self = null, object? payload = null)
{
	public string Key = key;
	public string Value = value;
	public object? Payload = payload;
	public SubProcess? SubProcess = self;
	public readonly DateTime Timestamp = DateTime.Now;
	//guid?
}