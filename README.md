# Game.NET
A lightweight game engine written in C# that utilizes Microsoft's DirectX API.

## Rerequirements
These are the components that this library has been tested with.
* DirextX 11
* .NET Framework 4.6.1
* Windows SDK 10.0.16299.0
* Visual Studio 2017
## Building
This project requires specifically building for either x64 or x86 architecture. Make sure that when copying the output .dlls you put the same architectures together.
## Usage
Reference the Game.NET.dll in a new Visual Studio project, and then be sure to put the `using GameNET;` statement at the top of your main cs file. From there, you can call `Engine.Init()`, `Engine.AddObject()`, and `Engine.Start()`. More Detailed documentation can be found within the code's comments and XML documentation. When you compile your game, make sure that both the D3DInterop.dll and the Game.NET.dll are present in the same folder as your output.

Example project coming soon.
