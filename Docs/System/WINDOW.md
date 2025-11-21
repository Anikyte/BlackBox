# Window

**Location:** `Window.cs` (root)
**Namespace:** `BlackBox`
**Status:** ✅ Complete

## Overview

Window manages Raylib rendering of the terminal, including character display, cursor blinking, and post-processing shader effects via render textures. This is a hostspace component.

## Features

- Hardware-accelerated terminal rendering via Raylib
- Render texture for post-processing shader effects
- Animated cursor with configurable blink rate
- Custom fragment shader support
- Monospace rendering (8x16 character cells)

## API

### Initialization

```csharp
public static void Initialize(int terminalWidth = 80, int terminalHeight = 25, string title = "BlackBox")
```

### Window Management

```csharp
public static bool ShouldClose()
public static void Close()
```

### Rendering

```csharp
public static void BeginFrame()
public static void Render()
public static void EndFrame()
```

### Shader Support

```csharp
public static void LoadShader(string fragmentShaderPath)
public static void UnloadShader()
```

### Terminal Access

```csharp
public static Terminal? GetTerminal()
public static void Write(string text)
```

## Usage

### Basic Loop

```csharp
Window.Initialize(80, 25, "BlackBox");

while (!Window.ShouldClose())
{
    Window.BeginFrame();

    // Update logic here

    Window.Render();
    Window.EndFrame();
}

Window.Close();
```

### Writing to Terminal (Hostspace)

```csharp
Window.Write("Hello World\n");
```

For userspace, use `System.Terminal.Write()` instead.

### Post-Processing Shaders

```csharp
// Load CRT shader effect
Window.LoadShader("shaders/crt.fs");

// Render loop runs with shader applied

// Unload when done
Window.UnloadShader();
```

## Shader Example

Fragment shader for CRT effect (shaders/crt.fs):

```glsl
#version 330

in vec2 fragTexCoord;
out vec4 finalColor;

uniform sampler2D texture0;

void main()
{
    vec4 color = texture(texture0, fragTexCoord);

    // CRT scanline effect
    float scanline = sin(fragTexCoord.y * 800.0) * 0.04;
    color.rgb -= scanline;

    finalColor = color;
}
```

## Render Pipeline

```
┌─────────────────────────┐
│  BeginFrame()           │
│  Begin render to texture│
└──────────┬──────────────┘
           │
┌──────────▼──────────────┐
│  Render()               │
│  Draw terminal buffer   │
│  Draw cursor            │
└──────────┬──────────────┘
           │
┌──────────▼──────────────┐
│  EndFrame()             │
│  Apply shader (optional)│
│  Draw texture to screen │
└─────────────────────────┘
```

## See Also

- [Terminal](TERMINAL.md) - Terminal emulator (hostspace and userspace)
- [Serial](SERIAL.md) - Console I/O
