// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.


// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.


using Prowl.Runtime;
using Prowl.Runtime.Utils;

using Sledge.Formats.Valve;

namespace Prowl.Editor.Assets.Importers.Source;

[Importer("SourceIcon.png", typeof(Material), ".vmt")]
public class VMTImporter : ScriptedImporter
{
    // Map source shader names to Prowl shaders
    private Dictionary<string, string> shaderMap = new()
    {
        { "unlitgeneric", "Default/StandardUnlit" },
        { "lightmappedgeneric", "Default/Standard" }
    };

    public override void Import(SerializedAsset ctx, FileInfo assetPath)
    {
        // Open stream
        using var stream = assetPath.OpenRead();

        // Read KeyValues
        SerialisedObject keyValues = new SerialisedObject(stream);

        // Read Data
        string shaderName = shaderMap.GetValueOrDefault(keyValues.Name.ToLowerInvariant()) ?? keyValues.Name;

        // Load shader
        AssetRef<Shader> shader = Application.AssetProvider.LoadAsset<Shader>(shaderName);

        Material output = new Material(shader);

        ctx.SetMainObject(output);
    }
}
