using System.Numerics;
using BlackBox;
using BlackBox.Machine;

namespace System;

public static class Panel
{
	public static int Width { get; private set; } = Window.CharsPerLine;

	private static int _height = 22;
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

	private static char[,] _buffer = new char[_height, Width];
	private static (byte r, byte g, byte b)[,] _fgColors = new (byte, byte, byte)[_height, Width];
	private static (byte r, byte g, byte b)[,] _bgColors = new (byte, byte, byte)[_height, Width];

	public static (byte r, byte g, byte b) DefaultFg { get; set; } = (255, 255, 255);
	public static (byte r, byte g, byte b) DefaultBg { get; set; } = (0, 0, 0);
	public static (byte r, byte g, byte b) ColorFg { get; set; } = (255, 255, 255);
	public static (byte r, byte g, byte b) ColorBg { get; set; } = (0, 0, 0);

	public static int CursorX { get; set; }
	public static int CursorY { get; set; }

	internal static List<SubProcess> ClearEventListeners = new();
	internal static List<SubProcess> WriteEventListeners = new();

	static Panel() => Clear();

	private static bool IsInBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

	public static void SetChar(int x, int y, char c, (byte r, byte g, byte b)? fg = null, (byte r, byte g, byte b)? bg = null)
	{
		if (!IsInBounds(x, y)) return;
		_buffer[y, x] = c;
		_fgColors[y, x] = fg ?? ColorFg;
		_bgColors[y, x] = bg ?? ColorBg;
	}

	public static void SetChar(Vector2 pos, char c, (byte r, byte g, byte b)? fg = null, (byte r, byte g, byte b)? bg = null) => SetChar((int)pos.X, (int)pos.Y, c, fg, bg);

	public static char GetChar(int x, int y) => IsInBounds(x, y) ? _buffer[y, x] : ' ';

	public static char GetChar(Vector2 pos) => GetChar((int)pos.X, (int)pos.Y);

	public static void SetRow(int y, string text, int startX = 0, (byte r, byte g, byte b)? fg = null, (byte r, byte g, byte b)? bg = null)
	{
		for (int x = Math.Max(0, startX); x < Width && y >= 0 && y < Height; x++)
			SetChar(x, y, x - startX < text.Length ? text[x - startX] : ' ', fg, bg);
	}

	public static void SetRow(Vector2 pos, string text, (byte r, byte g, byte b)? fg = null, (byte r, byte g, byte b)? bg = null) => SetRow((int)pos.Y, text, (int)pos.X, fg, bg);

	public static void SetColumn(int x, string text, int startY = 0, (byte r, byte g, byte b)? fg = null, (byte r, byte g, byte b)? bg = null)
	{
		for (int y = Math.Max(0, startY); y < Height && x >= 0 && x < Width; y++)
			SetChar(x, y, y - startY < text.Length ? text[y - startY] : ' ', fg, bg);
	}

	public static void SetColumn(Vector2 pos, string text, (byte r, byte g, byte b)? fg = null, (byte r, byte g, byte b)? bg = null) => SetColumn((int)pos.X, text, (int)pos.Y, fg, bg);

	public static string GetRow(int y) =>
		(y < 0 || y >= Height) ? new string(' ', Width) : string.Create(Width, y, (span, row) => { for (int x = 0; x < Width; x++) span[x] = _buffer[row, x]; });

	public static string GetColumn(int x) =>
		(x < 0 || x >= Width) ? new string(' ', Height) : string.Create(Height, x, (span, col) => { for (int y = 0; y < Height; y++) span[y] = _buffer[y, col]; });

	public static void Clear()
	{
		foreach (SubProcess process in ClearEventListeners)
			Process.Send(process, new Message("ClearEvent", ""));

		for (int y = 0; y < Height; y++)
		for (int x = 0; x < Width; x++)
			(_buffer[y, x], _fgColors[y, x], _bgColors[y, x]) = (' ', DefaultFg, DefaultBg);
		(CursorX, CursorY) = (0, 0);
	}

	public static (byte r, byte g, byte b) GetForegroundColor(int x, int y) => IsInBounds(x, y) ? _fgColors[y, x] : DefaultFg;

	public static (byte r, byte g, byte b) GetBackgroundColor(int x, int y) => IsInBounds(x, y) ? _bgColors[y, x] : DefaultBg;

	public static void ResetColors()
	{
		ColorFg = DefaultFg;
		ColorBg = DefaultBg;
	}

	public static void Write(string text)
	{
		foreach (SubProcess process in WriteEventListeners)
			Process.Send(process, new Message("WriteEvent", text));
	}

	public static void WriteLine(string text) => Write(text + "\n");
}

public static class Terminal
{
	
	public static int Width { get; private set; } = Window.CharsPerLine;

	private static int _height = 16;
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

	private static char[,] _buffer = new char[_height, Width];
	private static (byte r, byte g, byte b)[,] _fgColors = new (byte, byte, byte)[_height, Width];
	private static (byte r, byte g, byte b)[,] _bgColors = new (byte, byte, byte)[_height, Width];

	public static (byte r, byte g, byte b) DefaultFg { get; set; } = (255, 255, 255);
	public static (byte r, byte g, byte b) DefaultBg { get; set; } = (0, 0, 0);
	public static (byte r, byte g, byte b) ColorFg { get; set; } = (255, 255, 255);
	public static (byte r, byte g, byte b) ColorBg { get; set; } = (0, 0, 0);

	public static int CursorX { get; set; }
	public static int CursorY { get; set; }
	
	internal static List<SubProcess> ClearEventListeners = new();
	internal static List<SubProcess> WriteEventListeners = new();

	static Terminal() => Clear();

	private static bool IsInBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

	// === Core Grid Operations ===

	public static void SetChar(int x, int y, char c, (byte r, byte g, byte b)? fg = null, (byte r, byte g, byte b)? bg = null)
	{
		if (!IsInBounds(x, y)) return;
		_buffer[y, x] = c;
		_fgColors[y, x] = fg ?? ColorFg;
		_bgColors[y, x] = bg ?? ColorBg;
	}

	public static void SetChar(Vector2 pos, char c, (byte r, byte g, byte b)? fg = null, (byte r, byte g, byte b)? bg = null) => SetChar((int)pos.X, (int)pos.Y, c, fg, bg);

	public static char GetChar(int x, int y) => IsInBounds(x, y) ? _buffer[y, x] : ' ';

	public static char GetChar(Vector2 pos) => GetChar((int)pos.X, (int)pos.Y);

	public static void SetRow(int y, string text, int startX = 0, (byte r, byte g, byte b)? fg = null, (byte r, byte g, byte b)? bg = null)
	{
		for (int x = Math.Max(0, startX); x < Width && y >= 0 && y < Height; x++)
			SetChar(x, y, x - startX < text.Length ? text[x - startX] : ' ', fg, bg);
	}

	public static void SetRow(Vector2 pos, string text, (byte r, byte g, byte b)? fg = null, (byte r, byte g, byte b)? bg = null) => SetRow((int)pos.Y, text, (int)pos.X, fg, bg);

	public static void SetColumn(int x, string text, int startY = 0, (byte r, byte g, byte b)? fg = null, (byte r, byte g, byte b)? bg = null)
	{
		for (int y = Math.Max(0, startY); y < Height && x >= 0 && x < Width; y++)
			SetChar(x, y, y - startY < text.Length ? text[y - startY] : ' ', fg, bg);
	}

	public static void SetColumn(Vector2 pos, string text, (byte r, byte g, byte b)? fg = null, (byte r, byte g, byte b)? bg = null) => SetColumn((int)pos.X, text, (int)pos.Y, fg, bg);

	public static string GetRow(int y) =>
		(y < 0 || y >= Height) ? new string(' ', Width) : string.Create(Width, y, (span, row) => { for (int x = 0; x < Width; x++) span[x] = _buffer[row, x]; });

	public static string GetColumn(int x) =>
		(x < 0 || x >= Width) ? new string(' ', Height) : string.Create(Height, x, (span, col) => { for (int y = 0; y < Height; y++) span[y] = _buffer[y, col]; });

	public static void Clear()
	{
		foreach (SubProcess process in ClearEventListeners)
			Process.Send(process, new Message("ClearEvent", ""));

		for (int y = 0; y < Height; y++)
		for (int x = 0; x < Width; x++)
			(_buffer[y, x], _fgColors[y, x], _bgColors[y, x]) = (' ', DefaultFg, DefaultBg);
		(CursorX, CursorY) = (0, 0);
	}

	public static (byte r, byte g, byte b) GetForegroundColor(int x, int y) => IsInBounds(x, y) ? _fgColors[y, x] : DefaultFg;

	public static (byte r, byte g, byte b) GetBackgroundColor(int x, int y) => IsInBounds(x, y) ? _bgColors[y, x] : DefaultBg;

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