// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.


namespace Prowl.Editor.Utilities.Source;

/// <summary>
/// Utilities for DXT Compression.
/// </summary>
internal static class DXTCompression
{
    #region DXT1
    /// <summary>
    /// Decodes DXT1 compressed data to raw RGBA bytes (32-bit per pixel)
    /// </summary>
    public static byte[] DecodeDXT1(int width, int height, byte[] dxt1Data)
    {
        int blockCountX = (width + 3) / 4;
        int blockCountY = (height + 3) / 4;
        byte[] pixels = new byte[width * height * 4];

        for (int blockY = 0; blockY < blockCountY; blockY++)
        {
            for (int blockX = 0; blockX < blockCountX; blockX++)
            {
                int blockIndex = (blockY * blockCountX + blockX) * 8; // DXT1 block size = 8 bytes
                DecodeDXT1Block(dxt1Data, blockIndex, pixels, width, height, blockX * 4, blockY * 4);
            }
        }

        return pixels;
    }

    /// <summary>
    /// Decodes a single 8-byte DXT1 block at blockOffset in compressed data, writes RGBA pixels at (px, py)
    /// </summary>
    public static void DecodeDXT1Block(byte[] data, int blockOffset, byte[] pixels, int width, int height, int px, int py)
    {
        ushort color0 = BitConverter.ToUInt16(data, blockOffset);
        ushort color1 = BitConverter.ToUInt16(data, blockOffset + 2);
        uint code = BitConverter.ToUInt32(data, blockOffset + 4);

        (byte r0, byte g0, byte b0) = DecodeRGB565(color0);
        (byte r1, byte g1, byte b1) = DecodeRGB565(color1);

        byte[] r = new byte[4];
        byte[] g = new byte[4];
        byte[] b = new byte[4];
        byte[] a = new byte[4];  // alpha is 255 or 0 depending on color interpolation

        r[0] = r0; g[0] = g0; b[0] = b0; a[0] = 255;
        r[1] = r1; g[1] = g1; b[1] = b1; a[1] = 255;

        if (color0 > color1)
        {
            r[2] = (byte)((2 * r0 + r1) / 3);
            g[2] = (byte)((2 * g0 + g1) / 3);
            b[2] = (byte)((2 * b0 + b1) / 3);
            a[2] = 255;

            r[3] = (byte)((r0 + 2 * r1) / 3);
            g[3] = (byte)((g0 + 2 * g1) / 3);
            b[3] = (byte)((b0 + 2 * b1) / 3);
            a[3] = 255;
        }
        else
        {
            r[2] = (byte)((r0 + r1) / 2);
            g[2] = (byte)((g0 + g1) / 2);
            b[2] = (byte)((b0 + b1) / 2);
            a[2] = 255;

            r[3] = 0;
            g[3] = 0;
            b[3] = 0;
            a[3] = 0;  // Transparent pixel for DXT1 when color0 <= color1
        }

        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                int pixelX = px + col;
                int pixelY = py + row;
                if (pixelX >= width || pixelY >= height) continue;

                int pixelIndex = pixelY * width + pixelX;
                int codeIndex = 2 * (row * 4 + col);
                int colorCode = (int)((code >> codeIndex) & 0x03);

                pixels[pixelIndex * 4 + 0] = r[colorCode];
                pixels[pixelIndex * 4 + 1] = g[colorCode];
                pixels[pixelIndex * 4 + 2] = b[colorCode];
                pixels[pixelIndex * 4 + 3] = a[colorCode];
            }
        }
    }
    #endregion

    #region DXT5

    /// <summary>
    /// Decodes DXT5 compressed data to raw RGBA bytes (32-bit per pixel)
    /// </summary>
    public static byte[] DecodeDXT5(int width, int height, byte[] dxt5Data)
    {
        int blockCountX = (width + 3) / 4;
        int blockCountY = (height + 3) / 4;
        byte[] pixels = new byte[width * height * 4];

        for (int blockY = 0; blockY < blockCountY; blockY++)
        {
            for (int blockX = 0; blockX < blockCountX; blockX++)
            {
                int blockIndex = (blockY * blockCountX + blockX) * 16;
                DecodeDXT5Block(dxt5Data, blockIndex, pixels, width, height, blockX * 4, blockY * 4);
            }
        }

        return pixels;
    }

    /// <summary>
    /// Decodes a single 16-byte DXT5 block at blockOffset in compressed data, writes RGBA pixels at (px, py)
    /// </summary>
    public static void DecodeDXT5Block(byte[] data, int blockOffset, byte[] pixels, int width, int height, int px, int py)
    {
        // Decode alpha (8 bytes)
        byte alpha0 = data[blockOffset];
        byte alpha1 = data[blockOffset + 1];
        ulong alphaBits = 0;
        for (int i = 0; i < 6; i++)
        {
            alphaBits |= ((ulong)data[blockOffset + 2 + i]) << (8 * i);
        }

        byte[] alphaValues = new byte[8];
        alphaValues[0] = alpha0;
        alphaValues[1] = alpha1;

        if (alpha0 > alpha1)
        {
            for (int i = 2; i < 8; i++)
                alphaValues[i] = (byte)(((8 - i) * alpha0 + (i - 1) * alpha1) / 7);
        }
        else
        {
            for (int i = 2; i < 6; i++)
                alphaValues[i] = (byte)(((6 - i) * alpha0 + (i - 1) * alpha1) / 5);
            alphaValues[6] = 0;
            alphaValues[7] = 255;
        }

        // Decode color (8 bytes)
        ushort color0 = BitConverter.ToUInt16(data, blockOffset + 8);
        ushort color1 = BitConverter.ToUInt16(data, blockOffset + 10);

        uint code = BitConverter.ToUInt32(data, blockOffset + 12) & 0xFFFFFF;

        // Convert RGB565 to 24-bit RGB
        (byte r0, byte g0, byte b0) = DecodeRGB565(color0);
        (byte r1, byte g1, byte b1) = DecodeRGB565(color1);

        byte[] r = new byte[4];
        byte[] g = new byte[4];
        byte[] b = new byte[4];

        r[0] = r0; g[0] = g0; b[0] = b0;
        r[1] = r1; g[1] = g1; b[1] = b1;

        if (color0 > color1)
        {
            r[2] = (byte)((2 * r0 + r1) / 3);
            g[2] = (byte)((2 * g0 + g1) / 3);
            b[2] = (byte)((2 * b0 + b1) / 3);

            r[3] = (byte)((r0 + 2 * r1) / 3);
            g[3] = (byte)((g0 + 2 * g1) / 3);
            b[3] = (byte)((b0 + 2 * b1) / 3);
        }
        else
        {
            r[2] = (byte)((r0 + r1) / 2);
            g[2] = (byte)((g0 + g1) / 2);
            b[2] = (byte)((b0 + b1) / 2);

            r[3] = 0;
            g[3] = 0;
            b[3] = 0;
        }

        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                int pixelX = px + col;
                int pixelY = py + row;
                if (pixelX >= width || pixelY >= height) continue;

                int pixelIndex = pixelY * width + pixelX;

                int codeIndex = 2 * (row * 4 + col);
                int colorCode = (int)((code >> codeIndex) & 0x03);

                // Alpha index
                int alphaCodeIndex = 3 * (row * 4 + col);
                int alphaCode = (int)((alphaBits >> alphaCodeIndex) & 0x07);

                pixels[pixelIndex * 4 + 0] = r[colorCode];
                pixels[pixelIndex * 4 + 1] = g[colorCode];
                pixels[pixelIndex * 4 + 2] = b[colorCode];
                pixels[pixelIndex * 4 + 3] = alphaValues[alphaCode];
            }
        }
    }

    #endregion

    // Converts RGB565 16-bit color to 8-bit per channel
    private static (byte r, byte g, byte b) DecodeRGB565(ushort c)
    {
        byte r = (byte)((c >> 11) & 0x1F);
        byte g = (byte)((c >> 5) & 0x3F);
        byte b = (byte)(c & 0x1F);

        // Scale to 0-255
        r = (byte)((r << 3) | (r >> 2));
        g = (byte)((g << 2) | (g >> 4));
        b = (byte)((b << 3) | (b >> 2));

        return (r, g, b);
    }
}
