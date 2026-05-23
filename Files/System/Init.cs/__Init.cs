using System.IO;
using System;
using System.Peripherals;
using System.Threading;

Status.Throw(1, Process.Self, "Init Loaded");
Terminal.SetRow(0, $"{Process.Self.Name} [1]: Init Loaded"); //temp

Process.Spawn("Shell", "new Shell().Main();");

// Tests here

// End tests


//todo: side window managers

//Image test
