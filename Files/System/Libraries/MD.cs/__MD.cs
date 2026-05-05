using System;
using System.IO;

public static class MD
{
	public static void Read(Path path)
	{
		Terminal.Clear();
		string[] lines = path.Read();
		foreach (string line in lines)
		{
			Terminal.WriteLine(line);	
		}
	}

	public static void Read(string path)
	{
		Read(new Path(path));
	}
}