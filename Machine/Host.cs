namespace BlackBox.Machine;

//this will update peripherals, filesystem, etc
//this will also spawn the Sandbox loop (PID 0)
//will be the main program loop

public static class Host
{
	private static bool _initialized = false;

	static Host()
	{
		// Tests run later after Window is initialized
	}

	public static void Loop()
	{
		// Run initialization tests once
		if (!_initialized)
		{
			_initialized = true;

			// Test 1 - execute a simple command
			var result = Sandbox.Execute("var x = 10; return x * 2;").Result;
			if (result.Success)
			{
				Console.Write($"Test result: {result.ReturnValue}\n");
			}

			// Test 2 - use Terminal.Write() from userspace to write to window
			var result2 = Sandbox.Execute("Console.Write(\"Hello from userspace!\\n\"); return true;").Result;
			if (!result2.Success)
			{
				Console.Write($"Terminal test failed: {result2.ErrorMessage}\n");
			}
			
			Window.Write("Hello World!");
		}

		//check sandbox status and error/crash handling
		//update timed events
		//update peripherals/filesystem
		//update shell
		//update serial
		//Sandbox.Run() - now uses continuous execution loop
	}
}