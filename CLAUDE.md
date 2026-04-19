This project is a video game emulating a full and novel C# based computer system.  

- prioritize static classes
- no single use functions
- code should be as concise as possible
- Don't use dedicated setter functions. Use properties.
- Use as few variables as necessary. It is better to compute a value than to store it.
- Always give a summary of what you've changed

## Architecture
**Hostspace**
- the systems that run userspace
- found in `Machine`

**Userspace**
- user accessible system types
- sandboxed using roslyn interpeter
- automatically allows access to public types and methods under the `System` namespace (see compilation)
- `internal` objects will not be accessible in programspace and should be used for objects that the player should not be able to access where it would ruin immersion or risk damaging their computer. aside from that, the player should never be limited.

**Programspace**
- code compiled at runtime within the virtual machine
- programs, user data, puzzles, etc

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