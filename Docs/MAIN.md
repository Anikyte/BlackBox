# BlackBox Documentation

A C# scripting sandbox environment built on Microsoft Roslyn, designed as a virtual machine with separation between userspace (sandboxed code) and hostspace (host system).

---

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Sandbox - Code Execution Engine](#sandbox---code-execution-engine)
- [Usage Patterns](#usage-patterns)
- [API Reference](#api-reference)
- [Security Features](#security-features)
- [Development Status](#development-status)

---

## Architecture Overview

### Terminology

| Term | Location | Description |
|------|----------|-------------|
| **Host Loop/Host** | `Machine/Host.cs` | Manages the emulated system, peripherals, and filesystem |
| **Sandbox Loop** | `Machine/Sandbox.cs` | Interactive shell loop running as PID 0 (init process) |
| **Shell** | `Machine/Shell.cs` | Primary user interface, part of PID 0/Sandbox Loop |
| **Userspace** | `System/*` | Code and APIs accessible from within the emulated system |
| **Hostspace** | `Machine/*` | Host-managed code inaccessible from userspace |

### Directory Structure

```
BlackBox/
├── Docs/                   # Documentation
│   ├── MAIN.md             # This file - comprehensive documentation
│   ├── Machine/            # Machine layer documentation
│   ├── System/             # System layer documentation
│   └── Files/              # Files documentation
├── Files/                  # Default filesystem (copied to virtual filesystem at compilation)
│   └── Userspace/          # User-accessible files
├── Machine/                # Host-managed code (inaccessible from userspace)
│   ├── Host.cs             # Main host loop
│   ├── Sandbox.cs          # Roslyn scripting sandbox (PID 0)
│   └── Shell.cs            # User interface shell
└── System/                 # Userspace-accessible APIs and syscalls
    ├── Filesystem/         # Virtual filesystem
    ├── Peripherals/        # Device interfaces
    ├── IO.cs               # Userspace shell functions
    ├── Process.cs          # Process management
    └── Serial.cs           # Serial console
```

### Component Documentation

- **[Machine Layer](Machine/MACHINE.md)** - Host loop, Sandbox, and Shell
  - [Host](Machine/HOST.md) - Main program loop
  - [Sandbox](Machine/SANDBOX.md) - Roslyn scripting environment
  - [Shell](Machine/SHELL.md) - User interface
- **[System Layer](System/SYSTEM.md)** - Userspace APIs
  - [Filesystem](System/Filesystem/FILESYSTEM.md) - Virtual filesystem
  - [Peripherals](System/Peripherals/PERIPHERALS.md) - Device interfaces
  - [IO](System/IO.md) - Shell functions
  - [Process](System/PROCESS.md) - Process management
  - [Serial](System/SERIAL.md) - Serial console
- **[Files](Files/FILES.md)** - Default filesystem structure

---

## Sandbox - Code Execution Engine

The `Sandbox` class (`Machine/Sandbox.cs`) is the core execution environment, providing both main-thread execution (for the shell) and subprocess management (for user programs). `Sandbox` is a static class - there is only one instance.

**See [Machine/SANDBOX.md](Machine/SANDBOX.md) for complete documentation.**

### Main Execution Loop

The sandbox runs as PID 0 with a continuous execution loop that runs as fast as possible.

```csharp
// Run with custom loop action
Sandbox.Run(() =>
{
    // Your per-iteration logic here
    // Runs as fast as possible
});

// Or run empty loop
Sandbox.Run();

// Stop when done
Sandbox.Stop();
await Sandbox.WaitForStop();
```

#### Execution Loop Functions

| Function | Description |
|----------|-------------|
| `Run(Action loopAction)` | Starts execution loop with custom action, runs continuously as fast as possible |
| `Run()` | Starts empty execution loop |
| `Stop()` | Stops the execution loop |
| `WaitForStop()` | Async wait for loop to finish |
| `IsRunning` | Property indicating if sandbox is running |

### Direct Code Execution (Main Thread)

Execute code directly on the sandbox's main thread, maintaining state between calls. This is used for the interactive shell.

| Function | Returns | Description |
|----------|---------|-------------|
| `Execute(string code, ...)` | `Task<ScriptExecutionResult>` | Executes C# code in the sandbox |
| `ExecuteFile(string path, ...)` | `Task<ScriptExecutionResult>` | Executes code from a file |
| `Evaluate<T>(string expr, ...)` | `Task<T?>` | Evaluates an expression and returns typed result |
| `Reset()` | `void` | Clears all variables and script state |
| `GetVariables()` | `IEnumerable<ScriptVariable>` | Lists all defined variables |

**Example:**
```csharp
// Interactive shell usage
await Sandbox.Execute("var x = 10;");
await Sandbox.Execute("var y = x + 5;");
var result = await Sandbox.Execute("y * 2");  // Returns 30

// Variables persist across calls
var vars = Sandbox.GetVariables();
foreach (var v in vars)
{
    Console.WriteLine($"{v.Name}: {v.Type.Name} = {v.Value}");
}

// Clear state
Sandbox.Reset();
```

### Subprocess Management (Separate Threads)

Spawn and manage user-initiated programs that run on separate threads. **Subprocesses automatically terminate when execution completes.**

| Function | Returns | Description |
|----------|---------|-------------|
| `Spawn(string code, ...)` | `int` | Spawns subprocess on separate thread, returns PID (-1 on failure) |
| `SpawnFile(string path, ...)` | `Task<int>` | Spawns subprocess from file |
| `Kill(int pid)` | `bool` | Terminates subprocess by PID |
| `Status(int pid)` | `ProcessStatus?` | Gets subprocess state, result, and timing |
| `ListPids()` | `IEnumerable<int>` | Lists all active subprocess PIDs |
| `Wait(int pid)` | `Task<ScriptExecutionResult?>` | Waits for subprocess to complete |

**Example:**
```csharp
// Spawn a background task (automatically terminates when done)
int pid = Sandbox.Spawn(@"
    for (int i = 0; i < 100; i++)
    {
        Console.WriteLine($""Progress: {i}%"");
        Thread.Sleep(100);
    }
    return ""Complete"";
");

// Check status
var status = Sandbox.Status(pid);
Console.WriteLine($"PID {status.Pid}: {status.State}");

// Wait for completion
var result = await Sandbox.Wait(pid);
if (result.Success)
{
    Console.WriteLine($"Result: {result.ReturnValue}");
}

// Or kill it early if needed
Sandbox.Kill(pid);
```

### Process States

| State | Description |
|-------|-------------|
| `Starting` | Process is initializing |
| `Running` | Process is executing |
| `Exited` | Process has completed or been killed |

### Configuration

| Function | Description |
|----------|-------------|
| `AddReferences(params Assembly[])` | Add assembly references to sandbox |
| `AddReferences(params Type[])` | Add assemblies by type |
| `AddImports(params string[])` | Add namespace imports |
| `CreateTimeoutToken(TimeSpan)` | Create cancellation token with timeout |

**Example:**
```csharp
// Add custom assemblies and namespaces
Sandbox.AddReferences(typeof(System.Net.Http.HttpClient).Assembly);
Sandbox.AddImports("System.Net.Http", "System.Threading.Tasks");

// Execute with timeout
var cts = Sandbox.CreateTimeoutToken(TimeSpan.FromSeconds(5));
await Sandbox.Execute("while(true) { }", cancellationToken: cts.Token);
```

---

## Usage Patterns

### Pattern 1: Interactive Shell
```csharp
bool shellRunning = true;

// Run shell on main execution loop
Sandbox.Run(() =>
{
    if (shellRunning)
    {
        Console.Write("> ");
        string? input = Console.ReadLine();

        if (input == "exit")
        {
            shellRunning = false;
            Sandbox.Stop();
            return;
        }

        var result = Sandbox.Execute(input).Result;
        if (result.Success)
            Console.WriteLine(result.ReturnValue);
        else
            Console.WriteLine($"Error: {result.ErrorMessage}");
    }
});

await Sandbox.WaitForStop();
```

### Pattern 2: Background Task Management
```csharp
// Start multiple background tasks (automatically terminate when done)
var pids = new List<int>
{
    Sandbox.Spawn("/* task 1 code */"),
    Sandbox.Spawn("/* task 2 code */"),
    Sandbox.Spawn("/* task 3 code */")
};

// Wait for all to complete
var tasks = pids.Select(pid => Sandbox.Wait(pid));
var results = await Task.WhenAll(tasks);

foreach (var result in results)
{
    if (result?.Success == true)
        Console.WriteLine($"Task completed: {result.ReturnValue}");
}
```

### Pattern 3: Continuous Execution Loop
```csharp
int iterations = 0;

Sandbox.Run(() =>
{
    iterations++;

    // Runs as fast as possible
    if (iterations % 100000 == 0)
        Console.WriteLine($"Iterations: {iterations}");

    // Custom per-loop logic here
});

// Stop after some condition
await Task.Delay(5000);
Sandbox.Stop();
await Sandbox.WaitForStop();

Console.WriteLine($"Total iterations in 5 seconds: {iterations}");
```

---

## API Reference

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

---

## Security Features

The sandbox enforces security restrictions:

- **No unsafe code** - `unsafe` keyword is disabled
- **Overflow checking** - Arithmetic overflow checking enabled
- **Limited assemblies** - Only safe system libraries are available by default
- **Isolated execution** - Subprocesses run in separate threads with independent state

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

---

## Development Status

| Component | Status |
|-----------|--------|
| Sandbox (Code Execution) | ✅ Complete |
| Main Loop System | ✅ Complete |
| Subprocess Management | ✅ Complete |
| Host Loop | 🚧 Placeholder |
| Shell | 🚧 Placeholder |
| Virtual Filesystem | 🚧 Placeholder |
| Process Manager | 🚧 Placeholder |
| Peripherals | 🚧 Placeholder |
| IO System | 🚧 Placeholder |
| Serial Console | ✅ Complete |

---

## Requirements

- **.NET 9.0**
- **Microsoft.CodeAnalysis.CSharp.Scripting 4.12.0**
