namespace BlackBox;

/// <summary>
/// Terminal emulator with VT100-like capabilities (hostspace)
/// </summary>
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
}
