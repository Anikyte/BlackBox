using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BlackBox;

public static class Input
{
	private static KeyboardState _currentKeyState;
	private static KeyboardState _previousKeyState;
	private static readonly Queue<char> _charQueue = new();
	private static readonly Dictionary<Keys, double> _keyRepeatTimers = new();
	private static double _elapsedTime;

	private const double RepeatDelay = 0.5;
	private const double RepeatRate = 0.03;

	public static void Initialize(GameWindow window) => window.TextInput += OnTextInput;

	private static void OnTextInput(object? sender, TextInputEventArgs e)
	{
		if (e.Character >= 32 && e.Character <= 126)
			_charQueue.Enqueue(e.Character);
	}

	public static void Update(GameTime gameTime)
	{
		_previousKeyState = _currentKeyState;
		_currentKeyState = Keyboard.GetState();
		_elapsedTime = gameTime.ElapsedGameTime.TotalSeconds;
	}

	public static int GetCharPressed() => _charQueue.Count > 0 ? _charQueue.Dequeue() : 0;

	public static bool IsKeyPressed(Keys key) =>
		_currentKeyState.IsKeyDown(key) && _previousKeyState.IsKeyUp(key);

	public static bool IsKeyDown(Keys key) => _currentKeyState.IsKeyDown(key);

	public static bool IsKeyPressedRepeat(Keys key)
	{
		bool isDown = _currentKeyState.IsKeyDown(key);
		bool wasDown = _previousKeyState.IsKeyDown(key);

		if (isDown && !wasDown)
		{
			_keyRepeatTimers[key] = 0;
			return true;
		}

		if (isDown && wasDown)
		{
			if (!_keyRepeatTimers.ContainsKey(key))
				_keyRepeatTimers[key] = 0;

			_keyRepeatTimers[key] += _elapsedTime;

			if (_keyRepeatTimers[key] >= RepeatDelay)
			{
				double timeInRepeat = _keyRepeatTimers[key] - RepeatDelay;
				if (timeInRepeat >= RepeatRate)
				{
					_keyRepeatTimers[key] = RepeatDelay;
					return true;
				}
			}
		}

		if (!isDown && _keyRepeatTimers.ContainsKey(key))
			_keyRepeatTimers.Remove(key);

		return false;
	}
}