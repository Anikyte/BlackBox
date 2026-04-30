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
	private const string TITLE = "Black Box";
	private const string FONT_PATH = "./JetBrainsMono-Regular.ttf";

	public static int Gap = 6;
	public static int CharSpacingH = -1;
	public static int CharSpacingV = 3;

	private readonly GraphicsDeviceManager graphics;
	private SpriteBatch? spriteBatch;
	private Texture2D? pixelTexture;
	private FontSystem? fontSystem;
	private DynamicSpriteFont? font;

	private int charWidth;
	private int charHeight;
	private int cellWidth;
	private int cellHeight;
	private float fontWidthPerSize;
	
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

		if (!File.Exists(FONT_PATH))
		{
			Console.WriteLine("Could not find font file: " + FONT_PATH);
			Exit();
			return;
		}

		fontSystem = new FontSystem();
		fontSystem.AddFont(File.ReadAllBytes(FONT_PATH));

		// Measure font dimensions at reference size to get scaling ratio
		var refFont = fontSystem.GetFont(100);
		var refSize = refFont.MeasureString("M");
		fontWidthPerSize = refSize.X / 100f;

		// Initial sizing
		graphics.PreferredBackBufferWidth = 1280;
		graphics.PreferredBackBufferHeight = 720;
		graphics.ApplyChanges();

		RecalculateTerminal();

		MainPanel = new RenderTarget2D(GraphicsDevice, System.Terminal.Width * cellWidth, System.Terminal.Height * cellHeight);
		BitmapPanel = new RenderTarget2D(GraphicsDevice, 256, 256);
		FilePanel = new RenderTarget2D(GraphicsDevice, 256, 256);
		Background = new RenderTarget2D(GraphicsDevice, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

		pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
		pixelTexture.SetData(new[] { Color.White });
	}

	private void RecalculateTerminal()
	{
		Recalculate(Window.ClientBounds.Width, Window.ClientBounds.Height);

		// Calculate cell size to fit Terminal.Width columns in main panel
		cellWidth = MainPanelRectangle.Width / System.Terminal.Width;
		charWidth = cellWidth - CharSpacingH;

		// Calculate font size from desired character width
		int fontSize = (int)(charWidth / fontWidthPerSize);
		font = fontSystem?.GetFont(fontSize);

		// Measure actual character height from font
		var measured = font?.MeasureString("M") ?? new System.Numerics.Vector2(charWidth, charWidth);
		charHeight = (int)measured.Y;
		cellHeight = charHeight + CharSpacingV;

		System.Terminal.Height = MainPanelRectangle.Height / cellHeight;
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

		RecalculateTerminal();

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
				int posX = x * cellWidth;
				int posY = y * cellHeight;

				spriteBatch.Draw(pixelTexture, new Rectangle(posX, posY, cellWidth, cellHeight), new Color(bgColor.r, bgColor.g, bgColor.b));
				if (ch != ' ')
					spriteBatch.DrawString(font, ch.ToString(), new Vector2(posX, posY), new Color(fgColor.r, fgColor.g, fgColor.b));
			}
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