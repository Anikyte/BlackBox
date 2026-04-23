using System.IO;
using System;

Status.Throw(1, Process.Self, "Init Loaded");

Path shell = new Path("System/Programs/Shell.cs");
Process.Spawn("Shell", shell);
