// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System.Text;
using Prowl.Runtime;
using Prowl.Runtime.Rendering;
using Prowl.Runtime.Utils;

namespace Prowl.Editor.Assets.Importers.Source;

/// <summary>
/// All VTF image formats supported by Source engine.
/// </summary>
public enum ImageFormat
{
    IMAGE_FORMAT_NONE = -1,
    IMAGE_FORMAT_RGBA8888 = 0,
    IMAGE_FORMAT_ABGR8888,
    IMAGE_FORMAT_RGB888,
    IMAGE_FORMAT_BGR888,
    IMAGE_FORMAT_RGB565,
    IMAGE_FORMAT_I8,
    IMAGE_FORMAT_IA88,
    IMAGE_FORMAT_P8,
    IMAGE_FORMAT_A8,
    IMAGE_FORMAT_RGB888_BLUESCREEN,
    IMAGE_FORMAT_BGR888_BLUESCREEN,
    IMAGE_FORMAT_ARGB8888,
    IMAGE_FORMAT_BGRA8888,
    IMAGE_FORMAT_DXT1,
    IMAGE_FORMAT_DXT3,
    IMAGE_FORMAT_DXT5,
    IMAGE_FORMAT_BGRX8888,
    IMAGE_FORMAT_BGR565,
    IMAGE_FORMAT_BGRX5551,
    IMAGE_FORMAT_BGRA4444,
    IMAGE_FORMAT_DXT1_ONEBITALPHA,
    IMAGE_FORMAT_BGRA5551,
    IMAGE_FORMAT_UV88,
    IMAGE_FORMAT_UVWQ8888,
    IMAGE_FORMAT_RGBA16161616F,
    IMAGE_FORMAT_RGBA16161616,
    IMAGE_FORMAT_UVLX8888
}

[Importer("FileIcon.png", typeof(Texture2D), ".vtf")]
public class VTFImporter : ScriptedImporter
{

    public override void Import(SerializedAsset ctx, FileInfo assetPath)
    {
        Texture2D texture = LoadVTFTexture(assetPath);
        ctx.SetMainObject(texture);
    }


    /// <summary>
    /// Loads a VTF file and converts it into a Texture2D.
    /// </summary>
    /// <param name="assetPath">The path to the VTF file.</param>
    /// <returns>A Texture2D object containing the parsed texture.</returns>
    public static Texture2D LoadVTFTexture(FileInfo assetPath)
    {
        // Start binary stream
        using var stream = assetPath.OpenRead();
        using var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: false);

        // Validate VTF signature
        string signature = Encoding.ASCII.GetString(reader.ReadBytes(4));
        if (signature != "VTF\0")
            throw new InvalidDataException("Invalid VTF file signature.");

        // Read version
        uint versionMajor = reader.ReadUInt32(); // version[0]
        uint versionMinor = reader.ReadUInt32(); // version[1]
        string versionStr = $"{versionMajor}.{versionMinor}";
        float version = float.Parse(versionStr, System.Globalization.CultureInfo.InvariantCulture); // Store version as float so we can do checks like "if (version >= 7.5)"

        Console.WriteLine($"[VTFImporter] Loading: {assetPath.Name} (v{version})");

        uint headerSize = reader.ReadUInt32();

        // Image dimensions
        ushort width = reader.ReadUInt16();
        ushort height = reader.ReadUInt16();

        // Image metadata
        uint flags = reader.ReadUInt32();
        ushort frames = reader.ReadUInt16();
        ushort firstFrame = reader.ReadUInt16();

        reader.ReadBytes(4); // Padding
        Vector3 reflectivity = new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        reader.ReadBytes(4); // Padding

        float bumpmapScale = reader.ReadSingle();
        ImageFormat highResImageFormat = (ImageFormat)reader.ReadInt32();
        byte mipmapCount = reader.ReadByte();

        ImageFormat lowResImageFormat = (ImageFormat)reader.ReadInt32();
        byte lowResImageWidth = reader.ReadByte();
        byte lowResImageHeight = reader.ReadByte();

        // Version 7.2+
        ushort depth = 1;
        if (version >= 7.2f)
            depth = reader.ReadUInt16();

        // Version 7.3+
        if (version >= 7.3f)
        {
            reader.BaseStream.Seek(3, SeekOrigin.Current);  // padding2[3]
            uint numResources = reader.ReadUInt32();        // number of resources

            reader.BaseStream.Seek(8, SeekOrigin.Current);  // padding3[8]
        }

        var texture = new Texture2D(width, height)
        {
            Name = Path.GetFileNameWithoutExtension(assetPath.Name)
        };

        return texture;
    }

    /// <summary>
    /// Converts an ImageFormat to a Veldrid PixelFormat.
    /// </summary>
    public static Veldrid.PixelFormat GetVeldridPixelFormat(ImageFormat format) =>
        format switch
        {
            ImageFormat.IMAGE_FORMAT_RGBA8888 => Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,
            _ => LogAndFallback(format)
        };

    private static Veldrid.PixelFormat LogAndFallback(ImageFormat format)
    {
        Console.Error.WriteLine($"[VTFImporter] Unsupported VTF format: {format}");
        return Veldrid.PixelFormat.R8_UNorm;
    }
}
