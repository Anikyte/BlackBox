using Microsoft.Xna.Framework.Input;

namespace BlackBox.Machine;

public static class Shell
{
	private static string _inputBuffer = "";
	private static readonly List<string> _history = new();
	private static int _historyIndex = -1;
	private static int _offset;

	public static void ShowPrompt()
	{
		System.Terminal.Write("\n> ");
		_offset = System.Terminal.CursorX;
		System.Terminal.SetRow(_offset, new string(' ', System.Terminal.Width - _offset));
	}

	public static void ProcessInput()
	{
		int key = Input.GetCharPressed();

		while (key > 0)
		{
			if (key >= 32 && key <= 126)
			{
				char c = (char)key;
				int cursorPos = System.Terminal.CursorX - _offset;
				_inputBuffer = _inputBuffer.Insert(cursorPos, c.ToString());
				System.Terminal.CursorX = _offset;
				System.Terminal.SetRow(System.Terminal.CursorY, _inputBuffer + new string(' ', System.Terminal.Width - _offset - _inputBuffer.Length), _offset);
				System.Terminal.CursorX = _offset + cursorPos + 1;
			}
			key = Input.GetCharPressed();
		}

		if (Input.IsKeyPressed(Keys.Enter))
		{
			ExecuteLine();
		}
		else if (Input.IsKeyPressed(Keys.Back) || Input.IsKeyPressedRepeat(Keys.Back))
		{
			int cursorPos = System.Terminal.CursorX - _offset;
			if (cursorPos > 0 && _inputBuffer.Length > 0)
			{
				_inputBuffer = _inputBuffer.Remove(cursorPos - 1, 1);
				System.Terminal.CursorX = _offset;
				System.Terminal.SetRow(System.Terminal.CursorY, _inputBuffer + new string(' ', System.Terminal.Width - _offset - _inputBuffer.Length), _offset);
				System.Terminal.CursorX = _offset + cursorPos - 1;
			}
		}
		else if (Input.IsKeyPressed(Keys.Up) || Input.IsKeyPressedRepeat(Keys.Up))
		{
			NavigateHistory(-1);
		}
		else if (Input.IsKeyPressed(Keys.Down) || Input.IsKeyPressedRepeat(Keys.Down))
		{
			NavigateHistory(1);
		}
		else if (Input.IsKeyPressed(Keys.Left) || Input.IsKeyPressedRepeat(Keys.Left))
		{
			if (System.Terminal.CursorX > _offset)
				System.Terminal.CursorX--;
		}
		else if (Input.IsKeyPressed(Keys.Right) || Input.IsKeyPressedRepeat(Keys.Right))
		{
			if (System.Terminal.CursorX < _offset + _inputBuffer.Length)
				System.Terminal.CursorX++;
		}
	}

	private static void ExecuteLine()
	{
		System.Terminal.Write("\n");

		if (string.IsNullOrWhiteSpace(_inputBuffer))
		{
			ShowPrompt();
			return;
		}

		_history.Add(_inputBuffer);
		_historyIndex = _history.Count;

		string code = _inputBuffer.Trim();
		_inputBuffer = "";

		var result = Sandbox.Execute(code);

		if (result.Success)
		{
			if (result.ReturnValue != null)
				System.Terminal.WriteLine($"=> {result.ReturnValue}");
		}
		else
		{
			Console.Error.WriteLine($"[Shell] Runtime/Compilation Error: {result.ErrorMessage}");
			System.Terminal.WriteLine($"[Shell] Runtime/Compilation Error: {result.ErrorMessage}");
		}

		ShowPrompt();
	}

	private static void NavigateHistory(int direction)
	{
		if (_history.Count == 0) return;

		int newIndex = _historyIndex + direction;

		if (newIndex >= 0 && newIndex < _history.Count)
		{
			_historyIndex = newIndex;
			_inputBuffer = _history[_historyIndex];
			RedrawInputLine();
		}
		else if (newIndex >= _history.Count)
		{
			_historyIndex = _history.Count;
			_inputBuffer = "";
			RedrawInputLine();
		}
	}

	private static void RedrawInputLine()
	{
		System.Terminal.CursorX = _offset;
		System.Terminal.SetRow(System.Terminal.CursorY, _inputBuffer + new string(' ', System.Terminal.Width - _offset - _inputBuffer.Length), _offset);
		System.Terminal.CursorX = _offset + _inputBuffer.Length;
	}
}
