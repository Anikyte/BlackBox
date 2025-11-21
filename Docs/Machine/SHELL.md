# Shell

**Location:** `Machine/Shell.cs`
**Type:** Static class
**Status:** 🚧 Placeholder

## Overview

The Shell is the primary user interface running within the Sandbox (PID 0). It provides command parsing, execution, and user interaction for the hostspace environment.

## Current Implementation

Currently a placeholder class:

```csharp
public class Shell
{

}
```

## Planned Architecture

The Shell will be a static class that provides the interactive command interface.

### Responsibilities

- **Command Parsing** - Parse user input into commands and arguments
- **Command Execution** - Execute built-in commands and user programs
- **History Management** - Track command history
- **Environment Variables** - Manage shell environment
- **Built-in Commands** - Provide shell utilities

## Planned API

### Core Functions

```csharp
public static class Shell
{
    // Initialize shell environment
    static Shell()
    {
        // Set up default environment
        // Register built-in commands
    }

    // Process single line of input
    public static async Task<ShellResult> ProcessInput(string input)
    {
        // Parse command
        // Execute and return result
    }

    // Interactive REPL loop
    public static void Run()
    {
        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();

            if (input == "exit") break;

            var result = ProcessInput(input).Result;
            if (result.Success)
                Console.WriteLine(result.Output);
            else
                Console.Error.WriteLine(result.Error);
        }
    }

    // Command history
    public static IEnumerable<string> GetHistory();
    public static void AddHistory(string command);

    // Environment variables
    public static void SetEnv(string name, string value);
    public static string? GetEnv(string name);

    // Register custom command
    public static void RegisterCommand(string name, Func<string[], Task<ShellResult>> handler);
}
```

## Built-in Commands (Planned)

### Process Management
- `ps` - List running processes
- `kill <pid>` - Terminate a process
- `spawn <file>` - Spawn a new process

### File Operations
- `ls [path]` - List directory contents
- `cd <path>` - Change directory
- `cat <file>` - Display file contents
- `pwd` - Print working directory

### System Information
- `help` - Display help information
- `clear` - Clear screen
- `echo <text>` - Print text
- `env` - Show environment variables

### Execution
- `run <file>` - Execute a script file
- `eval <code>` - Evaluate C# code

## Integration with Sandbox

The Shell runs on the Sandbox's main thread:

```csharp
// In Host initialization
Sandbox.Run(() =>
{
    // Shell processes input in the execution loop
    if (Console.KeyAvailable)
    {
        var input = Console.ReadLine();
        var result = Shell.ProcessInput(input).Result;
        Console.WriteLine(result.Output);
    }
});
```

## Example Usage

### Interactive Session
```
> var x = 10
> x + 5
15
> spawn program.cs
Process spawned with PID 1
> ps
PID   STATE      START TIME
0     Running    00:00:00
1     Running    00:01:23
> kill 1
Process 1 terminated
> exit
```

## Planned Types

### ShellResult
```csharp
public class ShellResult
{
    public bool Success { get; set; }
    public string? Output { get; set; }
    public string? Error { get; set; }
}
```

### Command
```csharp
public class Command
{
    public string Name { get; set; }
    public string[] Args { get; set; }
    public Dictionary<string, string> Options { get; set; }
}
```

## See Also

- [Sandbox](SANDBOX.md) - Execution environment for the Shell
- [Host](HOST.md) - Manages the overall system
- [IO](../System/IO.md) - Userspace shell functions
