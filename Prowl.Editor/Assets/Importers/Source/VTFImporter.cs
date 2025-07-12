// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using Prowl.Runtime.GUI;
using Prowl.Runtime.Rendering;
using Prowl.Runtime.Utils;

using Sledge.Formats.Texture;
using Sledge.Formats.Texture.Vtf;

using Veldrid;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Prowl.Editor.Assets.Importers.Source;

[Importer("SourceIcon.png", typeof(Texture2D), ".vtf")]
public class VTFImporter : ScriptedImporter
{
    public TextureWrapMode TextureWrap = TextureWrapMode.Wrap;

    public enum MipmapGenerationMode { Default, DontGenerate, GenerateMipmaps}

    public MipmapGenerationMode MipmapGeneration = MipmapGenerationMode.Default;

    public FilterType TextureMinFilter = FilterType.Linear;
    public FilterType TextureMagFilter = FilterType.Linear;
    public FilterType TextureMipFilter = FilterType.Linear;

    private record struct VTFResource(string Tag, byte Flags, uint Offset);

    public override void Import(SerializedAsset ctx, FileInfo assetPath)
    {
        Texture2D texture = LoadVTFTexture(assetPath, true ? MipmapGeneration == MipmapGenerationMode.GenerateMipmaps : false);

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
    public static Texture2D LoadVTFTexture(FileInfo assetPath, bool generateMipmaps)
    {
        // Start stream
        using var stream = assetPath.OpenRead();

        VtfFile vtf = new VtfFile(stream);

        TextureUsage usage = TextureUsage.Sampled;

        if (generateMipmaps)
            usage |= TextureUsage.GenerateMipmaps;

        Texture2D texture = new Texture2D(
            (uint)vtf.Images[^1].Width,
            (uint)vtf.Images[^1].Height,
            0,
            PixelFormat.B8_G8_R8_A8_UNorm,
            usage
        );

        // Upload only the highest resolution image (for now)
        texture.SetData<byte>(vtf.Images[^1].GetBgra32Data());

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
