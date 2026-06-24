Welcome to BlackBox! This document describes basic function of the C# shell.

A distinction should be made between programs and commands. 
Programs are singular scripts providing sets of commands.
For example, the program *Man* provides the command *Read*,
which can be invoked using *Man.Read(args)*.
For now, all programs are listed here. In future,
each will have its own man entry and an easy way to access it.

Builtin program and command list:  
*Man*  
- Read(string or Path path): display a text or markdown file in the viewer pane
- Read(string or Path path, int line): ditto, starting at line `line`
*ed*
- e(string or Path path): loads file `path` into buffer for editing
- a(int line, string s): inserts `string` before `line`
- i(int line, string s): inserts `string` after `line`
- c(int line, string s): replaces `line` with `string`
- d(int line): deletes `line`
- w(): writes to file
- p(int line): prints every line after `line`
- p(): prints all lines