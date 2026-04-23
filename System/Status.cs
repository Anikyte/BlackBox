using System.Utils;
using BlackBox.Machine;

namespace System;

public class Status
{
	public static Path Log = new Path("System/Logs/"+DateTime.Now.ToString("yyyy-MM-dd:HH:mm:ss"));
	public static bool DEBUG = true;

	public SubProcess? Process;
	public int Level;
	public string Value;

	public Status(int level, SubProcess process, string value)
	{
		Level = level;
		Value = value;
		Process = process;
	}

	internal Status(int level, string value) //kernel use only
	{
		Level = level;
		Value = value;
		Process = null;
	}

	public static void Throw(int level, SubProcess process, string value)
	{
		string str = $"{process.Name}({process.GUID}) [{level}]: {value}\n";
		Log.Write(str);
		Console.Write(str);
		if (DEBUG) Terminal.Write(str);
	}
	
	public static void Throw(int level, GUID guid, string name, string value)
	{
		string str = $"{name}({guid}) [{level}]: {value}\n";
		Log.Write(str);
		Console.Write(str);
		if (DEBUG) Terminal.Write(str);
	}

	internal static void Throw(int level, string value)
	{
		string str = $"KERNEL [{level}]: {value}\n";
		Log.Write(str);
		Console.Write(str);
		if (DEBUG) Terminal.Write(str);
	}
}