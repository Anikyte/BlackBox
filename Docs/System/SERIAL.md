# Serial

**Location:** `System/Serial.cs`
**Namespace:** `BlackBox.System`
**Status:** ✅ Complete

## Overview

Serial provides low-level console I/O for debugging. It writes to stdout (not the terminal window) and maintains a buffer. Use this for debugging output that should go to the console rather than the graphical terminal.

## Implementation

```csharp
namespace BlackBox.System;

public static class Serial
{
    public static string ConsoleBuffer = "";

    public static void Write(string s)
    {
        Console.Write(s);
        ConsoleBuffer += s;
    }

    public static string Read(string s)
    {
        return ConsoleBuffer;
    }
}
```

## API

### Write(string)

Writes a string to stdout for debugging.

**Parameters:**
- `s` - String to write

**Example:**
```csharp
Serial.Write("Debug: x = 42\n");
Serial.Write($"Value: {x}\n");
```

### Read(string)

Returns the current console buffer.

**Parameters:**
- `s` - Unused parameter (legacy)

**Returns:** Current console buffer contents

**Example:**
```csharp
var buffer = Serial.Read("");
```

### ConsoleBuffer

Public field storing console buffer contents.

**Type:** `string`

**Example:**
```csharp
var content = Serial.ConsoleBuffer;
```

## Serial vs Terminal

**Important distinction:**

- **`Serial.Write()`** - Writes to stdout (for debugging, logging, console output)
- **`Terminal.Write()`** - Writes to the graphical terminal window (user-facing output)

### When to use Serial

Use Serial for:
- Debug messages
- Logging
- Development diagnostics
- Background process output

### When to use Terminal

Use Terminal for:
- User-facing text in the terminal window
- Interactive applications
- Visual output in the GUI

## Usage from Userspace

Serial is automatically available in userspace through the `BlackBox.System` namespace:

```csharp
// Directly accessible, no import needed
Serial.Write("Debug message\n");

// Format strings
var x = 42;
Serial.Write($"Debug: Value of x: {x}\n");
```

## Common Patterns

### Debug Output
```csharp
Serial.Write($"[DEBUG] Function called with arg: {arg}\n");
```

### Error Reporting
```csharp
if (error)
{
    Serial.Write($"[ERROR] {errorMessage}\n");
}
```

### Formatted Debug Output
```csharp
Serial.Write("PID   Name       Status\n");
Serial.Write("---   ----       ------\n");
foreach (var p in processes)
{
    Serial.Write($"{p.Id,-5} {p.Name,-10} {p.Status}\n");
}
```

## Design Notes

### Why Serial?

The name "Serial" reflects its purpose as a low-level, stream-based debugging interface, similar to serial console access in embedded systems and operating systems.

### Buffer Management

The `ConsoleBuffer` field is currently a simple string. Future enhancements could include:
- Circular buffer for memory efficiency
- Line-based history
- Buffer size limits

### Thread Safety

Current implementation writes directly to `Console.Write()`, which is thread-safe. Multiple processes can write simultaneously without corruption.

## Future Enhancements

Potential additions:
- `WriteLine(string)` - Write with automatic newline
- `ReadLine()` - Read line from input
- `Clear()` - Clear the buffer
- Multiple output streams (stdout, stderr)

## See Also

- [Terminal](TERMINAL.md) - Terminal window output (user-facing)
- [Window](WINDOW.md) - Terminal window rendering
- [System Layer](SYSTEM.md) - Overview of userspace APIs
