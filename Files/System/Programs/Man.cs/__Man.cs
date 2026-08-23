using System;

public static class Man
{
	// (foreground, background) per notation - assign to customize
	public static ((byte r, byte g, byte b) fg, (byte r, byte g, byte b) bg) Normal { get; set; } = ((255, 255, 255), (0, 0, 0));
	public static ((byte r, byte g, byte b) fg, (byte r, byte g, byte b) bg) Heading { get; set; } = ((0, 0, 0), (255, 255, 255));
	public static ((byte r, byte g, byte b) fg, (byte r, byte g, byte b) bg) Subheading { get; set; } = ((255, 255, 255), (80, 80, 80));
	public static ((byte r, byte g, byte b) fg, (byte r, byte g, byte b) bg) Bold { get; set; } = ((125, 200, 100), (0, 0, 0));
	public static ((byte r, byte g, byte b) fg, (byte r, byte g, byte b) bg) Italic { get; set; } = ((230, 120, 65), (0, 0, 0));
	public static ((byte r, byte g, byte b) fg, (byte r, byte g, byte b) bg) Code { get; set; } = ((170, 150, 240), (0, 0, 0));

	public static void Read(Path path, int line = 1)
	{
		Panel.Clear();
		Panel.SetRow(0, path.ToString(), fg: (0, 0, 0), bg: (255, 255, 255));

		int y = 1;
		bool fenced = false;

		foreach (string source in ((string[])path.Read())[(line - 1)..])
		{
			string trimmed = source.Trim();

			if (trimmed.StartsWith("```")) { fenced = !fenced; continue; }
			if (fenced) FillRow(y++, source, Code);
			// '---' or '===' alone on a line draws a full width rule
			else if (trimmed.Length >= 3 && (trimmed.Trim('-').Length == 0 || trimmed.Trim('=').Length == 0))
				FillRow(y++, new string(trimmed[0], Panel.Width), Normal);
			else y = RenderLine(y, source);
		}
	}

	public static void Read(string path, int line = 1) => Read(new Path(path), line);

	// draws one line of markup, returns the row below it - notations never carry to the next line
	private static int RenderLine(int y, string text)
	{
		int x = 0, heading = 0;
		bool code = false, bold = false, italic = false, literal = false;

		// headings only open at the start of a line, as '# ' or '## '
		string body = text.TrimStart();
		int hashes = Read(body, 0, '#');
		if (hashes is 1 or 2 && hashes < body.Length && body[hashes] == ' ')
		{
			heading = hashes;
			text = body[(hashes + 1)..];
		}

		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			// code > heading > subheading > bold > italic
			var style = code ? Code
				: heading == 1 ? Heading
				: heading == 2 ? Subheading
				: bold ? Bold
				: italic ? Italic
				: Normal;

			// a run of backslashes toggles literal mode and prints half its length, so '\\' prints '\'
			if (c == '\\' && !code)
			{
				int slashes = Read(text, i, '\\');
				literal = !literal;
				for (int j = 0; j < slashes / 2; j++) Put(ref x, ref y, '\\', style);
				i += slashes - 1;
				continue;
			}
			if (literal) { Put(ref x, ref y, c, style); continue; }

			if (c == '`') { code = !code; continue; }
			if (code) { Put(ref x, ref y, c, style); continue; }

			// a heading closes on ' #', the rest of the line reverting to normal
			if (c == ' ' && heading > 0 && i + 1 < text.Length && text[i + 1] == '#')
			{
				i += Read(text, i + 1, '#');
				heading = 0;
				continue;
			}
			if (c == '*')
			{
				int stars = Read(text, i, '*');
				if (stars > 1) bold = !bold;
				else italic = !italic;
				i += stars - 1;
				continue;
			}
			Put(ref x, ref y, c, style);
		}
		return y + 1;
	}

	// prints a character, wrapping to the next row once the current one is full
	private static void Put(ref int x, ref int y, char c, ((byte r, byte g, byte b) fg, (byte r, byte g, byte b) bg) style)
	{
		if (x >= Panel.Width) { x = 0; y++; }
		Panel.SetChar(x++, y, c, style.fg, style.bg);
	}

	// pads the row out to the panel width so the background covers the whole line
	private static void FillRow(int y, string text, ((byte r, byte g, byte b) fg, (byte r, byte g, byte b) bg) style)
	{
		for (int x = 0; x < Panel.Width; x++) Panel.SetChar(x, y, x < text.Length ? text[x] : ' ', style.fg, style.bg);
	}

	// number of consecutive c starting at i
	private static int Read(string text, int i, char c)
	{
		int n = 0;
		while (i + n < text.Length && text[i + n] == c) n++;
		return n;
	}
}