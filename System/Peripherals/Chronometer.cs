namespace System.Peripherals;

public static class Chronometer
{
	public static long Time { get; set; } = 984241843990000000; //ticks (100ns) since epoch
}

public class DateTime
{
	//todo datetime class using Int128 or BigInt
	//only really needs to handle gregorian dates and arbitrary timezone offsets
	//and maybe relativity
	//so not that simple
}