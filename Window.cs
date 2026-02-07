using BlackBox.Machine;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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

	private readonly GraphicsDeviceManager graphics;
	private SpriteBatch? spriteBatch;
	private Texture2D? pixelTexture;
	private DynamicSpriteFont? font;
	private FontSystem? fontSystem;

	public static Terminal Terminal = new Terminal(TERMINAL_WIDTH, TERMINAL_HEIGHT);

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
		Terminal.InitializeInput(Window);
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

		graphics.PreferredBackBufferWidth = windowWidth + 300;
		graphics.PreferredBackBufferHeight = windowHeight;
		graphics.ApplyChanges();
		
		MainPanel = new RenderTarget2D(GraphicsDevice, windowWidth, windowHeight);
		BitmapPanel = new RenderTarget2D(GraphicsDevice, 256, 256);
		FilePanel = new RenderTarget2D(GraphicsDevice, 256, 256);

		// Create 1x1 white pixel for rectangle drawing
		pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
		pixelTexture.SetData(new[] { Color.White });

		// Execute ShellRC.cs initialization
		var result = Sandbox.Execute(new Path("System/ShellRC.cs").Read());
		if (result.Success)
		{
			if (result.ReturnValue != null)
			{
				Terminal.Write($"=> {result.ReturnValue}\n");
			}
		}
		else
		{
			Terminal.Write($"ShellRC Error: {result.ErrorMessage}\n");
		}
	}

	protected override void Update(GameTime gameTime)
	{
		Terminal.UpdateInput(gameTime);
		ProcessScrolling();
		Host.Loop();
		base.Update(gameTime);
	}

	private void ProcessScrolling()
	{
		if (Terminal.IsKeyPressed(Keys.PageUp))
			Terminal.PageUp();
		else if (Terminal.IsKeyPressed(Keys.PageDown))
			Terminal.PageDown();

		if (Terminal.IsKeyDown(Keys.LeftControl) || Terminal.IsKeyDown(Keys.RightControl))
		{
			if (Terminal.IsKeyPressed(Keys.Up))
				Terminal.PageUp();
			else if (Terminal.IsKeyPressed(Keys.Down))
				Terminal.PageDown();
		}
	}

	protected override void Draw(GameTime gameTime)
	{
		if (spriteBatch == null || pixelTexture == null || font == null)
			return;
		
		//todo: only on window resize?
		Recalculate(Window.ClientBounds.Width, Window.ClientBounds.Height);
		
		// Render to texture
		GraphicsDevice.SetRenderTarget(MainPanel);
		GraphicsDevice.Clear(Color.Black);

		spriteBatch.Begin(samplerState: SamplerState.PointClamp);

		// Draw terminal characters
		for (int y = 0; y < Terminal.Height; y++)
		{
			for (int x = 0; x < Terminal.Width; x++)
			{
				var bgColor = Terminal.GetBackgroundColor(x, y);
				var fgColor = Terminal.GetForegroundColor(x, y);
				var ch = Terminal.GetChar(x, y);
				int posX = x * charWidth;
				int posY = y * charHeight;

				// Draw background rectangle
				var bgRectangle = new Rectangle(posX, posY, charWidth, charHeight);
				spriteBatch.Draw(pixelTexture, bgRectangle, new Color(bgColor.r, bgColor.g, bgColor.b));

				// Draw character
				if (ch != ' ')
				{
					spriteBatch.DrawString(font, ch.ToString(), new Vector2(posX, posY), new Color(fgColor.r, fgColor.g, fgColor.b));
				}
			}
		}

		// Update cursor blink
		cursorBlinkTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
		if (cursorBlinkTime >= CURSOR_BLINK_RATE)
		{
			showCursor = !showCursor;
			cursorBlinkTime = 0;
		}

		// Draw cursor
		if (showCursor)
		{
			int cursorScreenY = Terminal.CursorY - Terminal.ViewportOffset;
			if (cursorScreenY >= 0 && cursorScreenY < Terminal.Height)
			{ //todo: use unicode?
				int cursorX = Terminal.CursorX * charWidth;
				int cursorY = (cursorScreenY + 1) * charHeight - 5;
				var cursorRectangle = new Rectangle(cursorX, cursorY, charWidth, 2);
				spriteBatch.Draw(pixelTexture, cursorRectangle, Color.White);
			}
		}
		spriteBatch.End();

		GraphicsDevice.SetRenderTarget(null);
		GraphicsDevice.Clear(Color.Gray);
		
		spriteBatch.Begin(samplerState: SamplerState.AnisotropicClamp);

		spriteBatch.Draw(MainPanel, MainPanelRectangle, Color.White);
		spriteBatch.Draw(BitmapPanel, BitmapPanelRectangle, Color.White);
		spriteBatch.Draw(FilePanel, FilePanelRectangle, Color.White);
		
		spriteBatch.End();
		
		base.Draw(gameTime);
	}

	public void Recalculate(int screenW, int screenH)
	{
		//todo: improve this function drastically
        
		float sideWidthFraction = 0.3f; // tune this
		int sideW = (int)(screenW * sideWidthFraction);
		int miniSize = sideW; // square
        
		int gap = screenH - (miniSize * 2);
		if (gap < 0)
		{
			// screen too short - shrink minis to fit
			miniSize = screenH / 2;
			sideW = miniSize;
			gap = 0;
		}
        
		int mainW = screenW - sideW;
        
		MainPanelRectangle = new Rectangle(0, 0, mainW, screenH);
		BitmapPanelRectangle = new Rectangle(mainW, 0, sideW, miniSize);
		FilePanelRectangle = new Rectangle(mainW, screenH - miniSize, sideW, miniSize);
		BackgroundRectangle = new Rectangle(mainW, miniSize, sideW, gap);
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