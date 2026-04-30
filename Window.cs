using BlackBox.Machine;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace BlackBox;

//todo: IMPORTANT: create api for bitmap and file subwindows
//also make file subwindow actually useful!

/// <summary>
/// MonoGame window with terminal rendering (hostspace)
/// </summary>
public class Window : Game
{
	private const int TERMINAL_WIDTH = 80;
	private const int TERMINAL_HEIGHT = 25;
	private const string TITLE = "Black Box";
	private const string FONT_PATH = "./JetBrainsMono-Regular.ttf";
	private const int FONT_SIZE = 24;
	private const float CURSOR_BLINK_RATE = 0.5f;

	public static int Gap = 6;

	private readonly GraphicsDeviceManager graphics;
	private SpriteBatch? spriteBatch;
	private Texture2D? pixelTexture;
	private DynamicSpriteFont? font;
	private FontSystem? fontSystem;

	private int charWidth = 8;
	private int charHeight = FONT_SIZE;
	private int windowWidth;
	private int windowHeight;
	private bool showCursor = true;
	private float cursorBlinkTime;
	
	public static Rectangle MainPanelRectangle;
	public static Rectangle BitmapPanelRectangle;
	public static Rectangle FilePanelRectangle;
	public static Rectangle BackgroundRectangle; 
	
	public static RenderTarget2D MainPanel;
	public static RenderTarget2D BitmapPanel;
	public static RenderTarget2D FilePanel;
	public static RenderTarget2D Background;
	
	public Window()
	{
		graphics = new GraphicsDeviceManager(this);
		Content.RootDirectory = "Content";
		IsMouseVisible = true;
		IsFixedTimeStep = true;
		TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);
		Window.Title = TITLE;
	}

	protected override void Initialize()
	{
		Input.Initialize(Window);
		base.Initialize();
	}

	protected override void LoadContent()
	{
		spriteBatch = new SpriteBatch(GraphicsDevice);

		// Load font using FontStashSharp
		if (File.Exists(FONT_PATH))
		{
			fontSystem = new FontSystem();
			fontSystem.AddFont(File.ReadAllBytes(FONT_PATH));
			font = fontSystem.GetFont(charHeight);
		}
		else
		{
			Console.WriteLine("Could not find font file: " + FONT_PATH);
			// Fallback: we'd need a default font - for now just exit
			Exit();
			return;
		}

		// Measure character dimensions
		var testSize = font.MeasureString("M");
		charWidth = (int)testSize.X;
		
		// Calculate window size
		windowWidth = TERMINAL_WIDTH * charWidth;
		windowHeight = (TERMINAL_HEIGHT + 1) * charHeight;

		graphics.PreferredBackBufferWidth = windowWidth + Gap + 300;
		graphics.PreferredBackBufferHeight = windowHeight;
		graphics.ApplyChanges();

		MainPanel = new RenderTarget2D(GraphicsDevice, windowWidth, windowHeight);
		BitmapPanel = new RenderTarget2D(GraphicsDevice, 256, 256);
		FilePanel = new RenderTarget2D(GraphicsDevice, 256, 256);
		Background = new RenderTarget2D(GraphicsDevice, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

		// Create 1x1 white pixel for rectangle drawing
		pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
		pixelTexture.SetData(new[] { Color.White });
	}

	protected override void Update(GameTime gameTime)
	{
		Input.Update(gameTime);
		Host.Loop();
		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime)
	{
		if (spriteBatch == null || pixelTexture == null || font == null)
			return;

		Recalculate(Window.ClientBounds.Width, Window.ClientBounds.Height);

		// --- Background ---
		GraphicsDevice.SetRenderTarget(Background);
		GraphicsDevice.Clear(Color.White);

		// --- Main Panel (terminal) ---
		GraphicsDevice.SetRenderTarget(MainPanel);
		GraphicsDevice.Clear(Color.Black);
		spriteBatch.Begin(samplerState: SamplerState.PointClamp);
		for (int y = 0; y < System.Terminal.Height; y++)
		{
			for (int x = 0; x < System.Terminal.Width; x++)
			{
				var bgColor = System.Terminal.GetBackgroundColor(x, y);
				var fgColor = System.Terminal.GetForegroundColor(x, y);
				var ch = System.Terminal.GetChar(x, y);
				int posX = x * charWidth;
				int posY = y * charHeight;

				spriteBatch.Draw(pixelTexture, new Rectangle(posX, posY, charWidth, charHeight), new Color(bgColor.r, bgColor.g, bgColor.b));
				if (ch != ' ')
					spriteBatch.DrawString(font, ch.ToString(), new Vector2(posX, posY), new Color(fgColor.r, fgColor.g, fgColor.b));
			}
		}
		cursorBlinkTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
		if (cursorBlinkTime >= CURSOR_BLINK_RATE)
		{
			showCursor = !showCursor;
			cursorBlinkTime = 0;
		}
		if (showCursor)
		{
			spriteBatch.Draw(pixelTexture,
				new Rectangle(System.Terminal.CursorX * charWidth, (System.Terminal.CursorY + 1) * charHeight - 5, charWidth, 2),
				Color.White);
		}
		spriteBatch.End();

		// --- Bitmap Panel ---
		GraphicsDevice.SetRenderTarget(BitmapPanel);
		GraphicsDevice.Clear(Color.Gray);

		// --- File Panel ---
		GraphicsDevice.SetRenderTarget(FilePanel);
		GraphicsDevice.Clear(Color.Gray);

		// --- Composite to screen ---
		GraphicsDevice.SetRenderTarget(null);
		spriteBatch.Begin(samplerState: SamplerState.AnisotropicClamp);
		spriteBatch.Draw(Background, BackgroundRectangle, Color.White);
		spriteBatch.Draw(MainPanel, MainPanelRectangle, Color.White);
		spriteBatch.Draw(BitmapPanel, BitmapPanelRectangle, Color.White);
		spriteBatch.Draw(FilePanel, FilePanelRectangle, Color.White);
		spriteBatch.End();

		base.Draw(gameTime);
	}

	public static void Recalculate(int screenW, int screenH)
	{
		int sideW = (int)(screenW * 0.3f);
		int miniSize = Math.Min(sideW, (screenH - Gap) / 2);
		sideW = Math.Min(sideW, miniSize);
		int mainW = screenW - sideW - Gap;

		BackgroundRectangle = new Rectangle(0, 0, screenW, screenH);
		MainPanelRectangle = new Rectangle(0, 0, mainW, screenH);
		BitmapPanelRectangle = new Rectangle(mainW + Gap, 0, sideW, miniSize);
		FilePanelRectangle = new Rectangle(mainW + Gap, screenH - miniSize, sideW, miniSize);
	}
	
	protected override void UnloadContent()
	{
		pixelTexture?.Dispose();
		base.UnloadContent();
	}
	

	public static void Main()
	{
		Window game = new();
		game.Run();
	}
}