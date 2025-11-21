namespace BlackBox.Machine;

//this will update peripherals, filesystem, etc
//this will also spawn the Sandbox loop (PID 0)
//will be the main program loop

public static class Host
{
	static Host()
	{
		// Test 1 - execute a simple command
		var result = Sandbox.Execute("var x = 10; return x * 2;").Result;
		if (result.Success)
		{
			Console.WriteLine($"Test result: {result.ReturnValue}");
		}

		// Test 2 - use Serial.Write() from userspace
		var result2 = Sandbox.Execute("Serial.Write(\"Hello from userspace!\\n\"); return true;").Result;
		if (!result2.Success)
		{
			Console.WriteLine($"Serial test failed: {result2.ErrorMessage}");
		}
	}

	public static void Loop()
	{
		//check sandbox status and error/crash handling
		//update timed events
		//update peripherals/filesystem
		//update shell
		//update serial
		//Sandbox.Run() - now uses continuous execution loop
	}
}