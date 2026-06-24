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
}