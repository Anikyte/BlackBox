namespace BlackBox.System;

/// <summary>
/// Terminal userspace API for writing to the terminal window
/// </summary>
public static class Terminal
{
	/// <summary>
	/// Write text to the terminal window
	/// </summary>
	public static void Write(string text)
	{
		Window.Write(text);
	}

	/// <summary>
	/// Write line to the terminal window
	/// </summary>
	public static void WriteLine(string text)
	{
		Window.Write(text + "\n");
	}

	/// <summary>
	/// Clear the terminal screen
	/// </summary>
	public static void Clear()
	{
		BlackBox.Terminal? terminal = Window.GetTerminal();
		terminal?.Clear();
	}

	/// <summary>
	/// Set cursor position in the terminal
	/// </summary>
	public static void SetCursorPosition(int x, int y)
	{
		BlackBox.Terminal? terminal = Window.GetTerminal();
		terminal?.SetCursorPosition(x, y);
	}

	/// <summary>
	/// Set foreground color
	/// </summary>
	public static void SetColor(byte r, byte g, byte b)
	{
		BlackBox.Terminal? terminal = Window.GetTerminal();
		terminal?.SetColor(r, g, b);
	}

	/// <summary>
	/// Set background color
	/// </summary>
	public static void SetBackgroundColor(byte r, byte g, byte b)
	{
		BlackBox.Terminal? terminal = Window.GetTerminal();
		terminal?.SetBackgroundColor(r, g, b);
	}

	/// <summary>
	/// Reset colors to default
	/// </summary>
	public static void ResetColors()
	{
		BlackBox.Terminal? terminal = Window.GetTerminal();
		terminal?.ResetColors();
	}

	/// <summary>
	/// Get character at position
	/// </summary>
	public static char GetChar(int x, int y)
	{
		BlackBox.Terminal? terminal = Window.GetTerminal();
		return terminal?.GetChar(x, y) ?? ' ';
	}

	/// <summary>
	/// Get terminal width
	/// </summary>
	public static int GetWidth()
	{
		BlackBox.Terminal? terminal = Window.GetTerminal();
		return terminal?.Width ?? 0;
	}

	/// <summary>
	/// Get terminal height
	/// </summary>
	public static int GetHeight()
	{
		BlackBox.Terminal? terminal = Window.GetTerminal();
		return terminal?.Height ?? 0;
	}
}
