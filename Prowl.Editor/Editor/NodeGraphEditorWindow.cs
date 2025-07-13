// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using Prowl.Editor.Preferences;
using Prowl.Icons;
using Prowl.Runtime;
using Prowl.Runtime.GUI;
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
        gui.CurrentNode.Layout(LayoutType.Column);
        gui.CurrentNode.ScaleChildren();
        gui.CurrentNode.Spacing(10);
        gui.CurrentNode.Padding(10);

        if (OpenedGraph == null)
        {
            gui.TextNode("NoGraph", "No Graph Opened.").Expand();
            return;
        }

        using (gui.Node("TopBar").Layout(LayoutType.Row).ExpandWidth().MaxHeight(25).Enter())
        {
            gui.Draw2D.DrawRectFilled(gui.CurrentNode.LayoutData.Rect, EditorStylePrefs.Instance.Borders);
            if (EditorGUI.StyledButton(FontAwesome6.Plus + " Add Node", 80, 25, false))
                OpenedGraph.Nodes.Add(new Node { Name = "Node " + OpenedGraph.Nodes.Count });
        }

        // DEBUG: Make node on space
        if (gui.IsKeyPressed(Runtime.Key.Space))
            OpenedGraph.Nodes.Add(new Node());

        using (gui.Node("NodeArea").Layout(LayoutType.None).Expand().Enter())
        {
            for (int i = 0; i < OpenedGraph.Nodes.Count; i++)
                OpenedGraph.Nodes[i].Draw(gui, i);
        }

    }
}
