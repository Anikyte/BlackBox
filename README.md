# BlackBox

A C# scripting sandbox environment built on Microsoft Roslyn, designed as a virtual machine with strict separation between userspace (sandboxed code) and hostspace (host system).

## Overview

BlackBox provides a secure execution environment for C# scripts with:
- **Roslyn-based sandbox** - Execute C# code safely in an isolated environment
- **Virtual filesystem** - Sandboxed file operations
- **Process management** - Spawn and manage subprocesses on separate threads
- **System APIs** - Controlled userspace access to system functions
- **Security boundaries** - Clear separation between trusted and untrusted code

## Architecture

```
┌─────────────────────────────────────────┐
│  Machine Layer (Hostspace)              │
│  ┌───────────────────────────────────┐  │
│  │  Host - Main program loop         │  │
│  └───────────────────────────────────┘  │
│  ┌───────────────────────────────────┐  │
│  │  Sandbox - PID 0 / Init process   │  │
│  │  - Execution loop                 │  │
│  │  - Main thread (shell)            │  │
│  │  - Subprocesses (user programs)   │  │
│  └───────────────────────────────────┘  │
│  ┌───────────────────────────────────┐  │
│  │  Shell - User interface           │  │
│  └───────────────────────────────────┘  │
└─────────────────────────────────────────┘
                   │
        Security Boundary
                   │
┌─────────────────▼─────────────────────┐
│  System Layer (Userspace)             │
│  ┌──────────────────────────────────┐ │
│  │  Serial - Console I/O            │ │
│  │  IO - High-level I/O             │ │
│  │  Process - Process management    │ │
│  │  Filesystem - Virtual filesystem │ │
│  │  Peripherals - Device interfaces │ │
│  └──────────────────────────────────┘ │
└────────────────────────────────────────┘
```

### Key Components

| Layer | Component | Description | Status |
|-------|-----------|-------------|--------|
| **Machine** | Host | Main program loop, system management | 🚧 Placeholder |
| **Machine** | Sandbox | Roslyn script executor (PID 0) | ✅ Complete |
| **Machine** | Shell | Interactive user interface | 🚧 Placeholder |
| **System** | Serial | Low-level console I/O | ✅ Complete |
| **System** | IO | High-level I/O operations | 🚧 Placeholder |
| **System** | Process | Process management | 🚧 Placeholder |
| **System** | Filesystem | Virtual filesystem | 🚧 Placeholder |
| **System** | Peripherals | Device interfaces | 🚧 Placeholder |

## Quick Start

### Basic Usage

```csharp
// Execute C# code in sandbox
var result = await Sandbox.Execute("var x = 10; return x * 2;");
Console.WriteLine(result.ReturnValue);  // Output: 20

// Spawn subprocess
int pid = Sandbox.Spawn(@"
    for (int i = 0; i < 10; i++)
    {
        Serial.Write($""Count: {i}\n"");
        Thread.Sleep(100);
    }
");

// Wait for completion
var processResult = await Sandbox.Wait(pid);
```

### Running the Execution Loop

```csharp
// Start continuous execution loop
Sandbox.Run(() =>
{
    // Your per-iteration logic
    // Runs as fast as possible
});

// Stop when done
Sandbox.Stop();
await Sandbox.WaitForStop();
```

## Documentation

**Full documentation:** [Docs/MAIN.md](Docs/MAIN.md)

### Component Documentation

- **[Machine Layer](Docs/Machine/MACHINE.md)** - Hostspace components
  - [Host](Docs/Machine/HOST.md) - Main program loop
  - [Sandbox](Docs/Machine/SANDBOX.md) - Execution engine ✅
  - [Shell](Docs/Machine/SHELL.md) - User interface
- **[System Layer](Docs/System/SYSTEM.md)** - Userspace APIs
  - [Serial](Docs/System/SERIAL.md) - Console I/O ✅
  - [IO](Docs/System/IO.md) - High-level I/O
  - [Process](Docs/System/PROCESS.md) - Process management
  - [Filesystem](Docs/System/Filesystem/FILESYSTEM.md) - Virtual filesystem
  - [Peripherals](Docs/System/Peripherals/PERIPHERALS.md) - Devices
- **[Files](Docs/Files/FILES.md)** - Default filesystem structure

## Requirements

- **.NET 9.0**
- **Microsoft.CodeAnalysis.CSharp.Scripting 4.12.0**

## Security Features

- No unsafe code allowed
- Arithmetic overflow checking enforced
- Limited assembly access (only approved libraries)
- Sandboxed execution with no direct host filesystem access
- Controlled API surface through System layer

## Project Status

BlackBox is in **early development**. Core Sandbox functionality is complete, other components are placeholders.

### Completed ✅
- Sandbox execution engine
- Main execution loop
- Subprocess management
- Serial console

### In Progress 🚧
- Host loop
- Shell implementation
- Virtual filesystem
- Process management
- Peripherals
- IO operations

## License

[To be determined]

## Contributing

[To be determined]
