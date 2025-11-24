using BlackBox.Machine;
using Raylib_cs;
using System.Numerics;

namespace BlackBox;

/// <summary>
/// Raylib window with terminal rendering and shader support (hostspace)
/// </summary>
public static class Window
{
	private const int TERMINAL_WIDTH = 80;
	private const int TERMINAL_HEIGHT = 25;
	private const string TITLE = "Black Box";
	private const string FONT_PATH = "./JetBrainsMono-Regular.ttf";
	private const int FONT_SIZE = 24;
	private const int TARGET_FPS = 60;
	private const float CURSOR_BLINK_RATE = 0.5f;

	public static Terminal Terminal = new(TERMINAL_WIDTH, TERMINAL_HEIGHT);
	private static RenderTexture2D renderTexture;
	private static Font font;
	private static int charWidth = 8;
	private static int charHeight = FONT_SIZE;
	private static int windowWidth;
	private static int windowHeight;
	private static bool showCursor = true;
	private static float cursorBlinkTime;
	//private static Shader _postProcessShader;


	public static void Main()
	{
		Raylib.InitWindow(800, 600, TITLE);
		Raylib.SetTargetFPS(TARGET_FPS);

		if (File.Exists(FONT_PATH))
		{
			font = Raylib.LoadFontEx(FONT_PATH, charHeight, null, 0);
			Raylib.SetTextureFilter(font.Texture, TextureFilter.Anisotropic16X);
		}
		else
		{
			Console.WriteLine("Could not find font file: " + FONT_PATH);
			font = Raylib.GetFontDefault();
		}

		var testChar = Raylib.MeasureTextEx(font, "M", charHeight, 0);
		charWidth = (int)testChar.X;

		windowWidth = TERMINAL_WIDTH * charWidth;
		windowHeight = (TERMINAL_HEIGHT + 1) * charHeight;
		Raylib.SetWindowSize(windowWidth, windowHeight);

		renderTexture = Raylib.LoadRenderTexture(windowWidth, windowHeight);

		// if (File.Exists(fragmentShaderPath))
		// {
		// 	_postProcessShader = Raylib.LoadShader(null, fragmentShaderPath);
		// }
		
		while (!ShouldClose())
		{
			BeginFrame();
			ProcessScrolling();
			Host.Loop();
			Render();
			EndFrame();
		}

		Raylib.UnloadRenderTexture(renderTexture);
		Raylib.UnloadFont(font);
		//Raylib.UnloadShader(_postProcessShader);
		Raylib.CloseWindow();
	}

	public static bool ShouldClose() => Raylib.WindowShouldClose();

	public static void BeginFrame()
	{
		Raylib.BeginTextureMode(renderTexture);
		Raylib.ClearBackground(Color.Black);
	}

	public static void ProcessScrolling()
	{
		if (Raylib.IsKeyPressed(KeyboardKey.PageUp))
			Terminal.PageUp();
		else if (Raylib.IsKeyPressed(KeyboardKey.PageDown))
			Terminal.PageDown();

		if (Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl))
		{
			if (Raylib.IsKeyPressed(KeyboardKey.Up))
				Terminal.PageUp();
			else if (Raylib.IsKeyPressed(KeyboardKey.Down))
				Terminal.PageDown();
		}
	}

	public static void Render()
	{
		for (int y = 0; y < Terminal.Height; y++)
		{
			for (int x = 0; x < Terminal.Width; x++)
			{
				var bgColor = Terminal.GetBackgroundColor(x, y);
				var fgColor = Terminal.GetForegroundColor(x, y);
				var ch = Terminal.GetChar(x, y);
				int posX = x * charWidth;
				int posY = y * charHeight;

				Raylib.DrawRectangle(posX, posY, charWidth, charHeight, new Color((int)bgColor.r, bgColor.g, bgColor.b, 255));

				if (ch != ' ')
					Raylib.DrawTextEx(font, ch.ToString(), new Vector2(posX, posY), charHeight, 0, new Color((int)fgColor.r, fgColor.g, fgColor.b, 255));
			}
		}

		cursorBlinkTime += Raylib.GetFrameTime();
		if (cursorBlinkTime >= CURSOR_BLINK_RATE)
		{
			showCursor = !showCursor;
			cursorBlinkTime = 0;
		}

		if (showCursor)
		{
			int cursorScreenY = Terminal.CursorY - Terminal.ViewportOffset;
			if (cursorScreenY >= 0 && cursorScreenY < Terminal.Height)
			{
				int cursorX = Terminal.CursorX * charWidth;
				int cursorY = (cursorScreenY + 1) * charHeight - 5;
				Raylib.DrawRectangle(cursorX, cursorY, charWidth, 2, Color.White);
			}
		}

		Raylib.EndTextureMode();
	}

	public static void EndFrame()
	{
		Raylib.BeginDrawing();
		Raylib.ClearBackground(Color.Black);
		
		//Raylib.BeginShaderMode(_postProcessShader);

		Raylib.DrawTextureRec(renderTexture.Texture, new Rectangle(0, 0, renderTexture.Texture.Width, -renderTexture.Texture.Height), new Vector2(0, 0), Color.White);

		//Raylib.EndShaderMode();

		Raylib.EndDrawing();
	}
}