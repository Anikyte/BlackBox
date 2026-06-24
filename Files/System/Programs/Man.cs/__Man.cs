using System;
using System.IO;

public static class Man
{
	private const int width = 100;
	
	private static (byte, byte, byte) fgNormal = (255, 255, 255), bgNormal = (0, 0, 0);
	private static (byte, byte, byte) fgBold = (0, 0, 0), bgBold = (255, 255, 255);
	//private static (byte, byte, byte) fgCode = (0, 0, 0), bgCode = (127, 127, 127);
	private static (byte, byte, byte) fgCode = (127, 80, 200), bgCode = (0, 0, 0);

	public static void Read(Path path)
	{
		Panel.Clear();
		Panel.SetRow(0,path.ToString(),bg:(255,255,255),fg:(0,0,0));
		string text = string.Join("\n", (string[])path.Read());
		int x = 0, y = 1;
		bool inMultiCode = false, inInlineCode = false, inBold = false;

		for (int i = 0; i < text.Length; i++)
		{
			// Check for multiline code block ```
			if (i + 2 < text.Length && text[i] == '`' && text[i + 1] == '`' && text[i + 2] == '`')
			{
				inMultiCode = !inMultiCode;
				inInlineCode = false;
				inBold = false;
				i += 2;
				// Skip to next line (skip language identifier)
				while (i + 1 < text.Length && text[i + 1] != '\n') i++;
				continue;
			}
			// Check for inline code `
			if (text[i] == '`' && !inMultiCode)
			{
				inInlineCode = !inInlineCode;
				inBold = false;
				continue;
			}
			// Check for bold/italic (any sequence of *)
			if (text[i] == '*' && !inMultiCode && !inInlineCode)
			{
				while (i + 1 < text.Length && text[i + 1] == '*') i++;
				inBold = !inBold;
				continue;
			}
			// Handle newlines
			if (text[i] == '\n')
			{
				if (inMultiCode) while (x < width) Panel.SetChar(x++, y, ' ', fgCode, bgCode);
				x = 0;
				y++;
				if (!inMultiCode) inInlineCode = false;
				continue;
			}
			// Get colors based on state (code > bold > normal)
			var fg = inMultiCode || inInlineCode ? fgCode : inBold ? fgBold : fgNormal;
			var bg = inMultiCode || inInlineCode ? bgCode : inBold ? bgBold : bgNormal;
			Panel.SetChar(x, y, text[i], fg, bg);
			x++;
			if (x >= width) { x = 0; y++; }
		}
	}

	public static void Read(Path path, int line)
	{
		Panel.Clear();
		Panel.SetRow(0, path.ToString(), bg: (255, 255, 255), fg: (0, 0, 0));
		string text = string.Join("\n", ((string[])path.Read())[(line - 1)..]);
		int x = 0, y = 1;
		bool inMultiCode = false, inInlineCode = false, inBold = false;

		for (int i = 0; i < text.Length; i++)
		{
			if (i + 2 < text.Length && text[i] == '`' && text[i + 1] == '`' && text[i + 2] == '`')
			{
				inMultiCode = !inMultiCode;
				inInlineCode = false;
				inBold = false;
				i += 2;
				while (i + 1 < text.Length && text[i + 1] != '\n') i++;
				continue;
			}
			if (text[i] == '`' && !inMultiCode)
			{
				inInlineCode = !inInlineCode;
				inBold = false;
				continue;
			}
			if (text[i] == '*' && !inMultiCode && !inInlineCode)
			{
				while (i + 1 < text.Length && text[i + 1] == '*') i++;
				inBold = !inBold;
				continue;
			}
			if (text[i] == '\n')
			{
				if (inMultiCode) while (x < width) Panel.SetChar(x++, y, ' ', fgCode, bgCode);
				x = 0;
				y++;
				if (!inMultiCode) inInlineCode = false;
				continue;
			}
			var fg = inMultiCode || inInlineCode ? fgCode : inBold ? fgBold : fgNormal;
			var bg = inMultiCode || inInlineCode ? bgCode : inBold ? bgBold : bgNormal;
			Panel.SetChar(x, y, text[i], fg, bg);
			x++;
			if (x >= width) { x = 0; y++; }
		}
	}

	public static void Read(string path) => Read(new Path(path));
	public static void Read(string path, int line) => Read(new Path(path), line);
}