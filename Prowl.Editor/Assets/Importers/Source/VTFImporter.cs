// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System.Text;

using Prowl.Editor.Utilities.Source;
using Prowl.Runtime;
using Prowl.Runtime.GUI;
using Prowl.Runtime.Rendering;
using Prowl.Runtime.Utils;

using SourceFormats.NET.VTF;

using Vortice.DXGI;

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

[Importer("SourceIcon.png", typeof(Texture2D), ".vtf")]
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

    // Map VTF format to Veldrid PixelFormat
    private static readonly Dictionary<VTFFormat, Veldrid.PixelFormat> PixelFormatMap = new()
    {
        // 32-bit formats with alpha
        [VTFFormat.IMAGE_FORMAT_RGBA8888] = Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,
        [VTFFormat.IMAGE_FORMAT_ABGR8888] = Veldrid.PixelFormat.R8_G8_B8_A8_UNorm, // Note: No exact match in Veldrid
        [VTFFormat.IMAGE_FORMAT_ARGB8888] = Veldrid.PixelFormat.R8_G8_B8_A8_UNorm, // Note: No exact match in Veldrid
        [VTFFormat.IMAGE_FORMAT_BGRA8888] = Veldrid.PixelFormat.B8_G8_R8_A8_UNorm,
        [VTFFormat.IMAGE_FORMAT_BGRX8888] = Veldrid.PixelFormat.B8_G8_R8_A8_UNorm,

        // 16-bit float format with alpha
        [VTFFormat.IMAGE_FORMAT_RGBA16161616F] = Veldrid.PixelFormat.R16_G16_B16_A16_Float,

        // 16-bit unsigned normalized RGBA
        [VTFFormat.IMAGE_FORMAT_RGBA16161616] = Veldrid.PixelFormat.R16_G16_B16_A16_UNorm,

        // Compressed formats (decompressed to RGBA)
        [VTFFormat.IMAGE_FORMAT_DXT1] = Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,
        [VTFFormat.IMAGE_FORMAT_DXT1_ONEBITALPHA] = Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,
        [VTFFormat.IMAGE_FORMAT_DXT3] = Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,
        [VTFFormat.IMAGE_FORMAT_DXT5] = Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,

        // Single channel formats
        [VTFFormat.IMAGE_FORMAT_I8] = Veldrid.PixelFormat.R8_UNorm,
        [VTFFormat.IMAGE_FORMAT_A8] = Veldrid.PixelFormat.R8_UNorm,
        [VTFFormat.IMAGE_FORMAT_P8] = Veldrid.PixelFormat.R8_UNorm,

        // Two channel formats
        [VTFFormat.IMAGE_FORMAT_IA88] = Veldrid.PixelFormat.R8_G8_UNorm,
        [VTFFormat.IMAGE_FORMAT_UV88] = Veldrid.PixelFormat.R8_G8_UNorm,

        // 24-bit RGB formats
        [VTFFormat.IMAGE_FORMAT_RGB888] = Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,
        [VTFFormat.IMAGE_FORMAT_BGR888] = Veldrid.PixelFormat.B8_G8_R8_A8_UNorm,
        [VTFFormat.IMAGE_FORMAT_RGB888_BLUESCREEN] = Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,
        [VTFFormat.IMAGE_FORMAT_BGR888_BLUESCREEN] = Veldrid.PixelFormat.B8_G8_R8_A8_UNorm,

        // 16-bit packed formats
        [VTFFormat.IMAGE_FORMAT_RGB565] = Veldrid.PixelFormat.R16_UNorm,
        [VTFFormat.IMAGE_FORMAT_BGR565] = Veldrid.PixelFormat.R16_UNorm,
        [VTFFormat.IMAGE_FORMAT_BGRA4444] = Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,
        [VTFFormat.IMAGE_FORMAT_BGRA5551] = Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,
        [VTFFormat.IMAGE_FORMAT_BGRX5551] = Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,

        // Special formats
        [VTFFormat.IMAGE_FORMAT_UVWQ8888] = Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,
        [VTFFormat.IMAGE_FORMAT_UVLX8888] = Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,
    };

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
        // Start stream
        using var stream = assetPath.OpenRead();

        // Parse
        VTFFile file = new VTFFile(stream);

        Texture2D texture = new Texture2D((uint)file.Header.Width, (uint)file.Header.Height);
        texture.SetData<byte>(file.MipMaps[0].PixelData);

        return texture;
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
