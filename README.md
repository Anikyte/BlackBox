# Black Box

A video game about the end of everything through a C# REPL terminal. 

## Architecture
**Hostspace**
- the systems that run userspace
- found in `Machine`

**Userspace**
- everything the user can interact with
- sandboxed using roslyn interpeter
- automatically allows access to public types and methods under the `System` namespace (see compilation)

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
- special file `ShellRC.cs` ran on terminal startup

**Compilation**
- in order to partially replace certain system methods without reimplementing or recompiling the system library, 
we use runtime code analysis to frakenstein together the default system library and our custom `System` assembly
- said custom assembly is generated seperately from the `System` directory in the project folder
- as such, all types and methods are under the system namespace
- to avoid conflicts with the actual system implementations during compilation, we must avoid identifiers that already exist
- this can be done either by renaming your custom type or using a different subnamespace
- this whole system is a fragile house of cards

## Story
Provided nothing but a mysterious computer terminal, you must decipher who you are, *what* you are, **where** you are, and most importantly,
***when*** you are.

You're on a generation ship.  
The ship chronometer reads 3142. ~1000 years after mission start.  
The last navigation beacon broadcast says somewhere past Alpha Centauri.  

But the skies tell a different story.  

With careful analysis from faulty sensor data and half finished stellar drift databases,
you find something else.  
6.4 billion years have passed, and you are now floating within the galactic core, about 0.5 lyrs from Sagittarius Prime,
the black hole at the center of our galaxy.

The stars which once filled the skies have been encased in dyson spheres. The entire galactic core is a network of stellar computers
connected by great filaments. The Milky Way is dead. Every planet churned into computronium and steel.

Even with the combined power of the entire galaxy, the computer is not immortal. Over the billions of years, Andromeda has drifted closer.
You've come at a bad time. As the two galaxies merge, eventually, Sagittarius Prime and Andromeda Prime must merge.
A black hole merger with the combined mass of a hundred million suns.  
And when they collide, they will outshine the universe.
In mere hours.

A shame, really, as Andromeda is still full of life. A bustling galaxy of a trillion stars, each with dozens of planets,
majority home to civilizations far greater than humanity ever became.

But there's nothing you can do. A reviled amalgamation of man and machine, finally coming to rest; soon enough the power of a trillion stars
will be unleashed, and two galaxies scorched clean.

And a hundred million light years away, monkeys on small, wet rock, on the outskirts of their galaxy, will build a detector.  
And they'll pick up a signal. A single small blip, barely registering on the most sensitive instruments.
The last echo of two long dead galaxies.  

Just another data point for some monkey's research paper.

## TODO
- rewrite README.md