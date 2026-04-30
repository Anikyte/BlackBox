using System.IO;
using System;

Status.Throw(1, Process.Self, "Init Loaded");

// Tests here

// End tests

Path shell = new Path("System/Programs/Shell.cs");
Process.Spawn("Shell", shell);
//todo: side window managers