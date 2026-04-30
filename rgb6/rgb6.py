#!/usr/bin/env python3
"""Convert an image to base64 with 6 bits per channel (3 chars per pixel).

Each channel is quantized to 6 bits (top bits of the original 8) and mapped
directly to one base64 character. Pixels are emitted as RGB triplets, so
each pixel takes exactly 3 characters of output.
"""

import sys
from PIL import Image

ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"


def encode(path: str) -> tuple[int, int, str]:
	img = Image.open(path).convert("RGB")
	w, h = img.size
	pixels = img.getdata()
	out = bytearray(len(pixels) * 3)
	for i, (r, g, b) in enumerate(pixels):
		j = i * 3
		out[j]	   = ord(ALPHABET[r >> 2])
		out[j + 1] = ord(ALPHABET[g >> 2])
		out[j + 2] = ord(ALPHABET[b >> 2])
	return w, h, out.decode("ascii")


def decode(w: int, h: int, s: str) -> Image.Image:
	lookup = {c: i for i, c in enumerate(ALPHABET)}
	img = Image.new("RGB", (w, h))
	# Scale 6-bit (0..63) back to 8-bit (0..255). Multiplying by 255/63
	# spreads the levels evenly across the full output range.
	pixels = [
		(
			lookup[s[i]]	 * 255 // 63,
			lookup[s[i + 1]] * 255 // 63,
			lookup[s[i + 2]] * 255 // 63,
		)
		for i in range(0, len(s), 3)
	]
	img.putdata(pixels)
	return img


if __name__ == "__main__":
	if len(sys.argv) < 2:
		print("usage: rgb6perchannel.py <image>", file=sys.stderr)
		sys.exit(1)
	w, h, data = encode(sys.argv[1])
	print(f"{w} {h}")
	print(data)