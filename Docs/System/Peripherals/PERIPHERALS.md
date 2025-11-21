# Peripherals

**Location:** `System/Peripherals/`
**Namespace:** `BlackBox.System.Peripherals`
**Status:** 🚧 Placeholder

## Overview

Peripherals provides interfaces for virtual device access from userspace. This allows user code to interact with emulated hardware devices.

## Current Implementation

Currently empty directory - placeholder for future peripheral implementations.

## Planned Architecture

Each peripheral type will have its own class:

```
System/Peripherals/
├── Keyboard.cs        # Keyboard input
├── Mouse.cs           # Mouse input
├── Display.cs         # Display/graphics output
├── Timer.cs           # Hardware timers
├── Network.cs         # Network interface
├── Storage.cs         # Storage devices
└── Audio.cs           # Audio output
```

## Planned Peripherals

### Keyboard

```csharp
namespace BlackBox.System.Peripherals;

public static class Keyboard
{
    // Check if key is pressed
    public static bool IsKeyDown(Key key);

    // Get next key event
    public static KeyEvent? GetKeyEvent();

    // Wait for key press
    public static Key WaitForKey();
}
```

### Mouse

```csharp
public static class Mouse
{
    // Get mouse position
    public static Point Position { get; }

    // Check button state
    public static bool IsButtonDown(MouseButton button);

    // Get next mouse event
    public static MouseEvent? GetMouseEvent();
}
```

### Display

```csharp
public static class Display
{
    // Display dimensions
    public static int Width { get; }
    public static int Height { get; }

    // Set pixel
    public static void SetPixel(int x, int y, Color color);

    // Get pixel
    public static Color GetPixel(int x, int y);

    // Clear screen
    public static void Clear(Color color);

    // Draw primitives
    public static void DrawLine(Point p1, Point p2, Color color);
    public static void DrawRect(Rectangle rect, Color color);

    // Update display
    public static void Refresh();
}
```

### Timer

```csharp
public static class Timer
{
    // Get current tick count
    public static long Ticks { get; }

    // Create timer
    public static int Create(TimeSpan interval, Action callback);

    // Cancel timer
    public static void Cancel(int timerId);

    // Sleep
    public static void Sleep(TimeSpan duration);
}
```

### Network

```csharp
public static class Network
{
    // Create connection
    public static int Connect(string address, int port);

    // Send data
    public static void Send(int connectionId, byte[] data);

    // Receive data
    public static byte[]? Receive(int connectionId);

    // Close connection
    public static void Close(int connectionId);
}
```

## Usage Examples

### Keyboard Input

```csharp
// Check key state
if (Keyboard.IsKeyDown(Key.Enter))
{
    Serial.Write("Enter pressed\n");
}

// Wait for input
var key = Keyboard.WaitForKey();
Serial.Write($"Key pressed: {key}\n");
```

### Display Graphics

```csharp
// Clear screen to black
Display.Clear(Color.Black);

// Draw red rectangle
Display.DrawRect(new Rectangle(10, 10, 100, 50), Color.Red);

// Set individual pixels
for (int x = 0; x < Display.Width; x++)
{
    Display.SetPixel(x, 100, Color.White);
}

// Update display
Display.Refresh();
```

### Timer Events

```csharp
// Create repeating timer
var timerId = Timer.Create(TimeSpan.FromSeconds(1), () =>
{
    Serial.Write($"Tick: {DateTime.Now}\n");
});

// Cancel after 10 seconds
Timer.Sleep(TimeSpan.FromSeconds(10));
Timer.Cancel(timerId);
```

### Network Communication

```csharp
// Connect to server
int conn = Network.Connect("192.168.1.100", 8080);

// Send data
var data = Encoding.UTF8.GetBytes("Hello Server");
Network.Send(conn, data);

// Receive response
var response = Network.Receive(conn);
if (response != null)
{
    var text = Encoding.UTF8.GetString(response);
    Serial.Write($"Received: {text}\n");
}

// Close connection
Network.Close(conn);
```

## Event-Driven Model

Peripherals will support event-driven programming:

```csharp
// Register event handlers
Keyboard.OnKeyDown += (key) =>
{
    Serial.Write($"Key down: {key}\n");
};

Mouse.OnClick += (point, button) =>
{
    Serial.Write($"Click at {point.X},{point.Y}\n");
};

// Events are processed in main loop
```

## Peripheral Types

### Input Devices
- Keyboard
- Mouse
- Gamepad (future)
- Touch screen (future)

### Output Devices
- Display
- Serial (already implemented)
- Audio
- Storage

### Communication
- Network
- Serial ports
- USB (future)

### Timing
- Timers
- Real-time clock

## Integration with Host

Peripherals bridge userspace and hostspace:

```
┌──────────────────────────┐
│   Userspace Code         │
│   Keyboard.IsKeyDown()   │
└──────────┬───────────────┘
           │
┌──────────▼───────────────┐
│   System.Peripherals     │
│   Keyboard API           │
└──────────┬───────────────┘
           │
┌──────────▼───────────────┐
│   Host                   │
│   Physical device        │
│   or emulation           │
└──────────────────────────┘
```

## Emulation vs Real Hardware

Peripherals can be:
- **Emulated** - Software simulation of hardware
- **Pass-through** - Direct access to real hardware (with sandboxing)
- **Hybrid** - Mix of both

## See Also

- [System Layer](../SYSTEM.md) - Overview of userspace APIs
- [Host](../../Machine/HOST.md) - Peripheral management
- [Serial](../SERIAL.md) - Serial console (a type of peripheral)
