using BlackBox;
using BlackBox.Machine;

// Initialize window with terminal
Window.Initialize(fontSize: 24);

// Host static constructor runs tests automatically
// Now run the main loop
while (!Window.ShouldClose())
{
	Window.BeginFrame();

	Host.Loop();

	Window.Render();
	Window.EndFrame();
}

Window.Close();
