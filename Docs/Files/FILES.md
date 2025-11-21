# Files Directory

**Location:** `Files/`
**Status:** 🚧 Placeholder

## Overview

The Files directory contains the default filesystem structure and files that are copied to the virtual filesystem at compilation/startup.

## Current Structure

```
Files/
└── Userspace/          # User-accessible files
```

## Purpose

The Files directory serves as the **initial filesystem image** for the virtual filesystem:

1. **Development** - Files are stored in this directory during development
2. **Compilation** - Files are embedded or packaged with the application
3. **Runtime** - Files are loaded into the virtual filesystem at startup

## Planned Structure

```
Files/
└── Userspace/
    ├── bin/                    # System binaries/programs
    │   ├── init.cs             # Init script
    │   └── shell.cs            # Shell program
    ├── programs/               # User programs
    │   ├── hello.cs            # Hello world example
    │   └── calculator.cs       # Calculator program
    ├── data/                   # User data files
    │   ├── config.txt          # Configuration
    │   └── readme.md           # User documentation
    └── lib/                    # Libraries
        ├── utils.cs            # Utility functions
        └── common.cs           # Common code
```

## File Types

### System Files (`bin/`)

System initialization and core programs:

```csharp
// bin/init.cs - System initialization
Serial.Write("System initializing...\n");
// ... initialization code
```

### User Programs (`programs/`)

Example programs users can run:

```csharp
// programs/hello.cs
Serial.Write("Hello, World!\n");
```

### Data Files (`data/`)

Configuration and data files:

```
// data/config.txt
setting1=value1
setting2=value2
```

### Libraries (`lib/`)

Reusable code libraries:

```csharp
// lib/utils.cs
public static class Utils
{
    public static int Add(int a, int b) => a + b;
}
```

## Loading Process

### At Startup

```csharp
// In Host initialization
Filesystem.Initialize();
// Loads all files from Files/Userspace/ into virtual filesystem
```

### File Mapping

```
Files/Userspace/programs/hello.cs
    → Virtual filesystem: /programs/hello.cs

Files/Userspace/data/config.txt
    → Virtual filesystem: /data/config.txt
```

## Adding New Files

To add files to the default filesystem:

1. Create the file in `Files/Userspace/` (respecting directory structure)
2. File will automatically be available in virtual filesystem at runtime
3. Access from userspace using the mapped path

Example:

```bash
# Create file
echo "Test data" > Files/Userspace/data/test.txt

# Access from userspace
var content = Filesystem.Read("/data/test.txt");
Serial.Write(content);
```

## File Persistence

Currently planned as **non-persistent** (in-memory only):
- Files are loaded at startup
- Changes are lost when program exits
- Future: Add persistence layer to save changes

## Integration with Filesystem

The Files directory feeds into the virtual Filesystem:

```
┌──────────────────────────┐
│  Files/Userspace/        │
│  (Development files)     │
└──────────┬───────────────┘
           │ Load at startup
           ▼
┌──────────────────────────┐
│  Virtual Filesystem      │
│  (Runtime, in-memory)    │
└──────────┬───────────────┘
           │ Read/Write
           ▼
┌──────────────────────────┐
│  Userspace Code          │
│  Filesystem.Read()       │
└──────────────────────────┘
```

## Example Files

### bin/init.cs - System Initialization

```csharp
// Initialize system
Serial.Write("BlackBox OS initializing...\n");

// Set up environment
var programs = Filesystem.List("/programs");
Serial.Write($"Found {programs.Count()} programs\n");

// Start shell
Serial.Write("Starting shell...\n");
```

### programs/hello.cs - Hello World

```csharp
// Simple hello world program
Serial.Write("Hello, World!\n");
Serial.Write($"Current time: {DateTime.Now}\n");
```

### programs/list.cs - List Files

```csharp
// List all files in virtual filesystem
Serial.Write("Files:\n");
foreach (var file in Filesystem.List("/"))
{
    Serial.Write($"  - {file}\n");
}
```

## Directory Permissions (Future)

Planned permission structure:
- `/bin/` - System files (read-only)
- `/programs/` - User programs (read/write)
- `/data/` - User data (read/write)
- `/lib/` - Libraries (read-only)

## Build Integration

Files can be:
1. **Embedded** - Compiled into the executable
2. **Packaged** - Bundled as resources
3. **External** - Loaded from disk at runtime

Current: To be determined

## See Also

- [Filesystem](../System/Filesystem/FILESYSTEM.md) - Virtual filesystem implementation
- [System Layer](../System/SYSTEM.md) - Userspace APIs
- [Host](../Machine/HOST.md) - System initialization
