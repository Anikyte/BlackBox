# Filesystem

**Location:** `System/Filesystem/Filesystem.cs`
**Namespace:** `BlackBox.System.Filesystem`
**Status:** 🚧 Placeholder

## Overview

Filesystem provides a virtual filesystem accessible from userspace. User code cannot directly access the host filesystem - all file operations go through this virtual layer.

## Current Implementation

Currently a placeholder class:

```csharp
namespace BlackBox.System.Filesystem;

public class Filesystem
{

}
```

## Planned API

### File Operations

```csharp
public static class Filesystem
{
    // Read file contents
    public static string Read(string path);
    public static byte[] ReadBytes(string path);

    // Write file contents
    public static void Write(string path, string content);
    public static void WriteBytes(string path, byte[] data);

    // Append to file
    public static void Append(string path, string content);

    // Check if file exists
    public static bool Exists(string path);

    // Delete file
    public static void Delete(string path);

    // Copy file
    public static void Copy(string source, string dest);

    // Move/rename file
    public static void Move(string source, string dest);
}
```

### Directory Operations

```csharp
public static class Filesystem
{
    // List directory contents
    public static IEnumerable<string> List(string path);
    public static IEnumerable<FileInfo> ListDetailed(string path);

    // Create directory
    public static void CreateDirectory(string path);

    // Check if directory exists
    public static bool DirectoryExists(string path);

    // Delete directory
    public static void DeleteDirectory(string path, bool recursive = false);

    // Get current directory
    public static string CurrentDirectory { get; set; }
}
```

### Path Operations

```csharp
public static class Filesystem
{
    // Combine paths
    public static string Combine(params string[] paths);

    // Get absolute path
    public static string GetAbsolutePath(string path);

    // Get parent directory
    public static string GetParent(string path);

    // Get filename
    public static string GetFileName(string path);

    // Get extension
    public static string GetExtension(string path);
}
```

### File Information

```csharp
public static class Filesystem
{
    // Get file size
    public static long GetSize(string path);

    // Get timestamps
    public static DateTime GetCreatedTime(string path);
    public static DateTime GetModifiedTime(string path);

    // Get file attributes
    public static FileAttributes GetAttributes(string path);

    // Get detailed info
    public static FileInfo GetInfo(string path);
}
```

## Usage Examples

### Reading Files

```csharp
// Read text file
var content = Filesystem.Read("/data/config.txt");
Serial.Write(content);

// Read binary file
var data = Filesystem.ReadBytes("/data/image.png");
Serial.Write($"File size: {data.Length} bytes\n");
```

### Writing Files

```csharp
// Write text
Filesystem.Write("/data/output.txt", "Hello World");

// Append text
Filesystem.Append("/data/log.txt", $"{DateTime.Now}: Event occurred\n");

// Write binary
Filesystem.WriteBytes("/data/output.bin", byteArray);
```

### Directory Operations

```csharp
// List files
foreach (var file in Filesystem.List("/programs"))
{
    Serial.Write($"- {file}\n");
}

// Create directory
Filesystem.CreateDirectory("/data/temp");

// Check existence
if (Filesystem.Exists("/data/config.txt"))
{
    var content = Filesystem.Read("/data/config.txt");
}
```

### Path Manipulation

```csharp
// Combine paths
var fullPath = Filesystem.Combine("/data", "files", "document.txt");
// Result: "/data/files/document.txt"

// Get filename
var name = Filesystem.GetFileName("/data/file.txt");
// Result: "file.txt"

// Get parent
var parent = Filesystem.GetParent("/data/files/doc.txt");
// Result: "/data/files"
```

### File Information

```csharp
// Get file size
var size = Filesystem.GetSize("/data/file.txt");
Serial.Write($"Size: {size} bytes\n");

// Get modification time
var modified = Filesystem.GetModifiedTime("/data/file.txt");
Serial.Write($"Modified: {modified}\n");

// Detailed info
var info = Filesystem.GetInfo("/data/file.txt");
Serial.Write($"{info.Name}: {info.Size} bytes, {info.Type}\n");
```

## Planned Types

### FileInfo

```csharp
public class FileInfo
{
    public string Name { get; set; }
    public string Path { get; set; }
    public long Size { get; set; }
    public FileType Type { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
    public FileAttributes Attributes { get; set; }
}
```

### FileType

```csharp
public enum FileType
{
    File,
    Directory,
    Link
}
```

### FileAttributes

```csharp
[Flags]
public enum FileAttributes
{
    None = 0,
    ReadOnly = 1,
    Hidden = 2,
    System = 4,
    Archive = 8
}
```

## Virtual Filesystem Architecture

```
Virtual Filesystem (In-Memory)
├── /
│   ├── programs/          # User programs
│   │   ├── hello.cs
│   │   └── task.cs
│   ├── data/              # User data
│   │   ├── config.txt
│   │   └── output.txt
│   └── system/            # System files
│       ├── init.cs
│       └── shell.cs
```

### Initialization

The filesystem is initialized with default structure from `Files/Userspace/`:

```csharp
// At startup
Filesystem.Initialize();
// Loads default files from Files/Userspace/ into virtual filesystem
```

## Path Format

- **Absolute paths** start with `/`: `/data/file.txt`
- **Relative paths** are relative to current directory: `file.txt`, `../data/file.txt`
- **No drive letters** - Unix-style paths only

## Security

The virtual filesystem:
- **Cannot access host filesystem** directly
- All operations are sandboxed
- Path traversal (`../../../`) is controlled
- No symbolic links outside allowed paths

## Implementation Details

The virtual filesystem will be stored in-memory using:
- Dictionary-based structure for fast lookup
- Byte arrays for file contents
- Metadata tracking (timestamps, attributes)
- Copy-on-write for efficiency

## Integration with Files/

The `Files/Userspace/` directory contains default files that are copied into the virtual filesystem at compilation/startup.

## See Also

- [IO](../IO.md) - Higher-level I/O operations
- [System Layer](../SYSTEM.md) - Overview of userspace APIs
- [Files](../../Files/FILES.md) - Default filesystem structure
