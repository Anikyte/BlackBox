using System;
using System.Peripherals;
using System.Peripherals.Sensors;
using System.IO;
using System.Threading;

static class Utils
{
	public static void GetSensors()
	{
		Image image = new Image();
		for (int j = 0; j < 256; j++)
		{
			int w = j % 16, z = j / 16;
			byte[] r = image.GetBlock(Channel.Red, j);
			byte[] g = image.GetBlock(Channel.Green, j);
			byte[] b = image.GetBlock(Channel.Blue, j);
			for (int i = 0; i < 64; i++)
			{
				int x = i % 8, y = i / 8;
				Bitmap.Set(x+8*w, y+8*z, r[i], g[i], b[i]);
			}
		}

	}
	// public static void BPrint()
	// {
	// 	string b64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
	// 	string img = new Path("User/testcard.b64").Read();
	// 	for (int i = 0; i < 128 * 128; i++)
	// 	{
	// 		Thread.Sleep(1);
	// 		int x = i % 128, y = i / 128;
	// 		byte r = (byte)(b64.IndexOf(img[i * 3]) << 2);
	// 		byte g = (byte)(b64.IndexOf(img[i * 3 + 1]) << 2);
	// 		byte bl = (byte)(b64.IndexOf(img[i * 3 + 2]) << 2);
	// 		Bitmap.Set(x, y, r, g, bl);
	// 	}
	// }
}