using System;
using System.IO;
using Microsoft.Xna.Framework.Input;
var me = Process.Self;

public List<string> backBuffer = new();
private string buffer = "";
private int cursorPos = 0;
private int lineIndex = 2;
private int historyIndex = -1;
private string savedBuffer = "";

// Key repeat state
private Dictionary<Keys, (DateTime pressed, DateTime lastFired)> keyRepeatState = new();
private const int RepeatDelay = 400; // ms before repeat starts
private const int RepeatRate = 50;   // ms between repeats

Status.Throw(1, me, "New shell: " + me.GUID.ToString());
Terminal.SetRow(1, $"{Process.Self.Name} [1]: New shell: " + me.GUID.ToString()); //temp

Keys[] prevKeys = [];

//register kernel events
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
	var now = DateTime.UtcNow;

	if (keys.Contains(Keys.F12)) break;

	// Update held key tracking - remove released keys
	foreach (var key in prevKeys.Where(k => !keys.Contains(k)))
		keyRepeatState.Remove(key);

	foreach (Keys key in keys)
	{
		bool isNewPress = !prevKeys.Contains(key);
		bool shouldFire = false;

		if (isNewPress)
		{
			keyRepeatState[key] = (now, now);
			shouldFire = true;
		}
		else if (key != Keys.Enter && keyRepeatState.TryGetValue(key, out var state))
		{
			var sincePressed = (now - state.pressed).TotalMilliseconds;
			var sinceFired = (now - state.lastFired).TotalMilliseconds;
			if (sincePressed > RepeatDelay && sinceFired > RepeatRate)
			{
				keyRepeatState[key] = (state.pressed, now);
				shouldFire = true;
			}
		}

		if (!shouldFire) continue;

		// History navigation
		if (key == Keys.Up)
		{
			if (historyIndex == -1 && backBuffer.Count > 0)
			{
				savedBuffer = buffer;
				historyIndex = backBuffer.Count - 1;
				buffer = backBuffer[historyIndex];
			}
			else if (historyIndex > 0)
			{
				historyIndex--;
				buffer = backBuffer[historyIndex];
			}
			cursorPos = buffer.Length;
			continue;
		}
		if (key == Keys.Down)
		{
			if (historyIndex >= 0)
			{
				historyIndex++;
				if (historyIndex >= backBuffer.Count)
				{
					historyIndex = -1;
					buffer = savedBuffer;
				}
				else buffer = backBuffer[historyIndex];
			}
			cursorPos = buffer.Length;
			continue;
		}
		if (key == Keys.Left) { if (cursorPos > 0) cursorPos--; continue; }
		if (key == Keys.Right) { if (cursorPos < buffer.Length) cursorPos++; continue; }
		if (key == Keys.Home) { cursorPos = 0; continue; }
		if (key == Keys.End) { cursorPos = buffer.Length; continue; }

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
		if (c != null) { buffer = buffer[..cursorPos] + c + buffer[cursorPos..]; cursorPos++; }
		else if (key == Keys.Back && cursorPos > 0) { buffer = buffer[..(cursorPos - 1)] + buffer[cursorPos..]; cursorPos--; }
		else if (key == Keys.Enter) { backBuffer.Add(buffer); historyIndex = -1; ExecuteBuffer(); cursorPos = 0; }
	}
	
	while (me.Messages.TryDequeue(out var message))
	{
		if (message.Key == "ClearEvent") lineIndex = 0;
		else if (message.Key == "WriteEvent")
		{
			RenderWrapped(lineIndex, message.Value);
			lineIndex = (lineIndex + (message.Value.Length + Terminal.Width - 1) / Terminal.Width) % Terminal.Height;
		}
	}

	prevKeys = keys;
	RenderWrapped(lineIndex, "> " + buffer[..cursorPos] + "|" + buffer[cursorPos..]);
}

Terminal.SetRow(lineIndex, "Shell complete");

void RenderWrapped(int startRow, string text)
{
	int w = Terminal.Width;
	for (int i = 0; i * w < text.Length; i++)
		Terminal.SetRow((startRow + i) % Terminal.Height, text.Substring(i * w, Math.Min(w, text.Length - i * w)));
}

void ExecuteBuffer()
{
	string prompt = "> " + buffer;
	RenderWrapped(lineIndex, prompt);
	lineIndex = (lineIndex + (prompt.Length + Terminal.Width - 1) / Terminal.Width) % Terminal.Height;
	var result = Process.Execute(buffer);
	RenderWrapped(lineIndex, result.Item2);
	lineIndex = (lineIndex + (result.Item2.Length + Terminal.Width - 1) / Terminal.Width) % Terminal.Height;
	buffer = "";
}