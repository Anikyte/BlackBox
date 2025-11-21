# Machine Layer

The Machine layer contains host-managed code that is inaccessible from userspace. This is the "hostspace" boundary that manages the entire emulated system.

## Components

### [Host](HOST.md) - `Machine/Host.cs`
The main program loop responsible for:
- Spawning the Sandbox loop (PID 0)
- Managing peripherals and filesystem updates
- Status monitoring and error/crash handling
- Timed event management

**Status:** 🚧 Placeholder (basic test code only)

### [Sandbox](SANDBOX.md) - `Machine/Sandbox.cs`
The Roslyn-based C# script execution engine running as PID 0 (init process):
- Continuous execution loop
- Main thread code execution (for shell)
- Subprocess management on separate threads
- State management and security enforcement

**Status:** ✅ Complete

### [Shell](SHELL.md) - `Machine/Shell.cs`
The primary user interface and shell functions for hostspace:
- Command parsing and execution
- User interaction handling
- Shell built-ins and utilities

**Status:** 🚧 Placeholder

---

## Architecture

```
┌─────────────────────────────────────────┐
│           Host (Static Class)           │
│  ┌───────────────────────────────────┐  │
│  │  Main Program Loop                │  │
│  │  - Update peripherals/filesystem  │  │
│  │  - Monitor sandbox status         │  │
│  │  - Handle timed events            │  │
│  └───────────────────────────────────┘  │
│                   │                      │
│                   ▼                      │
│  ┌───────────────────────────────────┐  │
│  │    Sandbox (Static Class)         │  │
│  │    PID 0 - Init Process           │  │
│  │  ┌─────────────────────────────┐  │  │
│  │  │  Execution Loop             │  │  │
│  │  │  (runs as fast as possible) │  │  │
│  │  └─────────────────────────────┘  │  │
│  │  ┌─────────────────────────────┐  │  │
│  │  │  Main Thread Execution      │  │  │
│  │  │  (Shell commands)           │  │  │
│  │  └─────────────────────────────┘  │  │
│  │  ┌─────────────────────────────┐  │  │
│  │  │  Subprocess Management      │  │  │
│  │  │  PID 1, 2, 3... (threads)   │  │  │
│  │  └─────────────────────────────┘  │  │
│  └───────────────────────────────────┘  │
│                   │                      │
│                   ▼                      │
│  ┌───────────────────────────────────┐  │
│  │    Shell (Static Class)           │  │
│  │  - User interface                 │  │
│  │  - Command handling               │  │
│  └───────────────────────────────────┘  │
└─────────────────────────────────────────┘
                   │
                   ▼
         ┌─────────────────┐
         │  System APIs    │
         │  (Userspace)    │
         └─────────────────┘
```

## Boundary Between Hostspace and Userspace

The **Sandbox** serves as the border between hostspace and userspace:

### Hostspace (Machine Layer)
- Direct access to host system
- Manages the emulated environment
- Controls resource allocation
- Cannot be accessed from user code

### Userspace (System Layer)
- Sandboxed execution environment
- Access only through System APIs
- Security restrictions enforced
- User code runs here

---

## Static Design

All Machine layer classes are **static** - there is only one instance of each:

- **Host** - Single main program loop
- **Sandbox** - Single execution environment (PID 0)
- **Shell** - Single shell instance

This design ensures:
- No accidental multiple instances
- Global accessibility
- Clear singleton pattern
- Simplified lifecycle management
