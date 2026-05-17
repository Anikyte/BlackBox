using Microsoft.Xna.Framework;

namespace BlackBox.Machine.World;

public class Objectoid
{
	public static List<Objectoid> Objects = new(); //sensors render over this
	
	public int Diameter;
	public Vector3 Position;
}

public class Planet : Objectoid
{
	
}

public class Star : Objectoid
{
	
}

public class Filament : Objectoid
{
	public static Filament[] SpawnSwarm(Vector3 pos1, Vector3 pos2, int diameter1, int diameter2, int diameterPinch, int density)
	{
		//two point line equation to generate core line
		//using diameters, generate spline curve for outer shell of filament
		//revolve curve to get coordinate surface
		//spawn filament nodes on surface at given density
		return null;
	}
}