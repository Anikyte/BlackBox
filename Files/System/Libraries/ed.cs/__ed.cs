using System;
using System.IO;

public class ED
{
	private string[] lines;
	private Path path;

	private string[] insert(string[] array, int line, string str)
	{
		string[] result = new string[array.Length + 1];
		for (int i = 0; i < line; i++) result[i] = array[i];
		result[line] = str;
		for (int i = line; i < array.Length; i++) result[i + 1] = array[i];
		return result;
	}

	private string[] delete(string[] array, int line)
	{
		string[] result = new string[array.Length - 1];
		for (int i = 0; i < line; i++) result[i] = array[i];
		for (int i = line + 1; i < array.Length; i++) result[i - 1] = array[i];
		return result;
	}

	private int chars(string[] array)
	{
		int result = 0;
		foreach (string line in array) { result += line.Length; }
		return result;
	}
	
	//constructor
	public ED(Path p)
	{
		this.path = p;
		lines = path.Read();
	}

	public ED(string p)
	{
		this.path = new Path(p);
		lines = path.Read();
	}
	
	//append
	public void a(int line, string s) => lines = insert(lines, line+1, s);

	//insert
	public void i(int line, string s) => lines = insert(lines, line, s);
	
	//change
	public void c(int line, string s) => lines[line] = s;
	
	//delete
	public void d(int line) => lines = delete(lines, line);
	
	//save
	public void w()
	{
		path.Write(lines);
		Terminal.WriteLine($"Wrote {chars(lines)} chars to {path.ToString()}");
	}

	//print
	public void p(int line)
	{
		if (line == -1)
		{
			for (int i = 0; i < lines.Length; i++) Terminal.WriteLine(i + ": " + lines[i]);
		}
		else if (line == -2) //easter egg
		{
			Terminal.WriteLine("ed is the standard text editor!");
		}
		else if (line < -2) { Terminal.WriteLine("?"); }
		else { Terminal.WriteLine(lines[line]); }
	}

	public void p()
	{
		p(-1);
	}
}