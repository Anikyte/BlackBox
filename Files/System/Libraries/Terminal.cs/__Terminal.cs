using System;
using System.Peripherals;
using System.IO;

static class Terminux
{

	public static void BPrint()
	{
		string b64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
		string img = new Path("User/testcard.b64").Read();
		for (int i = 0; i < 128 * 128; i++)
		{
			//Thread.Sleep(1);
			int x = i % 128, y = i / 128;
			byte r = (byte)(b64.IndexOf(img[i * 3]) << 2);
			byte g = (byte)(b64.IndexOf(img[i * 3 + 1]) << 2);
			byte bl = (byte)(b64.IndexOf(img[i * 3 + 2]) << 2);
			Bitmap.Set(x, y, r, g, bl);
		}
	}
}