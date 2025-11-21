# Sandbox

**Location:** `Machine/Sandbox.cs`
**Type:** Static class
**Status:** ✅ Complete

## Overview

The Sandbox is the Roslyn-based C# script execution engine running as PID 0 (the init process). It provides the border between hostspace and userspace, executing user code in a secure, sandboxed environment.

## Architecture

The Sandbox provides two execution modes:

1. **Main Thread Execution** - For the interactive shell, with persistent state
2. **Subprocess Execution** - For user-initiated programs on separate threads

## Main Execution Loop

The Sandbox runs continuously as fast as possible, executing a custom action on each iteration.

### Functions

| Function | Description |
|----------|-------------|
| `Run(Action loopAction)` | Starts execution loop with custom action |
| `Run()` | Starts empty execution loop |
| `Stop()` | Stops the execution loop |
| `WaitForStop()` | Async wait for loop to finish |
| `IsRunning` | Property indicating if sandbox is running |

### Example

```csharp
// Start with custom loop action
Sandbox.Run(() =>
{
    // This runs as fast as possible
    ProcessShellInput();
    UpdateTimers();
});

// Stop when done
Sandbox.Stop();
await Sandbox.WaitForStop();
```

## Direct Code Execution

Execute code on the main thread with persistent state. Variables and definitions persist between calls.

### Functions

| Function | Returns | Description |
|----------|---------|-------------|
| `Execute(string code, object? globals, CancellationToken)` | `Task<ScriptExecutionResult>` | Executes C# code in the sandbox |
| `ExecuteFile(string path, object? globals, CancellationToken)` | `Task<ScriptExecutionResult>` | Executes code from a file |
| `Evaluate<T>(string expr, object? globals, CancellationToken)` | `Task<T?>` | Evaluates an expression and returns typed result |
| `Reset()` | `void` | Clears all variables and script state |
| `GetVariables()` | `IEnumerable<ScriptVariable>` | Lists all defined variables |

### Example

```csharp
// Execute with persistent state
await Sandbox.Execute("var count = 0;");
await Sandbox.Execute("count++;");
var result = await Sandbox.Execute("count");  // Returns 1

// Get all variables
foreach (var v in Sandbox.GetVariables())
{
    Console.WriteLine($"{v.Name}: {v.Type.Name} = {v.Value}");
}

// Clear state
Sandbox.Reset();
```

### Globals

You can pass global objects that are accessible from userspace:

```csharp
public class Globals
{
    public Serial Serial { get; set; }
    public CustomAPI API { get; set; }
}

var globals = new Globals { Serial = new Serial() };
await Sandbox.Execute("Serial.Write(\"Hello\");", globals);
```

## Subprocess Management

Spawn user programs on separate threads. Subprocesses **automatically terminate when execution completes**.

### Functions

| Function | Returns | Description |
|----------|---------|-------------|
| `Spawn(string code, object? globals)` | `int` | Spawns subprocess, returns PID (-1 on failure) |
| `SpawnFile(string path, object? globals)` | `Task<int>` | Spawns subprocess from file |
| `Kill(int pid)` | `bool` | Terminates subprocess by PID |
| `Status(int pid)` | `ProcessStatus?` | Gets subprocess state, result, and timing |
| `ListPids()` | `IEnumerable<int>` | Lists all active subprocess PIDs |
| `Wait(int pid)` | `Task<ScriptExecutionResult?>` | Waits for subprocess to complete |

### Process States

| State | Description |
|-------|-------------|
| `Starting` | Process is initializing |
| `Running` | Process is executing |
| `Exited` | Process has completed or been killed |

### Example

```csharp
// Spawn a background task
int pid = Sandbox.Spawn(@"
    for (int i = 0; i < 100; i++)
    {
        Console.WriteLine($""Progress: {i}%"");
        Thread.Sleep(100);
    }
    return ""Done"";
");

// Check status
var status = Sandbox.Status(pid);
Console.WriteLine($"PID {pid}: {status.State}");

// Wait for completion (optional)
var result = await Sandbox.Wait(pid);
Console.WriteLine($"Result: {result.ReturnValue}");

// Or kill early
Sandbox.Kill(pid);

// List all processes
foreach (var p in Sandbox.ListPids())
{
    Console.WriteLine($"Active PID: {p}");
}
```

## Configuration

### Assembly References and Imports

| Function | Description |
|----------|-------------|
| `AddReferences(params Assembly[])` | Add assembly references |
| `AddReferences(params Type[])` | Add assemblies by type |
| `AddImports(params string[])` | Add namespace imports |

**Example:**
```csharp
// Add custom assemblies
Sandbox.AddReferences(typeof(HttpClient).Assembly);
Sandbox.AddImports("System.Net.Http");
```

### Default Configuration

**Default References:**
- `mscorlib` (System.Object)
- `System.Console`
- `System.Collections.Generic`
- `System.Linq`
- `BlackBox.System` (All userspace API classes)

**Default Imports:**
- `System`
- `System.Collections.Generic`
- `System.Linq`
- `System.Text`
- `BlackBox.System`

### Timeouts

```csharp
// Create timeout token
var cts = Sandbox.CreateTimeoutToken(TimeSpan.FromSeconds(5));
await Sandbox.Execute("while(true) { }", cancellationToken: cts.Token);
```

## Security Features

The Sandbox enforces strict security restrictions:

- **No unsafe code** - The `unsafe` keyword is disabled
- **Overflow checking** - Arithmetic overflow checking enabled
- **Limited assemblies** - Only approved system libraries available
- **Isolated execution** - Subprocesses run in separate threads with independent state
- **No file system access** - User code cannot directly access host filesystem
- **Controlled APIs** - Only BlackBox.System namespace is accessible

## API Types

### ScriptExecutionResult
```csharp
public class ScriptExecutionResult
{
    public bool Success { get; set; }
    public object? ReturnValue { get; set; }
    public Exception? Exception { get; set; }
    public string? ErrorMessage { get; set; }
}
```

### ProcessStatus
```csharp
public class ProcessStatus
{
    public int Pid { get; set; }
    public ProcessState State { get; set; }
    public ScriptExecutionResult? Result { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}
```

### ScriptVariable
```csharp
public class ScriptVariable
{
    public string Name { get; set; }
    public Type Type { get; set; }
    public object? Value { get; set; }
}
```

## Implementation Details

### Static Fields
- `_scriptOptions` - Roslyn script configuration
- `_currentState` - Current script state (for main thread)
- `_running` - Execution loop status
- `_loopTask` - Background task for execution loop
- `_processes` - Subprocess registry (ConcurrentDictionary)
- `_nextPid` - Next available PID (starts at 1, PID 0 is Sandbox itself)

### Thread Safety
- Main thread execution uses locks (`_stateLock`)
- Subprocess management uses concurrent collections
- Automatic cleanup of finished processes

## See Also

- [Host](HOST.md) - Manages the Sandbox lifecycle
- [Shell](SHELL.md) - Runs in the Sandbox on the main thread
- [System APIs](../System/SYSTEM.md) - APIs accessible from userspace
