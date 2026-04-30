using System;
using System.IO;
using Microsoft.Xna.Framework.Input;
var me = Process.Self;

public List<string> backBuffer = new();
private string buffer = "";
private int lineIndex = 5;

Status.Throw(1, me, "New shell: " + me.GUID.ToString());

Keys[] prevKeys = [];

Process.Send(new Message("RegisterShellEvent", "Clear", me));
//todo: implement
// bool success = false;
// //wait 10ms
// for (int i = 0; i < 8; i++)
// {
// 	if (me.Messages.TryDequeue(out var message))
// 	{
// 		//if message == confirmation response then success = true
// 	}
// 	else
// 	{
// 		//wait 10ms
// 		//Process.Send(new Message("RegisterShellEvent", "Clear", me));
// 	}
// }
//
// if (success)
// {
// 	continue;
// }
// else
// {
// 	throw new Exception("Shell failed to establish IPC link with kernel!");
// }

while (true)
{
	var keys = Input.Pressed;
	bool SHIFT = keys.Contains(Keys.LeftShift) || keys.Contains(Keys.RightShift);

	if (keys.Contains(Keys.F12)) break;

	foreach (Keys key in keys)
	{
		if (prevKeys.Contains(key)) continue;
		char? c = key switch
		{ //todo: make locale independent
			>= Keys.A and <= Keys.Z => SHIFT ? key.ToString()[0] : char.ToLower(key.ToString()[0]),
			>= Keys.D0 and <= Keys.D9 => SHIFT ? ")!@#$%^&*("[key - Keys.D0] : (char)('0' + key - Keys.D0),
			Keys.Space => ' ',
			Keys.OemPeriod => SHIFT ? '>' : '.',
			Keys.OemComma => SHIFT ? '<' : ',',
			Keys.OemSemicolon => SHIFT ? ':' : ';',
			Keys.OemQuotes => SHIFT ? '"' : '\'',
			Keys.OemOpenBrackets => SHIFT ? '{' : '[',
			Keys.OemCloseBrackets => SHIFT ? '}' : ']',
			Keys.OemMinus => SHIFT ? '_' : '-',
			Keys.OemPlus => SHIFT ? '+' : '=',
			Keys.OemPipe => SHIFT ? '|' : '\\',
			Keys.OemTilde => SHIFT ? '~' : '`',
			Keys.OemQuestion => SHIFT ? '?' : '/',
			Keys.Tab => '\t',
			_ => null
		};
		if (c != null) buffer += c;
		else if (key == Keys.Back && buffer.Length > 0) buffer = buffer[..^1];
		else if (key == Keys.Enter) { backBuffer.Append(buffer); ExecuteBuffer(); }
	}
	
	if (me.Messages.TryDequeue(out var message))
	{
		if (message.Key == "ClearEvent")
		{
			lineIndex = 0;
		}
	}

	prevKeys = keys;
	Terminal.SetRow(lineIndex, "> "+buffer+"_");
}

Terminal.SetRow(lineIndex, "Shell complete");

void ExecuteBuffer()
{
	Terminal.SetRow(lineIndex, "> "+buffer);
	var result = Process.Execute(buffer);
	lineIndex++;
	Terminal.SetRow(lineIndex, result.Item2);
	lineIndex++;
	buffer = "";
}