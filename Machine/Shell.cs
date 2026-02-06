using Microsoft.Xna.Framework.Input;

namespace BlackBox.Machine;

//defines the shell and contains shell functions for hostspace

public static class Shell
{
	private static string inputBuffer = "";
	private static readonly List<string> History = new();
	private static int historyIndex = -1;
	private static int offset;

	public static void ShowPrompt()
	{
		Window.Terminal.Write("\n> ");
		offset =  Window.Terminal.CursorX;
		Window.Terminal.Write(new string(' ', Window.Terminal.Width - offset));
		Window.Terminal.CursorX = offset;
	}

	public static void ProcessInput()
	{
		int key = Window.Terminal.GetCharPressed();

		while (key > 0)
		{
			if (key >= 32 && key <= 126)
			{
				char c = (char)key;
				int cursorPos =  Window.Terminal.CursorX - offset;
				inputBuffer = inputBuffer.Insert(cursorPos, c.ToString());
				Window.Terminal.CursorX = offset;
				Window.Terminal.Write(inputBuffer + new string(' ', Window.Terminal.Width - offset - inputBuffer.Length));
				Window.Terminal.CursorX = offset + cursorPos + 1;
			}

			key = Window.Terminal.GetCharPressed();
		}

		if (Window.Terminal.IsKeyPressed(Keys.Enter))
		{
			ExecuteLine();
		}
		else if (Window.Terminal.IsKeyPressed(Keys.Back) || Window.Terminal.IsKeyPressedRepeat(Keys.Back))
		{
			int cursorPos =  Window.Terminal.CursorX - offset;
			if (cursorPos > 0 && inputBuffer.Length > 0)
			{
				inputBuffer = inputBuffer.Remove(cursorPos - 1, 1);
				Window.Terminal.CursorX = offset;
				Window.Terminal.Write(inputBuffer + new string(' ', Window.Terminal.Width - offset - inputBuffer.Length));
				Window.Terminal.CursorX = offset + cursorPos - 1;
			}
		}
		else if (Window.Terminal.IsKeyPressed(Keys.Up) || Window.Terminal.IsKeyPressedRepeat(Keys.Up))
		{
			NavigateHistory(-1);
		}
		else if (Window.Terminal.IsKeyPressed(Keys.Down) || Window.Terminal.IsKeyPressedRepeat(Keys.Down))
		{
			NavigateHistory(1);
		}
		else if (Window.Terminal.IsKeyPressed(Keys.Left) || Window.Terminal.IsKeyPressedRepeat(Keys.Left))
		{
			if (Window.Terminal.CursorX > offset)
			{
				Window.Terminal.CursorX--;
			}
		}
		else if (Window.Terminal.IsKeyPressed(Keys.Right) || Window.Terminal.IsKeyPressedRepeat(Keys.Right))
		{
			if (Window.Terminal.CursorX < offset + inputBuffer.Length)
			{
				Window.Terminal.CursorX++;
			}
		}
	}

	private static void ExecuteLine()
	{
		Window.Terminal.Write("\n");

		if (string.IsNullOrWhiteSpace(inputBuffer))
		{
			ShowPrompt();
			return;
		}

		History.Add(inputBuffer);
		historyIndex = History.Count;

		string code = inputBuffer.Trim();
		inputBuffer = "";

		var result = Sandbox.Execute(code);

		if (result.Success)
		{
			if (result.ReturnValue != null)
			{
				Window.Terminal.Write($"=> {result.ReturnValue}\n");
			}
		}
		else
		{
			Window.Terminal.Write($"Error: {result.ErrorMessage}\n");
		}

		ShowPrompt();
	}

	private static void NavigateHistory(int direction)
	{
		if (History.Count == 0) return;

		int newIndex = historyIndex + direction;

		if (newIndex >= 0 && newIndex < History.Count)
		{
			historyIndex = newIndex;
			inputBuffer = History[historyIndex];
			RedrawInputLine();
		}
		else if (newIndex >= History.Count)
		{
			historyIndex = History.Count;
			inputBuffer = "";
			RedrawInputLine();
		}
	}

	private static void RedrawInputLine()
	{
		Window.Terminal.CursorX = offset;
		Window.Terminal.Write(inputBuffer + new string(' ', Window.Terminal.Width - offset - inputBuffer.Length));
		Window.Terminal.CursorX = offset + inputBuffer.Length;
	}
}
