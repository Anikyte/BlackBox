using Window = global::BlackBox.Window;

namespace System;

/// <summary>
/// Terminal userspace API for writing to the terminal window
/// </summary>
public static class Terminal
{
	public static int Height = Window.Terminal.Height;
	public static int Width = Window.Terminal.Width;

	public static (byte r, byte g, byte b) ColorFg = Window.Terminal.CurrentFg;
	public static (byte r, byte g, byte b) ColorBg = Window.Terminal.CurrentBg;
	
	/// <summary>
	/// Write text to the terminal window
	/// </summary>
	public static void Write(string text)
	{
		Window.Terminal.Write(text);
	}

	/// <summary>
	/// Write line to the terminal window
	/// </summary>
	public static void WriteLine(string text)
	{
		Window.Terminal.Write(text + "\n");
	}

	/// <summary>
	/// Clear the terminal screen
	/// </summary>
	public static void Clear()
	{
		Window.Terminal.Clear();
	}

	/// <summary>
	/// Set cursor position in the terminal
	/// </summary>
	public static void SetCursorPosition(int x, int y)
	{
		global::BlackBox.Terminal terminal = Window.Terminal;
		terminal.CursorX = Math.Clamp(x, 0, terminal.Width - 1);
		terminal.CursorY = Math.Clamp(y, 0, terminal.Height - 1);
	}

	/// <summary>
	/// Reset colors to default
	/// </summary>
	public static void ResetColors()
	{
		Window.Terminal.CurrentFg = Window.Terminal.DefaultFg;
		Window.Terminal.CurrentBg = Window.Terminal.DefaultBg;
	}

	/// <summary>
	/// Get character at position
	/// </summary>
	public static char GetChar(int x, int y) => Window.Terminal.GetChar(x, y);
}
