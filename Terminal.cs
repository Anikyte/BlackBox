namespace BlackBox;

/// <summary>
/// Terminal emulator with VT100-like capabilities (hostspace)
/// </summary>
public class Terminal
{
	public int Width { get; private set; }
	public int Height { get; private set; }

	private char[,] _buffer;
	private (byte r, byte g, byte b)[,] _fgColors;
	private (byte r, byte g, byte b)[,] _bgColors;

	private int _cursorX;
	private int _cursorY;
	public int CursorX => _cursorX;
	public int CursorY => _cursorY;

	// Default colors
	private (byte r, byte g, byte b) _defaultFg = (200, 200, 200);
	private (byte r, byte g, byte b) _defaultBg = (0, 0, 0);
	private (byte r, byte g, byte b) _currentFg;
	private (byte r, byte g, byte b) _currentBg;

	public Terminal(int width = 80, int height = 25)
	{
		Width = width;
		Height = height;
		_buffer = new char[height, width];
		_fgColors = new (byte, byte, byte)[height, width];
		_bgColors = new (byte, byte, byte)[height, width];
		_currentFg = _defaultFg;
		_currentBg = _defaultBg;
		Clear();
	}

	public void Clear()
	{
		for (int y = 0; y < Height; y++)
		{
			for (int x = 0; x < Width; x++)
			{
				_buffer[y, x] = ' ';
				_fgColors[y, x] = _defaultFg;
				_bgColors[y, x] = _defaultBg;
			}
		}
		_cursorX = 0;
		_cursorY = 0;
	}

	public void Write(string text)
	{
		foreach (char c in text)
		{
			WriteChar(c);
		}
	}

	private void WriteChar(char c)
	{
		switch (c)
		{
			case '\n':
				_cursorX = 0;
				_cursorY++;
				if (_cursorY >= Height)
				{
					ScrollUp();
					_cursorY = Height - 1;
				}
				break;

			case '\r':
				_cursorX = 0;
				break;

			case '\t':
				_cursorX = (_cursorX + 8) & ~7; // Tab to next 8-char boundary
				if (_cursorX >= Width)
				{
					_cursorX = 0;
					_cursorY++;
					if (_cursorY >= Height)
					{
						ScrollUp();
						_cursorY = Height - 1;
					}
				}
				break;

			default:
				if (_cursorX >= Width)
				{
					_cursorX = 0;
					_cursorY++;
					if (_cursorY >= Height)
					{
						ScrollUp();
						_cursorY = Height - 1;
					}
				}

				_buffer[_cursorY, _cursorX] = c;
				_fgColors[_cursorY, _cursorX] = _currentFg;
				_bgColors[_cursorY, _cursorX] = _currentBg;
				_cursorX++;
				break;
		}
	}

	private void ScrollUp()
	{
		for (int y = 0; y < Height - 1; y++)
		{
			for (int x = 0; x < Width; x++)
			{
				_buffer[y, x] = _buffer[y + 1, x];
				_fgColors[y, x] = _fgColors[y + 1, x];
				_bgColors[y, x] = _bgColors[y + 1, x];
			}
		}

		// Clear last line
		for (int x = 0; x < Width; x++)
		{
			_buffer[Height - 1, x] = ' ';
			_fgColors[Height - 1, x] = _defaultFg;
			_bgColors[Height - 1, x] = _defaultBg;
		}
	}

	public void SetCursorPosition(int x, int y)
	{
		_cursorX = Math.Clamp(x, 0, Width - 1);
		_cursorY = Math.Clamp(y, 0, Height - 1);
	}

	public void SetColor(byte r, byte g, byte b)
	{
		_currentFg = (r, g, b);
	}

	public void SetBackgroundColor(byte r, byte g, byte b)
	{
		_currentBg = (r, g, b);
	}

	public void ResetColors()
	{
		_currentFg = _defaultFg;
		_currentBg = _defaultBg;
	}

	public char GetChar(int x, int y)
	{
		if (x < 0 || x >= Width || y < 0 || y >= Height)
			return ' ';
		return _buffer[y, x];
	}

	public (byte r, byte g, byte b) GetForegroundColor(int x, int y)
	{
		if (x < 0 || x >= Width || y < 0 || y >= Height)
			return _defaultFg;
		return _fgColors[y, x];
	}

	public (byte r, byte g, byte b) GetBackgroundColor(int x, int y)
	{
		if (x < 0 || x >= Width || y < 0 || y >= Height)
			return _defaultBg;
		return _bgColors[y, x];
	}
}
