using BlackBox.Machine;
using System.Utils;

namespace System;

public static class Process
{
	public static List<SubProcess> Processes = new();
	public static SubProcess? Self => SubProcess.Current;

	public static SubProcess Spawn(string code) => Sandbox.Spawn(code);
	public static SubProcess Spawn(Path path) => Sandbox.Spawn(new Path(path).Read());
	public static SubProcess? Get(GUID guid) => Processes.Find(p => p.GUID == guid);
	public static SubProcess? Get(string guid) => Processes.Find(p => p.GUID.ToString() == guid);

	public static void Send(SubProcess process, Message message) => process.Messages.Enqueue(message);
	
	public static List<int> List() => null;
	public static void Await(int pid)
	{
		//await a pid's completion	
	}
}

public class Message
{
	public string Key;
	public string Value;
	public object? Payload;
	public readonly DateTime Timestamp;
	//guid?
	
	public Message(string key, string value, object? payload)
	{
		Key = key;
		Value = value;
		Payload = payload;
		Timestamp = DateTime.Now;
	}
}