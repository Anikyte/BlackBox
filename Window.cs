using BlackBox.Machine;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Panel = System.Panel;
using Terminal = System.Terminal;
using Bitmap = System.Peripherals.Bitmap;

namespace BlackBox;

public class Window : Game
{
	private const string Title = "Black Box";
	private const string FontPath = "./JetBrainsMono-Regular.ttf";
	private const int WindowWidth = 1600;
	private const int WindowHeight = 900;
	private const int BitmapSize = 256;
	private const double TargetFps = 60.0;

	public static int Gap { get; set; } = 6;
	public static int CharSpacingH { get; set; } = -1;
	public static int CharSpacingV { get; set; } = 3;

	private readonly GraphicsDeviceManager graphics;
	private SpriteBatch spriteBatch = null!;
	private Texture2D pixelTexture = null!;
	private Texture2D bitmapTexture = null!;
	private FontSystem fontSystem = null!;
	private DynamicSpriteFont font = null!;

	private int cellWidth;
	private int cellHeight;
	private float fontWidthPerSize;

	private int CharWidth => cellWidth - CharSpacingH;
	private int CharHeight => cellHeight - CharSpacingV;

	public static Rectangle PanelRectangle { get; private set; }
	public static Rectangle TerminalRectangle { get; private set; }
	public static Rectangle BitmapPanelRectangle { get; private set; }
	public static Rectangle FilePanelRectangle { get; private set; }
	public static Rectangle BackgroundRectangle { get; private set; }

	public static RenderTarget2D PanelRenderTarget { get; private set; } = null!;
	public static RenderTarget2D TerminalRenderTarget { get; private set; } = null!;
	public static RenderTarget2D BitmapPanel { get; private set; } = null!;
	public static RenderTarget2D FilePanel { get; private set; } = null!;
	public static RenderTarget2D Background { get; private set; } = null!;
	
	public Window()
	{
		graphics = new GraphicsDeviceManager(this);
		Content.RootDirectory = "Content";
		IsMouseVisible = true;
		IsFixedTimeStep = true;
		TargetElapsedTime = TimeSpan.FromSeconds(1.0 / TargetFps);
		Window.Title = Title;
	}

	protected override void Initialize()
	{
		Input.Initialize(Window);
		base.Initialize();
	}

	protected override void LoadContent()
	{
		spriteBatch = new SpriteBatch(GraphicsDevice);

		if (!File.Exists(FontPath))
		{
			Console.WriteLine("Could not find font file: " + FontPath);
			Exit();
			return;
		}

		fontSystem = new FontSystem();
		fontSystem.AddFont(File.ReadAllBytes(FontPath));

		var refFont = fontSystem.GetFont(100);
		fontWidthPerSize = refFont.MeasureString("M").X / 100f;

		graphics.PreferredBackBufferWidth = WindowWidth;
		graphics.PreferredBackBufferHeight = WindowHeight;
		graphics.ApplyChanges();

		CalculateLayout();

		PanelRenderTarget = new RenderTarget2D(GraphicsDevice, Panel.Width * cellWidth, Panel.Height * cellHeight);
		TerminalRenderTarget = new RenderTarget2D(GraphicsDevice, Terminal.Width * cellWidth, Terminal.Height * cellHeight);
		BitmapPanel = new RenderTarget2D(GraphicsDevice, BitmapSize, BitmapSize);
		FilePanel = new RenderTarget2D(GraphicsDevice, BitmapSize, BitmapSize);
		Background = new RenderTarget2D(GraphicsDevice, WindowWidth, WindowHeight);

		pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
		pixelTexture.SetData(new[] { Color.White });

		bitmapTexture = new Texture2D(GraphicsDevice, Bitmap.Width, Bitmap.Height);
		Bitmap.Clear();
	}

	private void CalculateLayout()
	{
		int sideW = (int)(WindowWidth * 0.3f);
		int miniSize = Math.Min(sideW, (WindowHeight - Gap) / 2);
		sideW = Math.Min(sideW, miniSize);
		int mainW = WindowWidth - sideW - Gap;

		BackgroundRectangle = new Rectangle(0, 0, WindowWidth, WindowHeight);
		BitmapPanelRectangle = new Rectangle(mainW + Gap, 0, sideW, miniSize);
		FilePanelRectangle = new Rectangle(mainW + Gap, WindowHeight - miniSize, sideW, miniSize);

		cellWidth = mainW / Terminal.Width;
		int fontSize = (int)(CharWidth / fontWidthPerSize);
		font = fontSystem.GetFont(fontSize);
		cellHeight = (int)font.MeasureString("M").Y + CharSpacingV;

		int totalRows = WindowHeight / cellHeight;
		int panelRows = (int)(totalRows * 0.6f);
		int terminalRows = totalRows - panelRows;
		Panel.Height = panelRows;
		Terminal.Height = terminalRows;

		int panelH = panelRows * cellHeight;
		int terminalH = terminalRows * cellHeight;
		PanelRectangle = new Rectangle(0, 0, mainW, panelH);
		TerminalRectangle = new Rectangle(0, panelH, mainW, terminalH);
	}

	protected override void Update(GameTime gameTime)
	{
		Input.Update(gameTime);
		Host.Loop();
		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.SetRenderTarget(Background);
		GraphicsDevice.Clear(Color.White);

		GraphicsDevice.SetRenderTarget(PanelRenderTarget);
		GraphicsDevice.Clear(Color.Black);
		spriteBatch.Begin(samplerState: SamplerState.PointClamp);
		for (int y = 0; y < Panel.Height; y++)
		for (int x = 0; x < Panel.Width; x++)
		{
			var (bgR, bgG, bgB) = Panel.GetBackgroundColor(x, y);
			var (fgR, fgG, fgB) = Panel.GetForegroundColor(x, y);
			var ch = Panel.GetChar(x, y);
			int posX = x * cellWidth, posY = y * cellHeight;
			spriteBatch.Draw(pixelTexture, new Rectangle(posX, posY, cellWidth, cellHeight), new Color(bgR, bgG, bgB));
			if (ch != ' ')
				spriteBatch.DrawString(font, ch.ToString(), new Vector2(posX, posY), new Color(fgR, fgG, fgB));
		}
		spriteBatch.End();

		GraphicsDevice.SetRenderTarget(TerminalRenderTarget);
		GraphicsDevice.Clear(Color.Black);
		spriteBatch.Begin(samplerState: SamplerState.PointClamp);
		for (int y = 0; y < Terminal.Height; y++)
		for (int x = 0; x < Terminal.Width; x++)
		{
			var (bgR, bgG, bgB) = Terminal.GetBackgroundColor(x, y);
			var (fgR, fgG, fgB) = Terminal.GetForegroundColor(x, y);
			var ch = Terminal.GetChar(x, y);
			int posX = x * cellWidth, posY = y * cellHeight;
			spriteBatch.Draw(pixelTexture, new Rectangle(posX, posY, cellWidth, cellHeight), new Color(bgR, bgG, bgB));
			if (ch != ' ')
				spriteBatch.DrawString(font, ch.ToString(), new Vector2(posX, posY), new Color(fgR, fgG, fgB));
		}
		spriteBatch.End();

		GraphicsDevice.SetRenderTarget(BitmapPanel);
		bitmapTexture.SetData(Bitmap.Buffer);
		spriteBatch.Begin(samplerState: SamplerState.PointClamp);
		spriteBatch.Draw(bitmapTexture, new Rectangle(0, 0, BitmapSize, BitmapSize), Color.White);
		spriteBatch.End();

		GraphicsDevice.SetRenderTarget(FilePanel);
		GraphicsDevice.Clear(Color.Gray);

		GraphicsDevice.SetRenderTarget(null);
		spriteBatch.Begin(samplerState: SamplerState.AnisotropicClamp);
		spriteBatch.Draw(Background, BackgroundRectangle, Color.White);
		spriteBatch.Draw(PanelRenderTarget, PanelRectangle, Color.White);
		spriteBatch.Draw(TerminalRenderTarget, TerminalRectangle, Color.White);
		spriteBatch.Draw(FilePanel, FilePanelRectangle, Color.White);
		spriteBatch.End();

		spriteBatch.Begin(samplerState: SamplerState.PointClamp);
		spriteBatch.Draw(BitmapPanel, BitmapPanelRectangle, Color.White);
		spriteBatch.End();

		base.Draw(gameTime);
	}

	protected override void UnloadContent()
	{
		pixelTexture.Dispose();
		base.UnloadContent();
	}
	

	public static void Main()
	{
		Window game = new();
		game.Run();
	}
}