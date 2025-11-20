**Terminology:**

Host Loop/Host: Machine/Host.cs, manages the emulated system

Sandbox Loop/System Loop: Machine/Sandbox.cs, the interactive shell loop, PID 0

Userspace: Not a traditional userspace, refers to anything accessible from within the emulated system

Hostspace: Everything outside of userspace, including the entire host system

Shell: Machine/Shell.cs, the primary user interface, part of PID 0/System Loop

**Directory structure:**

Files - Default filesystem structure and files. Copied directly to the virtual filesystem at compilation

Machine - Contains host managed code inaccessible in userspace

System - Contains syscall definitions or any other API accessible in userspace