// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System.Text;

using Prowl.Editor.Utilities.Source;
using Prowl.Runtime;
using Prowl.Runtime.GUI;
using Prowl.Runtime.Rendering;
using Prowl.Runtime.Utils;

namespace Prowl.Editor.Assets.Importers.Source;

/// <summary>
/// All VTF image formats supported by Source engine.
/// </summary>
public enum VTFFormat
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
    // Map VTF format to decoder
    private static readonly Dictionary<VTFFormat, Func<int, int, byte[], byte[]>> DXTDecoders = new()
    {
        { VTFFormat.IMAGE_FORMAT_DXT1, DXTCompression.DecodeDXT1 },
        { VTFFormat.IMAGE_FORMAT_DXT1_ONEBITALPHA, DXTCompression.DecodeDXT1 },
        { VTFFormat.IMAGE_FORMAT_DXT3, DXTCompression.DecodeDXT3 },
        { VTFFormat.IMAGE_FORMAT_DXT5, DXTCompression.DecodeDXT5 }
    };

    [Header("VTF")]
    public TextureWrapMode TextureWrap = TextureWrapMode.Wrap;

    public enum MipmapGenerationMode { Default, DontGenerate, GenerateMipmaps}

    public MipmapGenerationMode MipmapGeneration = MipmapGenerationMode.Default;

    public FilterType TextureMinFilter = FilterType.Linear;
    public FilterType TextureMagFilter = FilterType.Linear;
    public FilterType TextureMipFilter = FilterType.Linear;

    private record struct VTFResource(string Tag, byte Flags, uint Offset);

    public override void Import(SerializedAsset ctx, FileInfo assetPath)
    {
        Texture2D texture = LoadVTFTexture(assetPath);

        texture.Name = Path.GetFileNameWithoutExtension(assetPath.Name);

        texture.Sampler.SetFilter(TextureMinFilter, TextureMagFilter, TextureMipFilter);
        texture.Sampler.SetWrapMode(SamplerAxis.U | SamplerAxis.V | SamplerAxis.W, TextureWrap);

        if (MipmapGeneration == MipmapGenerationMode.GenerateMipmaps)
            texture.GenerateMipmaps();

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
        VTFFormat highResImageFormat = (VTFFormat)reader.ReadInt32();
        byte mipmapCount = reader.ReadByte();

        VTFFormat lowResImageFormat = (VTFFormat)reader.ReadInt32();
        byte lowResImageWidth = reader.ReadByte();
        byte lowResImageHeight = reader.ReadByte();

        ushort depth = version >= 7.2f ? reader.ReadUInt16() : (ushort)1;

        List<VTFResource> resources = new();

        if (version >= 7.3f)
        {
            reader.BaseStream.Seek(3, SeekOrigin.Current);  // padding2[3]
            uint numResources = reader.ReadUInt32();
            reader.BaseStream.Seek(8, SeekOrigin.Current);  // padding3[8]

            for (int i = 0; i < numResources; i++)
            {
                byte[] tagBytes = reader.ReadBytes(3);
                string tag = Encoding.ASCII.GetString(tagBytes);
                byte flags2 = reader.ReadByte();
                uint offset = reader.ReadUInt32();
                resources.Add(new(tag, flags2, offset));
            }
        }

        var texture = new Texture2D(width, height, format: GetVeldridPixelFormat(highResImageFormat));

        if (version >= 7.3f)
        {
            var highRes = resources.FirstOrDefault(r => r.Tag == "\x30\0\0");
            if (highRes == default)
                throw new InvalidDataException("Missing high-res image resource.");

            stream.Seek(highRes.Offset, SeekOrigin.Begin);
        }

        if (DXTDecoders.TryGetValue(highResImageFormat, out var decoder))
        {
            int blockSize = highResImageFormat switch
            {
                VTFFormat.IMAGE_FORMAT_DXT1 or VTFFormat.IMAGE_FORMAT_DXT1_ONEBITALPHA => 8,
                VTFFormat.IMAGE_FORMAT_DXT3 or VTFFormat.IMAGE_FORMAT_DXT5 => 16,
                _ => throw new InvalidDataException($"Unsupported block-compressed format: {highResImageFormat}")
            };
            int blockWidth = (width + 3) / 4;
            int blockHeight = (height + 3) / 4;
            int compressedDataSize = blockWidth * blockHeight * blockSize;

            byte[] compressedData = reader.ReadBytes(compressedDataSize);
            if (compressedData.Length != compressedDataSize)
                throw new InvalidDataException($"Unexpected DXT data size. Expected {compressedDataSize}, got {compressedData.Length}");

            byte[] decodedPixels = decoder(width, height, compressedData);
            texture.SetData<byte>(decodedPixels);
        }
        else
        {
            int bytesPerPixel = 4; // Assuming uncompressed format
            int imageSize = width * height * bytesPerPixel;

            byte[] pixels = reader.ReadBytes(imageSize);
            if (pixels.Length != imageSize)
                throw new InvalidDataException($"Unexpected pixel data size. Expected {imageSize}, got {pixels.Length}");

            texture.SetData<byte>(pixels);
        }

        return texture;
    }

    /// <summary>
    /// Converts an ImageFormat to a Veldrid PixelFormat.
    /// </summary>
    public static Veldrid.PixelFormat GetVeldridPixelFormat(VTFFormat format) =>
        format switch
        {
            // 32-bit formats with alpha
            VTFFormat.IMAGE_FORMAT_RGBA8888 => Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,
            VTFFormat.IMAGE_FORMAT_BGRA8888 => Veldrid.PixelFormat.B8_G8_R8_A8_UNorm,

            // 16-bit float format with alpha
            VTFFormat.IMAGE_FORMAT_RGBA16161616F => Veldrid.PixelFormat.R16_G16_B16_A16_Float,

            // 16-bit unsigned normalized RGBA
            VTFFormat.IMAGE_FORMAT_RGBA16161616 => Veldrid.PixelFormat.R16_G16_B16_A16_UNorm,

            // Compressed formats, fallback to R8_G8_B8_A8_UNorm since we decompress on CPU
            VTFFormat.IMAGE_FORMAT_DXT1 => Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,
            VTFFormat.IMAGE_FORMAT_DXT1_ONEBITALPHA => Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,
            VTFFormat.IMAGE_FORMAT_DXT3 => Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,
            VTFFormat.IMAGE_FORMAT_DXT5 => Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,

            // Single channel grayscale
            VTFFormat.IMAGE_FORMAT_I8 => Veldrid.PixelFormat.R8_UNorm,

            // Two channel (Intensity + Alpha)
            VTFFormat.IMAGE_FORMAT_IA88 => Veldrid.PixelFormat.R8_G8_UNorm,

            // 24-bit RGB (no alpha) - no exact 24-bit RGB in Veldrid,
            // fallback to R8_G8_B8_A8_UNorm and handle alpha=1 in shader or code
            VTFFormat.IMAGE_FORMAT_RGB888 => Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,
            VTFFormat.IMAGE_FORMAT_BGR888 => Veldrid.PixelFormat.B8_G8_R8_A8_UNorm,

            // 16-bit packed RGB565 (no alpha)
            VTFFormat.IMAGE_FORMAT_RGB565 => Veldrid.PixelFormat.R16_UNorm, // no direct RGB565 in Veldrid, fallback to R16_UNorm (or handle manually)
            VTFFormat.IMAGE_FORMAT_BGR565 => Veldrid.PixelFormat.R16_UNorm,

            // 16-bit packed BGRA4444 or BGRA5551 - no direct equivalent in Veldrid, fallback
            VTFFormat.IMAGE_FORMAT_BGRA4444 => Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,
            VTFFormat.IMAGE_FORMAT_BGRA5551 => Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,
            VTFFormat.IMAGE_FORMAT_BGRX5551 => Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,

            // Palettized / unknown formats fallback
            VTFFormat.IMAGE_FORMAT_P8 => Veldrid.PixelFormat.R8_UNorm,
            VTFFormat.IMAGE_FORMAT_A8 => Veldrid.PixelFormat.R8_UNorm,

            // UV formats fallback to RG formats or 4 channels depending on assumed layout
            VTFFormat.IMAGE_FORMAT_UV88 => Veldrid.PixelFormat.R8_G8_UNorm,
            VTFFormat.IMAGE_FORMAT_UVWQ8888 => Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,
            VTFFormat.IMAGE_FORMAT_UVLX8888 => Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,

            // Bluescreen formats fallback to RGB888 variants with alpha = 1
            VTFFormat.IMAGE_FORMAT_RGB888_BLUESCREEN => Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,
            VTFFormat.IMAGE_FORMAT_BGR888_BLUESCREEN => Veldrid.PixelFormat.B8_G8_R8_A8_UNorm,

            // None / unknown
            VTFFormat.IMAGE_FORMAT_NONE => throw new InvalidDataException("VTF format NONE is invalid for pixel data."),

            // Default fallback
            _ => LogAndFallback(format)
        };

    private static Veldrid.PixelFormat LogAndFallback(VTFFormat format)
    {
        Console.Error.WriteLine($"[VTFImporter] Unsupported VTF format: {format}");
        return Veldrid.PixelFormat.R8_UNorm;
    }
}

[CustomEditor(typeof(VTFImporter))]
public class VTFImporterEditor : ScriptedEditor
{
    public override void OnInspectorGUI(EditorGUI.FieldChanges changes)
    {
        var importer = (VTFImporter)(target as MetaFile).importer;

        gui.CurrentNode.Layout(LayoutType.Column);

        if (EditorGUI.DrawProperty(0, "Mipmap Generation Mode", importer, "MipmapGeneration"))
            changes.Add(importer, nameof(VTFImporter.MipmapGeneration));
        if (EditorGUI.DrawProperty(1, "Min Filter", importer, "TextureMinFilter"))
            changes.Add(importer, nameof(VTFImporter.TextureMinFilter));
        if (EditorGUI.DrawProperty(2, "Mag Filter", importer, "TextureMagFilter"))
            changes.Add(importer, nameof(VTFImporter.TextureMagFilter));
        if (EditorGUI.DrawProperty(3, "Wrap Mode", importer, "TextureWrap"))
            changes.Add(importer, nameof(VTFImporter.TextureWrap));

        if (EditorGUI.StyledButton("Save"))
        {
            (target as MetaFile).Save();
            AssetDatabase.Reimport((target as MetaFile).AssetPath);
        }
    }
}
