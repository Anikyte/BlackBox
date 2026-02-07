using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BlackBox;

/// <summary>
/// Terminal emulator with VT100-like capabilities and keyboard input (hostspace)
/// </summary>

//todo: IMPORTANT: modify terminal to be writable as a grid rather than standard line by line api
//so Write('M', 12, 45) would write a character to (12,45)
//integrate vectors?

//todo: IMPORTANT: line wrap
public class Terminal
{
	public int Width { get; private set; }
	public int Height { get; private set; }

	// Scrollback buffer
	private const int SCROLLBACK_LINES = 1000;
	private const int TOTAL_BUFFER_LINES = SCROLLBACK_LINES;

	private char[,] buffer;
	private (byte r, byte g, byte b)[,] fgColors;
	private (byte r, byte g, byte b)[,] bgColors;

	public int CursorX;
	public int CursorY;
	public int ViewportOffset;
	public int ContentLines;

	public (byte r, byte g, byte b) DefaultFg = (255, 255, 255);
	public (byte r, byte g, byte b) DefaultBg = (0, 0, 0);
	public (byte r, byte g, byte b) CurrentFg;
	public (byte r, byte g, byte b) CurrentBg;

	// Input management
	private KeyboardState _currentKeyState;
	private KeyboardState _previousKeyState;
	private readonly Queue<char> _charQueue = new();
	private readonly Dictionary<Keys, double> _keyRepeatTimers = new();
	private double _elapsedTime;

	private const double RepeatDelay = 0.5;  // 500ms before repeat starts
	private const double RepeatRate = 0.03;  // 30ms between repeats

	public Terminal(int width = 80, int height = 25)
	{
		Width = width;
		Height = height;
		buffer = new char[TOTAL_BUFFER_LINES, width];
		fgColors = new (byte, byte, byte)[TOTAL_BUFFER_LINES, width];
		bgColors = new (byte, byte, byte)[TOTAL_BUFFER_LINES, width];
		CurrentFg = DefaultFg;
		CurrentBg = DefaultBg;
		Clear();
	}

	public void Clear()
	{
		for (int y = 0; y < TOTAL_BUFFER_LINES; y++)
		{
			for (int x = 0; x < Width; x++)
			{
				buffer[y, x] = ' ';
				fgColors[y, x] = DefaultFg;
				bgColors[y, x] = DefaultBg;
			}
		}
		CursorX = 0;
		CursorY = 0;
		ViewportOffset = 0;
		ContentLines = 0;
	}

	public void Write(string text)
	{
		ScrollToBottom();

		foreach (char c in text)
		{
			WriteChar(c);
		}

		ContentLines = Math.Max(ContentLines, CursorY + 1);
	}

	private void WriteChar(char c)
	{
		switch (c)
		{
			case '\n':
				CursorX = 0;
				CursorY++;
				if (CursorY >= TOTAL_BUFFER_LINES)
				{
					ScrollUp();
					CursorY = TOTAL_BUFFER_LINES - 1;
				}
				break;

			case '\r':
				CursorX = 0;
				break;

			case '\t':
				CursorX = (CursorX + 8) & ~7;
				if (CursorX >= Width)
				{
					CursorX = 0;
					CursorY++;
					if (CursorY >= TOTAL_BUFFER_LINES)
					{
						ScrollUp();
						CursorY = TOTAL_BUFFER_LINES - 1;
					}
				}
				break;

			default:
				if (CursorX >= Width)
				{
					CursorX = 0;
					CursorY++;
					if (CursorY >= TOTAL_BUFFER_LINES)
					{
						ScrollUp();
						CursorY = TOTAL_BUFFER_LINES - 1;
					}
				}

				buffer[CursorY, CursorX] = c;
				fgColors[CursorY, CursorX] = CurrentFg;
				bgColors[CursorY, CursorX] = CurrentBg;
				CursorX++;
				break;
		}
	}

	private void ScrollUp()
	{
		for (int y = 0; y < TOTAL_BUFFER_LINES - 1; y++)
		{
			for (int x = 0; x < Width; x++)
			{
				buffer[y, x] = buffer[y + 1, x];
				fgColors[y, x] = fgColors[y + 1, x];
				bgColors[y, x] = bgColors[y + 1, x];
			}
		}

		for (int x = 0; x < Width; x++)
		{
			buffer[TOTAL_BUFFER_LINES - 1, x] = ' ';
			fgColors[TOTAL_BUFFER_LINES - 1, x] = DefaultFg;
			bgColors[TOTAL_BUFFER_LINES - 1, x] = DefaultBg;
		}

		if (CursorY > 0)
			CursorY--;
		if (ViewportOffset > 0)
			ViewportOffset--;
		if (ContentLines > 0)
			ContentLines--;
	}

	public void PageUp()
	{
		int scrollAmount = Height - 1;
		ViewportOffset = Math.Max(0, ViewportOffset - scrollAmount);
	}

	public void PageDown()
	{
		int scrollAmount = Height - 1;
		int maxOffset = Math.Max(0, ContentLines - Height);
		ViewportOffset = Math.Min(maxOffset, ViewportOffset + scrollAmount);
	}

	public void ScrollToBottom()
	{
		int maxOffset = Math.Max(0, ContentLines - Height);
		ViewportOffset = maxOffset;
	}

	public bool IsAtBottom()
	{
		int maxOffset = Math.Max(0, ContentLines - Height);
		return ViewportOffset >= maxOffset;
	}

	public char GetChar(int x, int y)
	{
		int bufferY = y + ViewportOffset;
		if (x < 0 || x >= Width || y < 0 || y >= Height || bufferY >= TOTAL_BUFFER_LINES)
			return ' ';
		return buffer[bufferY, x];
	}

	public (byte r, byte g, byte b) GetForegroundColor(int x, int y)
	{
		int bufferY = y + ViewportOffset;
		if (x < 0 || x >= Width || y < 0 || y >= Height || bufferY >= TOTAL_BUFFER_LINES)
			return DefaultFg;
		return fgColors[bufferY, x];
	}

	public (byte r, byte g, byte b) GetBackgroundColor(int x, int y)
	{
		int bufferY = y + ViewportOffset;
		if (x < 0 || x >= Width || y < 0 || y >= Height || bufferY >= TOTAL_BUFFER_LINES)
			return DefaultBg;
		return bgColors[bufferY, x];
	}

	// Input methods
	public void InitializeInput(GameWindow window)
	{
		window.TextInput += OnTextInput;
	}

	private void OnTextInput(object? sender, TextInputEventArgs e)
	{
		// Only accept printable ASCII characters (32-126)
		if (e.Character >= 32 && e.Character <= 126)
		{
			_charQueue.Enqueue(e.Character);
		}
	}

	public int GetCharPressed()
	{
		return _charQueue.Count > 0 ? _charQueue.Dequeue() : 0;
	}

	public bool IsKeyPressed(Keys key)
	{
		return _currentKeyState.IsKeyDown(key) && _previousKeyState.IsKeyUp(key);
	}

	public bool IsKeyDown(Keys key)
	{
		return _currentKeyState.IsKeyDown(key);
	}

	public bool IsKeyPressedRepeat(Keys key)
	{
		bool isDown = _currentKeyState.IsKeyDown(key);
		bool wasDown = _previousKeyState.IsKeyDown(key);

		// First press
		if (isDown && !wasDown)
		{
			_keyRepeatTimers[key] = 0;
			return true;
		}

		// Key held down
		if (isDown && wasDown)
		{
			if (!_keyRepeatTimers.ContainsKey(key))
				_keyRepeatTimers[key] = 0;

			_keyRepeatTimers[key] += _elapsedTime;

			// After delay, start repeating
			if (_keyRepeatTimers[key] >= RepeatDelay)
			{
				double timeInRepeat = _keyRepeatTimers[key] - RepeatDelay;
				if (timeInRepeat >= RepeatRate)
				{
					_keyRepeatTimers[key] = RepeatDelay;  // Reset to start of repeat phase
					return true;
				}
			}
		}

		// Key released
		if (!isDown && _keyRepeatTimers.ContainsKey(key))
		{
			_keyRepeatTimers.Remove(key);
		}

		return false;
	}

	public void UpdateInput(GameTime gameTime)
	{
		_previousKeyState = _currentKeyState;
		_currentKeyState = Keyboard.GetState();
		_elapsedTime = gameTime.ElapsedGameTime.TotalSeconds;
	}
}
