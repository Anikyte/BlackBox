namespace BlackBox.System;

//manages the low level "serial" console
//mainly for debugging

public static class Serial
{
	public static string ConsoleBuffer = "";
	
	public static void Write(string s)
	{
		Console.Write(s);
	}

	public static string Read(string s)
	{
		return ConsoleBuffer;
	}
}