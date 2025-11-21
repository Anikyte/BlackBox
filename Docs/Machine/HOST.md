# Host

**Location:** `Machine/Host.cs`
**Type:** Static class
**Status:** 🚧 Placeholder (basic test code only)

## Overview

The Host is the main program loop that manages the entire emulated system. It spawns the Sandbox loop (PID 0) and coordinates all system updates.

## Responsibilities

- **Spawn Sandbox Loop** - Initialize and manage the Sandbox (PID 0)
- **Update Peripherals** - Manage peripheral device state
- **Update Filesystem** - Handle virtual filesystem operations
- **Status Monitoring** - Check sandbox status and handle errors/crashes
- **Timed Events** - Manage scheduled events and timers
- **Serial Updates** - Handle serial console I/O

## Current Implementation

Currently contains only basic test code:

```csharp
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
```

## Planned Architecture

### Static Constructor
- Initialize system components
- Spawn Sandbox with initial configuration
- Set up peripherals and filesystem

### Loop() Method
The main execution loop called repeatedly:

```csharp
public static void Loop()
{
    // Check sandbox status
    if (!Sandbox.IsRunning)
    {
        // Handle crash/stop
    }

    // Update timed events
    TimedEvents.Update();

    // Update peripherals
    Peripherals.Update();

    // Update filesystem
    Filesystem.Update();

    // Update shell
    Shell.Update();

    // Update serial
    Serial.Update();
}
```

## Integration with Sandbox

The Host manages the Sandbox lifecycle:

```csharp
// Start sandbox with shell
Sandbox.Run(() =>
{
    Shell.ProcessInput();
});

// Monitor in Host.Loop()
if (!Sandbox.IsRunning)
{
    // Handle shutdown or error
}
```

## See Also

- [Sandbox](SANDBOX.md) - The execution environment managed by Host
- [Shell](SHELL.md) - The user interface running in Sandbox
- [Machine Layer Overview](MACHINE.md)
