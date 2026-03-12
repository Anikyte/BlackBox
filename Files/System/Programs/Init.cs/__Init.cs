using System.IO;

Terminal.WriteLine("Init Loaded");

Path shell = new Path("System/Programs/Shell.cs");
Process.Spawn(shell);