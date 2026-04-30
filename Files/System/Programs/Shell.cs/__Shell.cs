using System;
using System.IO;
using Microsoft.Xna.Framework.Input;
var me = Process.Self;

public List<string> backBuffer = new();
private string buffer = "";
private int lineIndex = 2;

Status.Throw(1, me, "New shell: " + me.GUID.ToString());
Terminal.SetRow(1, $"{Process.Self.Name} [1]: New shell: " + me.GUID.ToString()); //temp

Keys[] prevKeys = [];

Process.Send(new Message("RegisterShellEvent", "Clear", me));
Process.Send(new Message("RegisterShellEvent", "Write", me));
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
	
	while (me.Messages.TryDequeue(out var message))
	{
		if (message.Key == "ClearEvent") lineIndex = 0;
		else if (message.Key == "WriteEvent")
		{
			RenderWrapped(lineIndex, message.Value);
			lineIndex += (message.Value.Length + Terminal.Width - 1) / Terminal.Width;
		}
	}

	prevKeys = keys;
	RenderWrapped(lineIndex, "> " + buffer + "_");
}

Terminal.SetRow(lineIndex, "Shell complete");

void RenderWrapped(int startRow, string text)
{
	int w = Terminal.Width;
	for (int i = 0; i * w < text.Length; i++)
		Terminal.SetRow(startRow + i, text.Substring(i * w, Math.Min(w, text.Length - i * w)));
}

void ExecuteBuffer()
{
	string prompt = "> " + buffer;
	RenderWrapped(lineIndex, prompt);
	lineIndex += (prompt.Length + Terminal.Width - 1) / Terminal.Width;
	var result = Process.Execute(buffer);
	RenderWrapped(lineIndex, result.Item2);
	lineIndex += (result.Item2.Length + Terminal.Width - 1) / Terminal.Width;
	buffer = "";
}