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

	public static SubProcess Spawn(string code) => Sandbox.Spawn(code);
	public static SubProcess Spawn(Path path) => Sandbox.Spawn(new Path(path).Read());
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