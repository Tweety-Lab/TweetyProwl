// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using Prowl.Echo;
using Prowl.Editor.Assets.Importers.Source;
using Prowl.Editor.Editor;
using Prowl.Runtime.GUI;
using Prowl.Runtime.Resources;
using Prowl.Runtime.Utils;

namespace Prowl.Editor.Assets.Importers;

[Importer("GameObjectIcon.png", typeof(NodeGraph), ".graph")]
public class NodeGraphImporter : ScriptedImporter
{
    public override void Import(SerializedAsset ctx, FileInfo assetPath)
    {
        var tag = EchoObject.ReadFromString(assetPath);
        NodeGraph? graph = Serializer.Deserialize<NodeGraph>(tag) ?? throw new Exception("Failed to Deserialize Node Graph.");
        ctx.SetMainObject(graph);
    }
}

[CustomEditor(typeof(NodeGraphImporter))]
public class NodeGraphEditor : ScriptedEditor
{
    public override void OnInspectorGUI(EditorGUI.FieldChanges changes)
    {
        var importer = (NodeGraphImporter)(target as MetaFile).importer;

        gui.CurrentNode.Layout(LayoutType.Column);
        EditorGUI.Text("Node Graph");
        if (EditorGUI.StyledButton("Edit"))
        {
            new NodeGraphEditorWindow((target as NodeGraph));
        }
    }
}
