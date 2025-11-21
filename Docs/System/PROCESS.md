# Process

**Location:** `System/Process.cs`
**Namespace:** `BlackBox.System`
**Status:** 🚧 Placeholder

## Overview

Process provides userspace access to process management functions. It allows user code to spawn, monitor, and control processes without direct access to the Sandbox.

## Current Implementation

Currently a placeholder class:

```csharp
namespace BlackBox.System;

public class Process
{

}
```

## Planned API

### Process Creation

```csharp
public static class Process
{
    // Spawn process from code
    public static int Spawn(string code);

    // Spawn process from file
    public static int SpawnFile(string path);

    // Spawn with custom globals
    public static int Spawn(string code, object globals);
}
```

### Process Control

```csharp
public static class Process
{
    // Kill a process
    public static bool Kill(int pid);

    // Wait for process to complete
    public static ProcessResult Wait(int pid);

    // Check if process is alive
    public static bool IsAlive(int pid);

    // Get process state
    public static ProcessState GetState(int pid);
}
```

### Process Information

```csharp
public static class Process
{
    // List all process IDs
    public static IEnumerable<int> List();

    // Get current process ID
    public static int CurrentPid { get; }

    // Get process info
    public static ProcessInfo GetInfo(int pid);
}
```

### Inter-Process Communication

```csharp
public static class Process
{
    // Send message to process
    public static void Send(int pid, object message);

    // Receive message
    public static object? Receive();

    // Check for messages
    public static bool HasMessage();
}
```

## Usage Examples

### Spawn and Wait

```csharp
// Spawn a background task
int pid = Process.Spawn(@"
    for (int i = 0; i < 10; i++)
    {
        Serial.Write($""Task {i}\n"");
        Thread.Sleep(1000);
    }
    return ""Done"";
");

// Wait for it to complete
var result = Process.Wait(pid);
Serial.Write($"Result: {result.Value}\n");
```

### Process Management

```csharp
// List all processes
foreach (var pid in Process.List())
{
    var info = Process.GetInfo(pid);
    Serial.Write($"PID {pid}: {info.State}\n");
}

// Kill a process
if (Process.IsAlive(5))
{
    Process.Kill(5);
    Serial.Write("Process 5 terminated\n");
}
```

### File Execution

```csharp
// Run a script file
int pid = Process.SpawnFile("/programs/task.cs");

// Monitor until complete
while (Process.IsAlive(pid))
{
    Thread.Sleep(100);
}

Serial.Write("Task completed\n");
```

### Inter-Process Communication

```csharp
// Process 1: Send message
Process.Send(2, "Hello from process 1");

// Process 2: Receive message
if (Process.HasMessage())
{
    var msg = Process.Receive();
    Serial.Write($"Received: {msg}\n");
}
```

## Planned Types

### ProcessResult

```csharp
public class ProcessResult
{
    public int Pid { get; set; }
    public bool Success { get; set; }
    public object? Value { get; set; }
    public string? Error { get; set; }
}
```

### ProcessInfo

```csharp
public class ProcessInfo
{
    public int Pid { get; set; }
    public ProcessState State { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Name { get; set; }
}
```

### ProcessState

```csharp
public enum ProcessState
{
    Starting,
    Running,
    Sleeping,
    Waiting,
    Exited
}
```

## Integration with Sandbox

Process functions are wrappers around Sandbox subprocess management:

```csharp
// Process.Spawn() internally calls:
Sandbox.Spawn(code);

// Process.Kill() internally calls:
Sandbox.Kill(pid);

// Process.Wait() internally calls:
Sandbox.Wait(pid);
```

This abstraction provides:
- Cleaner API for userspace
- Additional safety checks
- Process metadata management
- IPC infrastructure

## PID 0 - The Sandbox Itself

PID 0 is reserved for the Sandbox/init process. User processes start at PID 1.

```csharp
var currentPid = Process.CurrentPid;
// Returns: Current process ID (0 for main thread, 1+ for subprocesses)
```

## See Also

- [Sandbox](../Machine/SANDBOX.md) - Underlying subprocess implementation
- [System Layer](SYSTEM.md) - Overview of userspace APIs
- [IO](IO.md) - Input/output operations
