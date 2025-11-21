# Serial

**Location:** `System/Serial.cs`
**Namespace:** `BlackBox.System`
**Status:** ✅ Complete

## Overview

Serial provides low-level console I/O for debugging and output. It's the primary interface for displaying information from userspace code.

## Implementation

```csharp
namespace BlackBox.System;

public class Serial
{
    public string ConsoleBuffer = "";

    public void Write(string s)
    {
        Console.Write(s);
    }

    public string Read(string s)
    {
        return ConsoleBuffer;
    }
}
```

## API

### Write(string)

Writes a string directly to the console.

**Parameters:**
- `s` - String to write

**Example:**
```csharp
Serial.Write("Hello World\n");
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

## Usage from Userspace

Serial is automatically available in userspace through the `BlackBox.System` namespace:

```csharp
// Directly accessible, no import needed
Serial.Write("Debug message\n");

// Format strings
var x = 42;
Serial.Write($"Value of x: {x}\n");

// Multiple writes
Serial.Write("Line 1\n");
Serial.Write("Line 2\n");
Serial.Write("Line 3\n");
```

## Usage from Hostspace

Serial instances can be passed to the Sandbox as globals:

```csharp
// In Host
var serial = new Serial();
var globals = new Globals { Serial = serial };

await Sandbox.Execute("Serial.Write(\"Hello from userspace!\\n\");", globals);
```

## Common Patterns

### Debug Output
```csharp
Serial.Write($"[DEBUG] Function called with arg: {arg}\n");
```

### Progress Indicators
```csharp
for (int i = 0; i < 100; i++)
{
    Serial.Write($"\rProgress: {i}%");
    Thread.Sleep(100);
}
Serial.Write("\n");
```

### Error Reporting
```csharp
if (error)
{
    Serial.Write($"[ERROR] {errorMessage}\n");
}
```

### Formatted Tables
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

The `ConsoleBuffer` field is currently a simple string. In future implementations, this could be enhanced with:
- Circular buffer for memory efficiency
- Line-based history
- Buffer size limits
- Multiple virtual consoles

### Thread Safety

Current implementation writes directly to `Console.Write()`, which is thread-safe. Multiple processes can write simultaneously without corruption.

## Future Enhancements

Potential additions:
- `WriteLine(string)` - Write with automatic newline
- `ReadLine()` - Read line from input
- `Clear()` - Clear the console
- `SetColor(ConsoleColor)` - Set text color
- Multiple output streams (stdout, stderr)
- Input handling and echo control

## See Also

- [IO](IO.md) - Higher-level I/O operations
- [System Layer](SYSTEM.md) - Overview of userspace APIs
- [Sandbox](../Machine/SANDBOX.md) - Execution environment
