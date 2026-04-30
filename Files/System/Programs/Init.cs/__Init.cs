using System.IO;
using System;

Status.Throw(1, Process.Self, "Init Loaded");
Terminal.SetRow(0, $"{Process.Self.Name} [1]: Init Loaded"); //temp

// Tests here

// End tests

Path shell = new Path("System/Programs/Shell.cs");
Process.Spawn("Shell", shell);
//todo: side window managers