# IO

**Location:** `System/IO.cs`
**Namespace:** `BlackBox.System`
**Status:** 🚧 Placeholder

## Overview

IO provides userspace shell functions and higher-level I/O operations. Unlike Serial (which is low-level console access), IO provides structured input/output utilities.

## Current Implementation

Currently a placeholder class:

```csharp
namespace BlackBox.System;

public class IO
{

}
```

## Planned API

### Input Operations

```csharp
public static class IO
{
    // Read a line of input
    public static string? ReadLine();

    // Read a line with prompt
    public static string? ReadLine(string prompt);

    // Read a character
    public static char ReadChar();

    // Read with timeout
    public static string? ReadLine(TimeSpan timeout);

    // Check if input is available
    public static bool HasInput();
}
```

### Output Operations

```csharp
public static class IO
{
    // Write line with automatic newline
    public static void WriteLine(string text);

    // Write formatted text
    public static void Write(string format, params object[] args);
    public static void WriteLine(string format, params object[] args);

    // Write to error stream
    public static void WriteError(string text);
}
```

### File-like Operations

```csharp
public static class IO
{
    // Read entire file
    public static string ReadAllText(string path);

    // Write entire file
    public static void WriteAllText(string path, string content);

    // Read lines
    public static IEnumerable<string> ReadLines(string path);

    // Append to file
    public static void AppendAllText(string path, string content);
}
```

### Console Control

```csharp
public static class IO
{
    // Clear screen
    public static void Clear();

    // Set cursor position
    public static void SetCursorPosition(int left, int top);

    // Set text color
    public static void SetForeground(ConsoleColor color);
    public static void SetBackground(ConsoleColor color);

    // Get console dimensions
    public static int Width { get; }
    public static int Height { get; }
}
```

## Usage Examples

### Interactive Input
```csharp
var name = IO.ReadLine("Enter your name: ");
IO.WriteLine($"Hello, {name}!");
```

### File Operations
```csharp
// Read file
var content = IO.ReadAllText("/data/config.txt");

// Process lines
foreach (var line in IO.ReadLines("/data/log.txt"))
{
    IO.WriteLine(line);
}

// Write file
IO.WriteAllText("/data/output.txt", "Results");
```

### Formatted Output
```csharp
var x = 42;
var name = "test";
IO.WriteLine("Value: {0}, Name: {1}", x, name);
```

### Console Control
```csharp
IO.Clear();
IO.SetForeground(ConsoleColor.Green);
IO.WriteLine("Success!");
IO.SetForeground(ConsoleColor.Red);
IO.WriteError("Error occurred");
```

## Differences from Serial

| Feature | Serial | IO |
|---------|--------|-----|
| Level | Low-level | High-level |
| Purpose | Debugging output | User interaction |
| Formatting | Manual | Built-in |
| Input | Limited | Full support |
| Buffering | Simple | Structured |
| File Operations | No | Yes |

## Integration with Filesystem

IO operations that involve files will integrate with the virtual Filesystem:

```csharp
// IO.ReadAllText() calls Filesystem.Read() internally
var content = IO.ReadAllText("/data/file.txt");

// Equivalent to:
var content = Filesystem.Read("/data/file.txt");
```

## See Also

- [Serial](SERIAL.md) - Low-level console access
- [Filesystem](Filesystem/FILESYSTEM.md) - File system operations
- [System Layer](SYSTEM.md) - Overview of userspace APIs
