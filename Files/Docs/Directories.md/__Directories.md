You are probably used to directories and files being separate objects.
In BlackBox, there is only the unified `Path` object.
A `Path` object is technically just a pointer to a filesystem object,
where the object acts as both a directory and file, being able to
contain other paths and also be written to directly.

On writing to a `Path` object with `Path.Write`, 
it will generate the path and all intermediary paths automatically.
You never have to worry about creating paths!
Any binary data can be written to a path object.
You will generally only write text to them, though.

The special path `System/Init.cs` is ran on startup.
As well, all paths in `System/Programs/` are loaded into memory, 
so their functions can be used in scripts or called in the shell.
Note how `System/Programs/Shell.Main()` is called in `System/Init.cs`.