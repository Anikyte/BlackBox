//sensor subsystem
//works with raw sensor data
//will require the player to process it

//step 3: get sensors to transmit image of static rendered sphere
//step 4: wire sensors into world objects
//step 5: reuse code to implement laser distance measurement
//step 6: qol like noise et al

//once 70% or something of blocks are read, trigger next gamestage
//alternatively, if prerendering is used, check for pixels on the display buffer like those printer codes

namespace System.Peripherals.Sensors;

public enum Channel
{
	Red,
	Green,
	Blue,
}

public class Image {
	string img = new Path("User/bhole.b64").Read(); //temporary
	string b64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
	
	public static string Metadata()
	{
		return "Blocks: 256 (16x16); Mode: 8 bit color;";
	}
	
	public Image() { }
	public void NextBlock() {}

	public byte[] GetBlock(Channel channel, int j) //8x8 blocks, j is linear index 0-255
	{
		byte[] bytes = new byte[64];
		if (j < 0 || j >= 256) return bytes;

		int bx = j % 16, by = j / 16;
		for (int i = 0; i < 64; i++)
		{
			Thread.Sleep(1);
			int px = bx * 8 + i % 8, py = by * 8 + i / 8;
			int idx = (py * 128 + px) * 3;
			bytes[i] = channel switch
			{
				Channel.Red => (byte)(b64.IndexOf(img[idx]) << 2),
				Channel.Green => (byte)(b64.IndexOf(img[idx + 1]) << 2),
				Channel.Blue => (byte)(b64.IndexOf(img[idx + 2]) << 2),
				_ => 0
			};
		}
		return bytes;
	}
}