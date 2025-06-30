// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.


// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.


using System.Text;

using Prowl.Runtime.Rendering;
using Prowl.Runtime.Utils;

namespace Prowl.Editor.Assets.Importers.Source;

[Importer("FileIcon.png", typeof(Texture2D), ".vtf")]
public class VTFImporter : ScriptedImporter
{
    public override void Import(SerializedAsset ctx, FileInfo assetPath)
    {
        Console.WriteLine($"Importing VTF file: {assetPath}");

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

        // Read VTF header
        // 4 character signature
        string signature = Encoding.ASCII.GetString(reader.ReadBytes(4));
        if (signature != "VTF\0")
            throw new InvalidDataException("Invalid VTF file signature.");

        // Version number
        uint versionMajor = reader.ReadUInt16();
        uint versionMinor = reader.ReadUInt16();

        // Store version as float so we can do checks like "if (version >= 1.1)"
        float version = versionMajor + versionMinor / 100f;

        reader.ReadUInt16(); // Padding?
        reader.ReadUInt16(); // HeaderSize

        ushort width = reader.ReadUInt16();
        ushort height = reader.ReadUInt16();

        Texture2D texture = new Texture2D(width, height);
        texture.Name = assetPath.Name;

        return texture;
    }
}
