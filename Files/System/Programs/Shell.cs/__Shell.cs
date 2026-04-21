using BlackBox.Machine;

Terminal.Write("Shell Loaded\n");
Console.WriteLine("Shell Loaded");

//repl logic goes here

SubProcess me = Process.Self;

Process.Send(new Message("RegisterKeyEvent", "lpq", me));

Terminal.WriteLine("New shell: "+me.GUID.ToString());

while (true)
{
	if (me.Messages.TryDequeue(out var message))
	{
		Terminal.Write(message.Key + "(" + message.Timestamp + "): " + message.Value + "\n");
	}
}

/*
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

	public void UpdateInput(GameTime gameTime) //this should be elsewhere
	{
		_previousKeyState = _currentKeyState;
		_currentKeyState = Keyboard.GetState();
		_elapsedTime = gameTime.ElapsedGameTime.TotalSeconds;
	}
}

using System.Reflection;
using Sandbox = BlackBox.Machine.Sandbox;
using Window = BlackBox.Window;
using Path = System.IO.Path;

namespace System;

public static class Shell
{
	private static string GetSimpleTypeName(Type type)
	{
		if (type == typeof(void)) return "void";
		if (type == typeof(int)) return "int";
		if (type == typeof(string)) return "string";
		if (type == typeof(bool)) return "bool";
		if (type == typeof(byte)) return "byte";
		if (type == typeof(char)) return "char";
		if (type == typeof(float)) return "float";
		if (type == typeof(double)) return "double";

		// Handle generic types
		if (type.IsGenericType)
		{
			var genericArgs = type.GetGenericArguments();
			var genericName = type.Name.Substring(0, type.Name.IndexOf('`'));
			var genericParams = string.Join(", ", genericArgs.Select(GetSimpleTypeName));
			return $"{genericName}<{genericParams}>";
		}

		return type.Name;
	}
	
	// show: "custom" = just user types, "system" = namespace list, "all" = all classes
	public static void Help(string className = "", string show = "custom") //todo: fix getting from excluded system assemblies in some cases
	{
		if (className == "")
		{
			// Get all assemblies available in the sandbox
			var assemblies = AppDomain.CurrentDomain.GetAssemblies()
				.Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
				.ToList();

			// Get all public types
			var allTypes = assemblies
				.SelectMany(a => {
					try { return a.GetTypes(); }
					catch { return Array.Empty<Type>(); }
				})
				.Where(t => t.Namespace != null && t.IsPublic);

			if (show == "namespace")
			{
				// Show only namespaces for system types
				var namespaces = allTypes
					.Where(t => t.Namespace.StartsWith("System"))
					.Select(t => t.Namespace)
					.Distinct()
					.OrderBy(ns => ns);

				Window.Terminal.Write("Available System namespaces:\n");
				foreach (var ns in namespaces)
				{
					Window.Terminal.Write($"- {ns}\n");
				}
			}
			else
			{
				// Show individual types
				IEnumerable<Type> systemTypes = show switch
				{
					"simple" => allTypes.Where(t =>
						t.Assembly == typeof(Shell).Assembly && t.Namespace.StartsWith("System")),
					"all" => allTypes.Where(t => t.Namespace.StartsWith("System")),
					_ => allTypes.Where(t =>
						t.Assembly == typeof(Shell).Assembly && t.Namespace.StartsWith("System"))
				};

				systemTypes = systemTypes.OrderBy(t => t.Namespace).ThenBy(t => t.Name).ToList();

				string currentNamespace = "";
				foreach (var t in systemTypes)
				{
					if (t.Namespace != currentNamespace)
					{
						currentNamespace = t.Namespace!;
						Window.Terminal.Write($"{currentNamespace}:\n");
					}
					Window.Terminal.Write($"- {t.Name}\n");
				}
			}
		}
		else
		{
			// Search in all assemblies for the specific type
			var assemblies = AppDomain.CurrentDomain.GetAssemblies()
				.Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
				.ToList();
/*
			var type = assemblies
				.SelectMany(a => {
					try { return a.GetTypes(); }
					catch { return Array.Empty<Type>(); }
				})
				.FirstOrDefault(t => t.Namespace != null && t.Namespace.StartsWith("System") && !t.Namespace.StartsWith("System.IO") && t.Name == className && t.IsPublic);
*/
/*
			var type = typeof(Path).Assembly.GetTypes().FirstOrDefault( t=> t.Namespace != null && t.Namespace.StartsWith("System") && t.Name == className && t.IsPublic);

			if (type == null)
			{
				Window.Terminal.Write($"Class '{className}' not found\n");
				return;
			}

			Window.Terminal.Write($"{type.Name}:\n");

			var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)
				.Where(m => !m.IsSpecialName)
				.OrderBy(m => m.Name)
				.ToList();

			if (methods.Count == 0)
			{
				Window.Terminal.Write($"- {type.Name} has no methods\n");
			}
			
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly )
				.OrderBy(p => p.Name)
				.ToList();
			
			if (properties.Count == 0)
			{
				Window.Terminal.Write($"- {type.Name} has no properties\n");
			}

			foreach (var prop in properties)
			{
				var propType = GetSimpleTypeName(prop.PropertyType);
				Window.Terminal.Write($"- {prop.Name}: {propType}\n");
			}

			foreach (var method in methods)
			{
				var parameters = string.Join(", ", method.GetParameters().Select(p =>
					$"{GetSimpleTypeName(p.ParameterType)} {p.Name}"));
				var returnType = GetSimpleTypeName(method.ReturnType);
				Window.Terminal.Write($"- {method.Name}({parameters}): {returnType}\n");
			}
		}
	}
	
	public static void Clear()
	{
		Window.Terminal.Clear();
	}

	public static void Reset()
	{
		Sandbox.Reset();
		Window.Terminal.Write("Sandbox state reset\n");
	}

	public static void Vars()
	{
		var vars = Sandbox.GetVariables().ToList();
		if (vars.Count == 0)
		{
			Window.Terminal.Write("No variables defined\n");
		}
		else
		{
			Window.Terminal.Write("Environment Variables:\n");
			foreach (var v in vars)
			{
				Window.Terminal.Write($"  {v.Name} ({v.Type.Name}) = {v.Value}\n");
			}
		}
	}
	
	//File operations
	public static void Read(string path)
	{
		Window.Terminal.Write(new Path(path).Read());
	}
	public static void Write(string path, string text)
	{
		new Path(path).Write(text);
	}

	public static void Execute(string path)
	{
		var result = Sandbox.Execute(new Path(path).Read());

		if (result.Success)
		{
			if (result.ReturnValue != null)
			{
				Terminal.WriteLine($"=> {result.ReturnValue}");
			}
		}
		else
		{
			Console.Error.WriteLine($"[Shell] Runtime/Compilation Error: {result.ErrorMessage}");
			Terminal.WriteLine($"[Shell] Runtime/Compilation Error: {result.ErrorMessage}");
		}
	}
	
	public static void List(string path)
	{
		//list files
	}

	public static void Touch(string path)
	{
		//initialize file
	}
	
	//Move()
	//Copy()
	
}
*/