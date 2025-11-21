# Terminal

**Hostspace Location:** `Terminal.cs` (root)
**Userspace Location:** `System/Terminal.cs`
**Hostspace Namespace:** `BlackBox`
**Userspace Namespace:** `BlackBox.System`
**Status:** ✅ Complete

## Overview

Terminal has two components:
- **Hostspace (`BlackBox.Terminal`)** - Terminal emulator with VT100-like capabilities, character buffer, color support
- **Userspace (`BlackBox.System.Terminal`)** - API for userspace code to write to the terminal window

## Hostspace Terminal (`BlackBox.Terminal`)

Terminal emulator with character buffer, color support, and cursor management. Handles text rendering, scrolling, and basic control characters.

### Features

- Character buffer (2D grid)
- Per-character foreground/background colors
- Cursor position tracking
- Auto-scrolling when text exceeds height
- Tab support (8-character stops)
- Newline/carriage return handling

### Constructor

```csharp
public Terminal(int width = 80, int height = 25)
```

### Properties

```csharp
public int Width { get; }
public int Height { get; }
public int CursorX { get; }
public int CursorY { get; }
```

### Core Methods

```csharp
public void Clear()
public void Write(string text)
public void SetCursorPosition(int x, int y)
public void SetColor(byte r, byte g, byte b)
public void SetBackgroundColor(byte r, byte g, byte b)
public void ResetColors()
```

### Buffer Access

```csharp
public char GetChar(int x, int y)
public (byte r, byte g, byte b) GetForegroundColor(int x, int y)
public (byte r, byte g, byte b) GetBackgroundColor(int x, int y)
```

## Userspace Terminal (`BlackBox.System.Terminal`)

Userspace API for writing to the terminal window. This is what sandbox code should use.

### Methods

```csharp
public static void Write(string text)
public static void WriteLine(string text)
public static void Clear()
public static void SetCursorPosition(int x, int y)
public static void SetColor(byte r, byte g, byte b)
public static void SetBackgroundColor(byte r, byte g, byte b)
public static void ResetColors()
public static char GetChar(int x, int y)
public static int GetWidth()
public static int GetHeight()
```

### Usage

```csharp
// In userspace code (sandbox)
Terminal.Write("Hello, world!\n");
Terminal.SetColor(255, 0, 0);
Terminal.WriteLine("Red text");
Terminal.ResetColors();
```

## Console vs Terminal

- **`Console.Write()`** - Writes to stdout (for debugging/logging, not visible in terminal window)
- **`Terminal.Write()`** - Writes to the graphical terminal window (userspace)

## See Also

- [Window](WINDOW.md) - Renders terminal with Raylib
- [Serial](SERIAL.md) - Console I/O that writes to terminal
