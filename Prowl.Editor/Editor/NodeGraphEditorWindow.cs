// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using Prowl.Icons;
using Prowl.Runtime.Nodes;
using Prowl.Runtime.Resources;

namespace Prowl.Editor.Editor;

public class NodeGraphEditorWindow : EditorWindow
{
    /// <summary>
    /// Currently Opened Graph.
    /// </summary>
    public NodeGraph OpenedGraph { get; set; }

    public NodeGraphEditorWindow(NodeGraph graph) : base()
    {
        Title = FontAwesome6.CodeFork + "   Node Graph Editor";
        OpenedGraph = graph;
    }

    protected override void Draw()
    {
        if (OpenedGraph == null)
        {
            gui.TextNode("NoGraph", "No Graph Opened.").Expand();
            return;
        }

        foreach(Node node in OpenedGraph.Nodes)
        {
            Console.WriteLine(node.Name);
        }
    }
}
