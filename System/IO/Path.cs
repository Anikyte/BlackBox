using System.Text;
using SysIO = global::System.IO;

namespace System.IO;

//virtual filesystem file object
//ALL filesystem calls accessible by user must go through here

//files on disk are actually directories structured as such

// System/
// -- System/__System
// -- Files/
// -- -- Files/__Files
// -- -- Files/Doc1.txt
// -- -- -- Files/Doc1.txt/__Doc1.txt

//where all files are actually directories
//and the real files prefixed with __ contain the data of a file

//to the user they only see all paths being both a file and a directory

public class Path
{
	private static readonly string RootDirectory = "./Files";

	// Translates user path to actual disk path
	// e.g., /System/Files/data.dat -> ./Files/System/Files/data.dat/
	private static string GetHostspacePath(string userspacePath)
	{
		string normalized = userspacePath.TrimStart('/').TrimEnd('/').Replace('\\', '/');
		return RootDirectory + "/" + normalized + "/";
	}

	// Gets the actual data file path for a given user path
	// e.g., /System/Files/data.dat -> ./Files/System/Files/data.dat/__data.dat
	private static string GetDataFilePath(string userspacePath)
	{
		string normalized = userspacePath.TrimStart('/').TrimEnd('/').Replace('\\', '/');
		string fileName = normalized.Contains('/') ? normalized.Substring(normalized.LastIndexOf('/') + 1) : normalized;
		return RootDirectory + "/" + normalized + "/__" + fileName;
	}

	private string userspacePath;

	public Path(string path)
	{
		userspacePath = path;
	}

	public Path(Path path, string append)
	{
		userspacePath = path.userspacePath.TrimEnd('/') + "/" + append.TrimStart('/');
	}

	public static implicit operator string(Path path) => path.ToString();

	public Data Read()
	{
		string dataFilePath = GetDataFilePath(userspacePath);

		if (!SysIO.File.Exists(dataFilePath))
			return new Data(Array.Empty<byte>());

		byte[] bytes = SysIO.File.ReadAllBytes(dataFilePath);
		return new Data(bytes);
	}

	public void Write(string text) => Write(Encoding.UTF8.GetBytes(text));
	public void Write(string[] text) => Write(Encoding.UTF8.GetBytes(string.Join('\n', text)));
	public void Write(char[] chars) => Write(Encoding.UTF8.GetBytes(chars));
	public void Write(byte[] bytes)
	{
		SysIO.Directory.CreateDirectory(GetHostspacePath(userspacePath));
		string dataFilePath = GetDataFilePath(userspacePath);
		SysIO.File.WriteAllBytes(dataFilePath, bytes);
	}

	public Path[] List()
	{
		string hostPath = GetHostspacePath(userspacePath);

		if (!SysIO.Directory.Exists(hostPath))
			return Array.Empty<Path>();

		string[] subdirs = SysIO.Directory.GetDirectories(hostPath);
		Path[] paths = new Path[subdirs.Length];

		for (int i = 0; i < subdirs.Length; i++)
		{
			string trimmed = subdirs[i].TrimEnd('/', '\\');
			string dirName = trimmed.Contains('/') ? trimmed.Substring(trimmed.LastIndexOf('/') + 1) :
			                 trimmed.Contains('\\') ? trimmed.Substring(trimmed.LastIndexOf('\\') + 1) : trimmed;
			paths[i] = new Path(this, dirName);
		}

		return paths.ToArray();
	}

	public void Move(Path path) => SysIO.Directory.Move(GetHostspacePath(userspacePath), GetHostspacePath(path.userspacePath));

	// public void Copy(Path path)
	// {
	// 	SysIO.Directory.CreateDirectory(GetHostspacePath(path.userspacePath));
	//
	// 	foreach (string file in SysIO.Directory.GetFiles(GetHostspacePath(userspacePath)))
	// 	{
	// 		string destFile = SysIO.Path.Combine(GetHostspacePath(path.userspacePath), SysIO.Path.GetFileName(file));
	// 		SysIO.File.Copy(file, destFile, true);
	// 	}
	//
	// 	foreach (string dir in SysIO.Directory.GetDirectories(GetHostspacePath(userspacePath)))
	// 	{
	// 		string destSubDir = SysIO.Path.Combine(GetHostspacePath(path.userspacePath), SysIO.Path.GetFileName(dir));
	// 		Copy(dir, destSubDir);
	// 	}
	// }

	public new string ToString() => userspacePath;
}

public struct Data
{
	private byte[] data;

	public Data(byte[] data)
	{
		this.data = data;
	}
	
	public static implicit operator byte[](Data data) => data.data;
	public static implicit operator char[](Data data) => Encoding.UTF8.GetString(data).ToCharArray();
	public static implicit operator string[](Data data) => Encoding.UTF8.GetString(data).Split('\n'); //delimited by newlines
	public static implicit operator string(Data data) => Encoding.UTF8.GetString(data); //one long string
	
	//things like Data.Lines() => Data(substr lines) etc.
}