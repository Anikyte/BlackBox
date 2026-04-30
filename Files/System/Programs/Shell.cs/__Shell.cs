using System;
using System.IO;
using Microsoft.Xna.Framework.Input;
var me = Process.Self;

public List<string> backBuffer = new();
private string buffer = "";

Status.Throw(1, me, "New shell: " + me.GUID.ToString());

Keys[] prevKeys = [];

while (true)
{
	var keys = Input.Pressed;
	bool SHIFT = keys.Contains(Keys.LeftShift) || keys.Contains(Keys.RightShift);

	if (keys.Contains(Keys.F12)) break;

	foreach (Keys key in keys)
	{
		if (prevKeys.Contains(key)) continue;
		if (key >= Keys.A && key <= Keys.Z) { buffer += SHIFT ? key.ToString() : key.ToString().ToLower(); }
		else if (key == Keys.Space) { buffer += " "; }
		else if (key == Keys.Enter)
		{
			backBuffer.Append(buffer);
			buffer = "";
		}
	}

	prevKeys = keys;
	Terminal.SetRow(10, buffer);
}