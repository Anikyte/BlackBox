namespace System.Peripherals;

public static class Bitmap
{
	public const int Width = 128;
	public const int Height = 128;

	internal static uint[] Buffer = new uint[Width * Height];

	public static void Set(int x, int y, byte r, byte g, byte b) =>
		Set(x, y, (uint)(b << 16 | g << 8 | r | 0xFF000000));

	public static void Set(int x, int y, uint color)
	{
		if (x >= 0 && x < Width && y >= 0 && y < Height)
			Buffer[y * Width + x] = color;
	}

	public static void Clear(uint color = 0xFF000000)
	{
		for (int i = 0; i < Buffer.Length; i++)
			Buffer[i] = color;
	}
}