// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using Prowl.Echo;
using Prowl.Runtime.Resources;
using Prowl.Runtime.Utils;

namespace Prowl.Editor.Assets.Importers;

[Importer("GameObjectIcon.png", typeof(ScriptGraph), ".sgraph")]
public class ScriptGraphImporter : NodeGraphImporter
{
    public override void Import(SerializedAsset ctx, FileInfo assetPath)
    {
        var tag = EchoObject.ReadFromString(assetPath);
        ScriptGraph? graph = Serializer.Deserialize<ScriptGraph>(tag) ?? throw new Exception("Failed to Deserialize Script Graph.");
        ctx.SetMainObject(graph);
    }
}
