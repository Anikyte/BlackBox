using System.Numerics;
using BlackBox.Machine;

namespace System;

public static class Terminal
{
	public static int Width { get; private set; } = 100;

	private static int _height = 25;
	public static int Height
	{
		get => _height;
		internal set
		{
			if (value == _height) return;
			_height = value;
			_buffer = new char[_height, Width];
			_fgColors = new (byte, byte, byte)[_height, Width];
			_bgColors = new (byte, byte, byte)[_height, Width];
			Clear();
		}
	}

	private static char[,] _buffer = new char[25, Width];
	private static (byte r, byte g, byte b)[,] _fgColors = new (byte, byte, byte)[25, Width];
	private static (byte r, byte g, byte b)[,] _bgColors = new (byte, byte, byte)[25, Width];

	public static (byte r, byte g, byte b) DefaultFg = (255, 255, 255);
	public static (byte r, byte g, byte b) DefaultBg = (0, 0, 0);
	public static (byte r, byte g, byte b) ColorFg = (255, 255, 255);
	public static (byte r, byte g, byte b) ColorBg = (0, 0, 0);

	public static int CursorX;
	public static int CursorY;
	
	internal static List<SubProcess> ClearEventListeners = new();
	internal static List<SubProcess> WriteEventListeners = new();

	static Terminal() => Clear();

	// === Core Grid Operations ===

	public static void SetChar(int x, int y, char c, (byte r, byte g, byte b)? fg = null, (byte r, byte g, byte b)? bg = null)
	{
		if (x < 0 || x >= Width || y < 0 || y >= Height) return;
		_buffer[y, x] = c;
		_fgColors[y, x] = fg ?? ColorFg;
		_bgColors[y, x] = bg ?? ColorBg;
	}

	public static void SetChar(Vector2 pos, char c, (byte r, byte g, byte b)? fg = null, (byte r, byte g, byte b)? bg = null) => SetChar((int)pos.X, (int)pos.Y, c, fg, bg);

	public static char GetChar(int x, int y) =>
		(x < 0 || x >= Width || y < 0 || y >= Height) ? ' ' : _buffer[y, x];

	public static char GetChar(Vector2 pos) => GetChar((int)pos.X, (int)pos.Y);

	public static void SetRow(int y, string text, int startX = 0, (byte r, byte g, byte b)? fg = null, (byte r, byte g, byte b)? bg = null)
	{
		if (y < 0 || y >= Height) return;
		for (int x = startX; x < Width; x++)
			if (x >= 0) SetChar(x, y, x - startX < text.Length ? text[x - startX] : ' ', fg, bg);
	}

	public static void SetRow(Vector2 pos, string text, (byte r, byte g, byte b)? fg = null, (byte r, byte g, byte b)? bg = null) => SetRow((int)pos.Y, text, (int)pos.X, fg, bg);

	public static void SetColumn(int x, string text, int startY = 0, (byte r, byte g, byte b)? fg = null, (byte r, byte g, byte b)? bg = null)
	{
		if (x < 0 || x >= Width) return;
		for (int y = startY; y < Height; y++)
			if (y >= 0) SetChar(x, y, y - startY < text.Length ? text[y - startY] : ' ', fg, bg);
	}

	public static void SetColumn(Vector2 pos, string text, (byte r, byte g, byte b)? fg = null, (byte r, byte g, byte b)? bg = null) => SetColumn((int)pos.X, text, (int)pos.Y, fg, bg);

	public static string GetRow(int y)
	{
		if (y < 0 || y >= Height) return new string(' ', Width);
		char[] row = new char[Width];
		for (int x = 0; x < Width; x++) row[x] = _buffer[y, x];
		return new string(row);
	}

	public static string GetColumn(int x)
	{
		if (x < 0 || x >= Width) return new string(' ', Height);
		char[] col = new char[Height];
		for (int y = 0; y < Height; y++) col[y] = _buffer[y, x];
		return new string(col);
	}

	public static void Clear()
	{
		foreach (SubProcess process in ClearEventListeners)
		{
			Process.Send(process, new Message("ClearEvent", ""));
		}
		
		for (int y = 0; y < Height; y++)
		{
			for (int x = 0; x < Width; x++)
			{
				_buffer[y, x] = ' ';
				_fgColors[y, x] = DefaultFg;
				_bgColors[y, x] = DefaultBg;
			}
		}
		CursorX = 0;
		CursorY = 0;
	}

	public static (byte r, byte g, byte b) GetForegroundColor(int x, int y) =>
		(x < 0 || x >= Width || y < 0 || y >= Height) ? DefaultFg : _fgColors[y, x];

	public static (byte r, byte g, byte b) GetBackgroundColor(int x, int y) =>
		(x < 0 || x >= Width || y < 0 || y >= Height) ? DefaultBg : _bgColors[y, x];

	public static void ResetColors()
	{
		ColorFg = DefaultFg;
		ColorBg = DefaultBg;
	}

	// === IPC-based Write ===

	public static void Write(string text)
	{
		foreach (SubProcess process in WriteEventListeners)
			Process.Send(process, new Message("WriteEvent", text));
	}

	public static void WriteLine(string text) => Write(text + "\n");
}