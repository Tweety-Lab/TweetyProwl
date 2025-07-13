// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using Prowl.Editor.Assets;
using Prowl.Editor.Preferences;
using Prowl.Icons;
using Prowl.Runtime;
using Prowl.Runtime.GUI;
using Prowl.Runtime.Nodes;
using Prowl.Runtime.Nodes.Shader;
using Prowl.Runtime.Resources;

namespace Prowl.Editor.Editor;

public class NodeGraphEditorWindow : EditorWindow
{
    /// <summary>
    /// Currently Opened Graph.
    /// </summary>
    public NodeGraph OpenedGraph { get; set; }

    // Currently selected port
    private NodePort? _selectedPort;

    private Dictionary<NodePort, Vector2> _globalPortPositions = new();

    public NodeGraphEditorWindow(NodeGraph graph) : base()
    {
        Title = FontAwesome6.CodeFork + "   Node Graph Editor";
        OpenedGraph = graph;

        foreach (var node in OpenedGraph.Nodes)
        {
            node.OnPortClicked += (port) =>
            {
                if (_selectedPort != null && _selectedPort != port)
                {
                    // Only connect output -> input
                    if (_selectedPort.Direction == PortDirection.Output && port.Direction == PortDirection.Input)
                        _selectedPort.ConnectTo(port);
                    else if (port.Direction == PortDirection.Output && _selectedPort.Direction == PortDirection.Input)
                        port.ConnectTo(_selectedPort);

                    Console.WriteLine($"Connected {_selectedPort.Name} to {port.Name}");

                    _selectedPort = null;
                }
                else
                {
                    _selectedPort = port;
                }
            };
        }
    }

    protected override void Draw()
    {
        gui.CurrentNode.Layout(LayoutType.Column);
        gui.CurrentNode.ScaleChildren();
        gui.CurrentNode.Spacing(10);
        gui.CurrentNode.Padding(10);

        _globalPortPositions.Clear();

        if (OpenedGraph == null)
        {
            gui.TextNode("NoGraph", "No Graph Opened.").Expand();

            if (DragnDrop.Drop<NodeGraph>(out NodeGraph? graph))
                OpenedGraph = graph;

            return;
        }

        using (gui.Node("TopBar").Layout(LayoutType.Row).ExpandWidth().MaxHeight(25).Enter())
        {
            gui.Draw2D.DrawRectFilled(gui.CurrentNode.LayoutData.Rect, EditorStylePrefs.Instance.Borders);
            if (EditorGUI.StyledButton(FontAwesome6.Plus + " Add Node", 80, 25, false))
                OpenedGraph.Nodes.Add(new Node());

            if (EditorGUI.StyledButton(FontAwesome6.FloppyDisk, 25, 25, false))
                AssetDatabase.SaveAsset(OpenedGraph);
        }

        // DEBUG: Make node on space
        if (gui.IsKeyPressed(Runtime.Key.Space))
            OpenedGraph.Nodes.Add(new Texture2DNode());

        using (gui.Node("NodeArea").Layout(LayoutType.None).Expand().Enter())
        {
            for (int i = 0; i < OpenedGraph.Nodes.Count; i++)
            {
                OpenedGraph.Nodes[i].Draw(gui, _globalPortPositions, i);
            }
        }

        // Draw all connections
        foreach (var node in OpenedGraph.Nodes)
        {
            foreach (var port in node.Inputs.Concat(node.Outputs))
            {
                foreach (var connected in port.ConnectedPorts)
                {
                    Console.WriteLine($"Trying to draw connection: {port.Name} -> {connected.Name}");
                    if (_globalPortPositions.TryGetValue(port, out var a) &&
                        _globalPortPositions.TryGetValue(connected, out var b))
                    {
                        Console.WriteLine($"Drawing line from {a} to {b}");
                        gui.Draw2D.DrawLine(a, b, Color.white);
                    }
                }
            }
        }

    }
}
