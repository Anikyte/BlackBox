using BlackBox.Machine;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BlackBox;

/// <summary>
/// MonoGame window with terminal rendering (hostspace)
/// </summary>
public class BlackBox : Game
{
	private const int TERMINAL_WIDTH = 80;
	private const int TERMINAL_HEIGHT = 25;
	private const string TITLE = "Black Box";
	private const string FONT_PATH = "./JetBrainsMono-Regular.ttf";
	private const int FONT_SIZE = 24;
	private const float CURSOR_BLINK_RATE = 0.5f;

	private readonly GraphicsDeviceManager _graphics;
	private SpriteBatch? _spriteBatch;
	private RenderTarget2D? _renderTexture;
	private Texture2D? _pixelTexture;
	private DynamicSpriteFont? _font;
	private FontSystem? _fontSystem;

	public Terminal Terminal { get; private set; }

	private int _charWidth = 8;
	private int _charHeight = FONT_SIZE;
	private int _windowWidth;
	private int _windowHeight;
	private bool _showCursor = true;
	private float _cursorBlinkTime;

	public BlackBox()
	{
		_graphics = new GraphicsDeviceManager(this);
		Content.RootDirectory = "Content";
		IsMouseVisible = true;
		IsFixedTimeStep = true;
		TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);
		Window.Title = TITLE;

		Terminal = new Terminal(TERMINAL_WIDTH, TERMINAL_HEIGHT);
	}

	protected override void Initialize()
	{
		Terminal.InitializeInput(Window);
		base.Initialize();
	}

	protected override void LoadContent()
	{
		_spriteBatch = new SpriteBatch(GraphicsDevice);

		// Load font using FontStashSharp
		if (File.Exists(FONT_PATH))
		{
			_fontSystem = new FontSystem();
			_fontSystem.AddFont(File.ReadAllBytes(FONT_PATH));
			_font = _fontSystem.GetFont(_charHeight);
		}
		else
		{
			Console.WriteLine("Could not find font file: " + FONT_PATH);
			// Fallback: we'd need a default font - for now just exit
			Exit();
			return;
		}

		// Measure character dimensions
		var testSize = _font.MeasureString("M");
		_charWidth = (int)testSize.X;

		// Calculate window size
		_windowWidth = TERMINAL_WIDTH * _charWidth;
		_windowHeight = (TERMINAL_HEIGHT + 1) * _charHeight;

		_graphics.PreferredBackBufferWidth = _windowWidth;
		_graphics.PreferredBackBufferHeight = _windowHeight;
		_graphics.ApplyChanges();

		// Create render texture
		_renderTexture = new RenderTarget2D(GraphicsDevice, _windowWidth, _windowHeight);

		// Create 1x1 white pixel for rectangle drawing
		_pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
		_pixelTexture.SetData(new[] { Color.White });

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
			Terminal.Write($"Error: {result.ErrorMessage}\n");
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
		if (_spriteBatch == null || _renderTexture == null || _pixelTexture == null || _font == null)
			return;

		// Render to texture
		GraphicsDevice.SetRenderTarget(_renderTexture);
		GraphicsDevice.Clear(Color.Black);

		_spriteBatch.Begin(samplerState: SamplerState.AnisotropicClamp);

		// Draw terminal characters
		for (int y = 0; y < Terminal.Height; y++)
		{
			for (int x = 0; x < Terminal.Width; x++)
			{
				var bgColor = Terminal.GetBackgroundColor(x, y);
				var fgColor = Terminal.GetForegroundColor(x, y);
				var ch = Terminal.GetChar(x, y);
				int posX = x * _charWidth;
				int posY = y * _charHeight;

				// Draw background rectangle
				var bgRectangle = new Rectangle(posX, posY, _charWidth, _charHeight);
				_spriteBatch.Draw(_pixelTexture, bgRectangle, new Color(bgColor.r, bgColor.g, bgColor.b));

				// Draw character
				if (ch != ' ')
				{
					_spriteBatch.DrawString(_font, ch.ToString(), new Vector2(posX, posY), new Color(fgColor.r, fgColor.g, fgColor.b));
				}
			}
		}

		// Update cursor blink
		_cursorBlinkTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
		if (_cursorBlinkTime >= CURSOR_BLINK_RATE)
		{
			_showCursor = !_showCursor;
			_cursorBlinkTime = 0;
		}

		// Draw cursor
		if (_showCursor)
		{
			int cursorScreenY = Terminal.CursorY - Terminal.ViewportOffset;
			if (cursorScreenY >= 0 && cursorScreenY < Terminal.Height)
			{
				int cursorX = Terminal.CursorX * _charWidth;
				int cursorY = (cursorScreenY + 1) * _charHeight - 5;
				var cursorRectangle = new Rectangle(cursorX, cursorY, _charWidth, 2);
				_spriteBatch.Draw(_pixelTexture, cursorRectangle, Color.White);
			}
		}

		_spriteBatch.End();

		// Draw texture to screen
		GraphicsDevice.SetRenderTarget(null);
		GraphicsDevice.Clear(Color.Black);

		_spriteBatch.Begin();
		_spriteBatch.Draw(_renderTexture, Vector2.Zero, Color.White);
		_spriteBatch.End();

		base.Draw(gameTime);
	}

	protected override void UnloadContent()
	{
		_renderTexture?.Dispose();
		_pixelTexture?.Dispose();
		base.UnloadContent();
	}
}

/// <summary>
/// Static entry point wrapper
/// </summary>
public static class Window
{
	private static BlackBox game = new();

	public static Terminal Terminal => game.Terminal;

	public static void Main()
	{
		game.Run();
	}
}