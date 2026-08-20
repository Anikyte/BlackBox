using System.Peripherals.Sensors;
using System.Utils;
using Microsoft.Xna.Framework;

namespace BlackBox.Machine.World;

public class Objectoid
{
	public static List<Objectoid> Objects = new(); //sensors render over this
	
	public int Diameter;
	public Vector3 Position;
	public GUID GUID;
}

public class Planet : Objectoid
{
	
}

public class Star : Objectoid
{
	public int Luminosity;
	public int Temperature;
}

public class Pulsar : Star
{
	public float Period;
	public float PeriodDerivative;
	
	public Pulsar(float period, float periodDerivative, int luminosity, int temperature, int diameter, Vector3 position)
	{
		Period = period;
		PeriodDerivative = periodDerivative;
		Luminosity = luminosity;
		Temperature = temperature;
		Diameter = diameter;
		Position = position;
		GUID = GUID.V7(new Random()); //todo: single global random
		
		Readout.Pulsars.Add(this);
	}
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