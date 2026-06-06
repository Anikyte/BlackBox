using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

//command buffer string
//output buffer List<String>

//seperate buffers from rendering

//first, process input and output
//then rendering:
//first, render output buffer
//then, render command buffer
//if command buffer length > terminal width, wrap to next line


public class Shell
{
	private List<string> backBuffer = new();
	private string commandBuffer = "";
	private string savedBuffer = "";
	private List<string> outputBuffer = new();
	private int historyIndex = -1;
	private List<(string text, (byte r, byte g, byte b)? fg, (byte r, byte g, byte b)? bg)> renderBuffer = new();
	
	private Dictionary<Keys, (DateTime pressed, DateTime lastFired)> keyRepeatState = new();
	private const int RepeatDelay = 400;
	private const int RepeatRate = 50;
	private Keys[] prevKeys = [];

	private int cursorPos = 0;
	private int lineIndex = 0;

	public void Main()
	{
		var me = Process.Self;
		Output($"{me.Name} [1]: New shell: {me.GUID}");
		Process.Send(new Message("RegisterShellEvent", "Clear", me));
		Process.Send(new Message("RegisterShellEvent", "Write", me));
		
		while (true)
		{
			// Process
			ProcessInput();
			
			//Handle IPC
			if (me.Messages.TryDequeue(out var message))
			{
				if (message.Key == "ClearEvent")
				{
					outputBuffer.Clear();
					commandBuffer = "";	
				} else if (message.Key == "WriteEvent")
				{
					Output(message.Value);
				}
			}

			// Render
			Render();
		}
		Terminal.SetRow(lineIndex, "Shell complete");
	}

	private void ProcessInput()
	{
		var keys = Input.Pressed;
		bool SHIFT = keys.Contains(Keys.LeftShift) || keys.Contains(Keys.RightShift);
		var now = DateTime.UtcNow;

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

			if (key == Keys.Up)
			{
			 	if (historyIndex == -1 && backBuffer.Count > 0)
			 	{
			 		savedBuffer = commandBuffer;
			 		historyIndex = backBuffer.Count - 1;
			 		commandBuffer = backBuffer[historyIndex];
			 	}
			 	else if (historyIndex > 0)
			 	{
			 		historyIndex--;
			 		commandBuffer = backBuffer[historyIndex];
			 	}
			 	cursorPos = commandBuffer.Length;
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
			 			commandBuffer = savedBuffer;
			 		}
			 		else commandBuffer = backBuffer[historyIndex];
			 	}
			 	cursorPos = commandBuffer.Length;
				continue;
			}
			if (key == Keys.Left) { if (cursorPos > 0) cursorPos--; continue; }
			if (key == Keys.Right) { if (cursorPos < commandBuffer.Length) cursorPos++; continue; }
			if (key == Keys.Home) { cursorPos = 0; continue; }
			if (key == Keys.End) { cursorPos = commandBuffer.Length; continue; }

			char? c = key switch
			{
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
			if (c != null) { commandBuffer = commandBuffer[..cursorPos] + c + commandBuffer[cursorPos..]; cursorPos++; }
			else if (key == Keys.Back && cursorPos > 0) { commandBuffer = commandBuffer[..(cursorPos - 1)] + commandBuffer[cursorPos..]; cursorPos--; }
			else if (key == Keys.Enter) { backBuffer.Add(commandBuffer); historyIndex = -1; ExecuteBuffer(); cursorPos = 0; }
		}

		prevKeys = keys;
	}

	private void Render()
	{
		renderBuffer.Clear();
		ComposeOutputBuffer();
		ComposeCommandBuffer();
		for (int i = 0; i < renderBuffer.Count; i++)
			Terminal.SetRow(i, renderBuffer[i].text, fg: renderBuffer[i].fg, bg: renderBuffer[i].bg);
	}

	public void Output(string text) => outputBuffer.Insert(0,text);

	private void SetRow(int row, string text, (byte r, byte g, byte b)? fg = null, (byte r, byte g, byte b)? bg = null)
	{
		while (renderBuffer.Count <= row) renderBuffer.Add(("", null, null));
		renderBuffer[row] = (text, fg, bg);
	}

	private void ComposeOutputBuffer()
	{
		for (int i = 0; i < outputBuffer.Count; i++)
			SetRow(i+1, outputBuffer[i]);
	}

	private void ComposeCommandBuffer()
	{
		string text = "";
		if (cursorPos == commandBuffer.Length)
		{
			text = "> " + commandBuffer + "["; 
		}
		else
		{
			text = "> " + commandBuffer[..cursorPos] + "[" + commandBuffer[(cursorPos+1)..];
		}

		int w = Terminal.Width;
		for (int i = 0; i * w < text.Length; i++)
			SetRow(i, text.Substring(i * w, Math.Min(w, text.Length - i * w)), fg: (0, 0, 0), bg: (255, 255, 255));
	}

	private void ExecuteBuffer()
	{
		if (commandBuffer.Length > 0)
		{
			var result = Process.Execute(commandBuffer);
			if (result.Item2.Length > 0)
			{
				outputBuffer.Insert(0, result.Item2);
			}
			commandBuffer = "";
		}
	}
}