This project is a video game emulating a full and novel C# based computer system.  

- Prioritize static classes
- No single use functions
- Code should be as concise as possible
- Comment unclear code
- Don't use dedicated setter functions. Use properties
- Use as few variables as necessary. It is better to compute a value than to store it
- Always give a summary of what you've changed
- Always ask instead of guessing
- Do not touch strings unless explicitly instructed to do so
- Do only what is asked

If needed, tests may be added to Files/System/Programs/Init.cs/__Init.cs
Console.Write and Status.Throw will log to standard output

## Architecture
**Hostspace**
- the systems that run userspace
- found in `Machine`
- Host.cs handles IPC messages and general kernel logic that doesn't fit elsewhere
- Sandbox.cs handles the roslyn sandbox
- Ship.cs and World.cs handle gameplay systems

**Userspace**
- user accessible system types
- sandboxed using roslyn interpeter
- automatically allows access to public types and methods under the `System` namespace (see compilation)
- `internal` objects will not be accessible in programspace and should be used for objects that the player should not be able to access where it would ruin immersion or risk damaging their computer. aside from that, the player should never be limited.

**Programspace**
- code compiled at runtime within the virtual machine
- programs, user data, puzzles, the shell, etc

**Filesystem**
- custom 'filesystem' implementation over top of user filesystem
- all paths are both a file and a directory
- such any path can be listed or written to
- intermediary paths autogenerate when writing to a nonexistant subdirectory
- implemented as every path being a directory with a file inside under the name format `__DirectoryName`
- any binary data can be written to the file
- custom file operation implementation designed to reduce confusion for beginners while still being powerful and intuitive for experienced users
- default files are found under `Files` in the project directory
- will be copied to output on compilation
- special file `Init.cs` ran on terminal startup