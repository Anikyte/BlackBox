using System.IO;
using System;
using System.Peripherals;

Status.Throw(1, Process.Self, "Init Loaded");
Terminal.SetRow(0, $"{Process.Self.Name} [1]: Init Loaded"); //temp

// Tests here
string b64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
string img = new Path("User/testcard.b64").Read();
for (int i = 0; i < 128 * 128; i++)
{
	int x = i % 128, y = i / 128;
	byte r = (byte)(b64.IndexOf(img[i * 3]) << 2);
	byte g = (byte)(b64.IndexOf(img[i * 3 + 1]) << 2);
	byte bl = (byte)(b64.IndexOf(img[i * 3 + 2]) << 2);
	Bitmap.Set(x, y, r, g, bl);
}
// End tests

Path shell = new Path("System/Programs/Shell.cs");
Process.Spawn("Shell", shell);
//todo: side window managers

//Image test
