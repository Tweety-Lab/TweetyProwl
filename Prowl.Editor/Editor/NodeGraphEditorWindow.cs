// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using Prowl.Editor.Assets;
using Prowl.Editor.Preferences;
using Prowl.Icons;
using Prowl.Runtime;
using Prowl.Runtime.GUI;
using Prowl.Runtime.Nodes;
using Prowl.Runtime.Nodes.Scripting;
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

    public NodeGraphEditorWindow(NodeGraph graph) : base()
    {
        Title = FontAwesome6.CodeFork + "   Node Graph Editor";
        OpenedGraph = graph;

        foreach (var node in OpenedGraph.Nodes)
            node.OnPortClicked += HandlePortClicked;
    }

    protected override void Close()
    {
        if (OpenedGraph == null)
            return;

        foreach (var node in OpenedGraph.Nodes)
            node.OnPortClicked -= HandlePortClicked;

        OpenedGraph = null;
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

            if (DragnDrop.Drop<NodeGraph>(out NodeGraph? graph))
                OpenedGraph = graph;

            return;
        }

        using (gui.Node("TopBar").Layout(LayoutType.Row).ExpandWidth().MaxHeight(25).Enter())
        {
            gui.Draw2D.DrawRectFilled(gui.CurrentNode.LayoutData.Rect, EditorStylePrefs.Instance.Borders);
            if (EditorGUI.StyledButton(FontAwesome6.Plus + " Add OnInvoke Node", 80, 25, false))
            {
                var newNode = new OnInvokeNode();
                newNode.OnPortClicked += HandlePortClicked;
                OpenedGraph.Nodes.Add(newNode);
            }

            if (EditorGUI.StyledButton(FontAwesome6.Plus + " Add Wait Node", 80, 25, false))
            {
                var newNode = new WaitNode();
                newNode.OnPortClicked += HandlePortClicked;
                OpenedGraph.Nodes.Add(newNode);
            }

            if (EditorGUI.StyledButton(FontAwesome6.FloppyDisk, 25, 25, false))
                AssetDatabase.SaveAsset(OpenedGraph);
        }

        // DEBUG: Make node on space
        if (gui.IsKeyPressed(Key.Space))
        {
            var newNode = new LogNode();
            newNode.OnPortClicked += HandlePortClicked;
            OpenedGraph.Nodes.Add(newNode);
        }

        DrawNodeArea();
        DrawNodeConnections();
    }

    protected virtual void DrawNodeConnections()
    {
        foreach (var node in OpenedGraph.Nodes)
        {
            foreach (var port in node.Outputs)
            {
                foreach (var connected in port.ConnectedPorts)
                {
                    Vector2 a = port.Position;
                    Vector2 b = connected.Position;

                    float offset = MathF.Abs((float)(b.x - a.x)) * 0.5f;
                    Vector2 controlA = a + new Vector2(offset, 0f);
                    Vector2 controlB = b - new Vector2(offset, 0f);

                    gui.Draw2D.DrawBezierLine(a, controlA, b, controlB, Color.white, 3f);
                }
            }
        }

        if (_selectedPort == null)
            return;

        Vector2 from = _selectedPort.Position;
        Vector2 to = gui.PointerPos;
        gui.Draw2D.DrawLine(from, to, Color.white, 3f);
    }

    private void HandlePortClicked(NodePort port)
    {
        if (_selectedPort == null || _selectedPort == port)
        {
            _selectedPort = port;
            return;
        }

        if (_selectedPort.Direction == PortDirection.Output && port.Direction == PortDirection.Input && port.CanConnectTo(_selectedPort))
            _selectedPort.ConnectTo(port);
        else if (port.Direction == PortDirection.Output && _selectedPort.Direction == PortDirection.Input && _selectedPort.CanConnectTo(port))
            port.ConnectTo(_selectedPort);

        _selectedPort = null;
    }

    private void DrawNodeArea()
    {
        using (gui.Node("NodeArea").Layout(LayoutType.None).Expand().Enter())
            for (int i = 0; i < OpenedGraph.Nodes.Count; i++)
                OpenedGraph.Nodes[i].Draw(gui, i);
    }
}
