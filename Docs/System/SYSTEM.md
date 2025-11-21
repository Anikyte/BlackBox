# System Layer

The System layer contains userspace-accessible APIs and syscalls. This is the boundary between sandboxed user code and the host system.

## Components

### [Serial](SERIAL.md) - `System/Serial.cs`
Low-level serial console for debugging and output:
- Direct console write access
- Console buffer management
- Primary debugging interface

**Status:** ✅ Complete

### [IO](IO.md) - `System/IO.cs`
Userspace shell functions and I/O operations:
- Input/output operations
- Shell utilities accessible from userspace

**Status:** 🚧 Placeholder

### [Process](PROCESS.md) - `System/Process.cs`
Process management functions:
- Process creation and termination
- Process information and control
- Inter-process communication

**Status:** 🚧 Placeholder

### [Filesystem](Filesystem/FILESYSTEM.md) - `System/Filesystem/`
Virtual filesystem operations:
- File and directory operations
- Path management
- Virtual file storage

**Status:** 🚧 Placeholder

### [Peripherals](Peripherals/PERIPHERALS.md) - `System/Peripherals/`
Device interface management:
- Peripheral device access
- Device I/O operations

**Status:** 🚧 Placeholder

---

## Architecture

The System layer is automatically accessible from all sandboxed code through the `BlackBox.System` namespace.

```
┌────────────────────────────────────┐
│         Userspace Code             │
│  (Running in Sandbox)              │
│                                    │
│  Can directly access:              │
│  - Serial.Write()                  │
│  - IO functions                    │
│  - Process management              │
│  - Filesystem operations           │
│  - Peripheral devices              │
└────────────────────────────────────┘
                │
                ▼
┌────────────────────────────────────┐
│      BlackBox.System Namespace     │
│                                    │
│  ┌──────────────────────────────┐  │
│  │  Serial                      │  │
│  │  - Write(string)             │  │
│  │  - Read()                    │  │
│  └──────────────────────────────┘  │
│                                    │
│  ┌──────────────────────────────┐  │
│  │  IO                          │  │
│  │  - Input/output operations   │  │
│  └──────────────────────────────┘  │
│                                    │
│  ┌──────────────────────────────┐  │
│  │  Process                     │  │
│  │  - Process management        │  │
│  └──────────────────────────────┘  │
│                                    │
│  ┌──────────────────────────────┐  │
│  │  Filesystem                  │  │
│  │  - Virtual file operations   │  │
│  └──────────────────────────────┘  │
└────────────────────────────────────┘
                │
                ▼
┌────────────────────────────────────┐
│      Sandbox (Hostspace)           │
│  Security boundary and enforcement │
└────────────────────────────────────┘
```

## Automatic Namespace Import

The `BlackBox.System` namespace is automatically imported in the Sandbox, so userspace code can directly use:

```csharp
// No need for: using BlackBox.System;

Serial.Write("Hello World\n");
var files = Filesystem.List("/");
var pid = Process.Spawn("program.cs");
```

## Security Model

### What Userspace Can Do
- Call System layer APIs
- Access virtual filesystem
- Spawn and manage processes
- Write to serial console
- Use peripheral devices (through APIs)

### What Userspace Cannot Do
- Access host filesystem directly
- Call arbitrary .NET APIs
- Use unsafe code
- Access Machine layer (Host, Sandbox internals)
- Break out of sandbox

## Creating New System APIs

To add a new userspace API:

1. Create the class in `System/` namespace
2. The Sandbox automatically includes all `BlackBox.System` types
3. No additional configuration needed

Example:

```csharp
// System/Network.cs
namespace BlackBox.System;

public class Network
{
    public static void Connect(string address)
    {
        // Implementation
    }
}

// Automatically accessible from userspace:
// Network.Connect("192.168.1.1");
```

## Component Status

| Component | File | Status |
|-----------|------|--------|
| Serial | `Serial.cs` | ✅ Complete |
| IO | `IO.cs` | 🚧 Placeholder |
| Process | `Process.cs` | 🚧 Placeholder |
| Filesystem | `Filesystem/Filesystem.cs` | 🚧 Placeholder |
| Peripherals | `Peripherals/` | 🚧 Placeholder |

## See Also

- [Sandbox](../Machine/SANDBOX.md) - Executes userspace code
- [Machine Layer](../Machine/MACHINE.md) - Hostspace components
