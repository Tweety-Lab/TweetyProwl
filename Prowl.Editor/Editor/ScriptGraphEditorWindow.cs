// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using Prowl.Runtime;
using Prowl.Runtime.Nodes.Scripting;
using Prowl.Runtime.Resources;

namespace Prowl.Editor.Editor;

public class ScriptGraphEditorWindow : NodeGraphEditorWindow
{
    public ScriptGraphEditorWindow(NodeGraph graph) : base(graph)
    {
    }

    protected override void DrawNodeConnections()
    {
        base.DrawNodeConnections();

        foreach (var node in OpenedGraph.Nodes)
        {
            ScriptNode scriptNode = node as ScriptNode;
            if (scriptNode == null)
                continue;
        }
    }
}
