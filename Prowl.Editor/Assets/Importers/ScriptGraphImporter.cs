// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using Prowl.Echo;
using Prowl.Editor.Editor;
using Prowl.Runtime;
using Prowl.Runtime.GUI;
using Prowl.Runtime.Resources;
using Prowl.Runtime.Utils;

namespace Prowl.Editor.Assets.Importers;

[Importer("GameObjectIcon.png", typeof(ScriptGraph), ".sgraph")]
public class ScriptGraphImporter : ScriptedImporter
{
    public override void Import(SerializedAsset ctx, FileInfo assetPath)
    {
        var tag = EchoObject.ReadFromString(assetPath);
        ScriptGraph? graph = Serializer.Deserialize<ScriptGraph>(tag) ?? throw new Exception("Failed to Deserialize Script Graph.");
        ctx.SetMainObject(graph);
    }
}

[CustomEditor(typeof(ScriptGraphImporter))]
public class ScriptGraphEditor : ScriptedEditor
{
    public override void OnInspectorGUI(EditorGUI.FieldChanges changes)
    {
        // Load asset info
        var meta = target as MetaFile;
        var importer = meta?.importer as NodeGraphImporter;
        var graph = Application.AssetProvider.LoadAsset<ScriptGraph>(meta.guid);

        // Draw
        gui.CurrentNode.Layout(LayoutType.Column);
        EditorGUI.Text("Script Graph");
        if (EditorGUI.StyledButton("Edit"))
            new ScriptGraphEditorWindow(graph.Res);
    }
}
