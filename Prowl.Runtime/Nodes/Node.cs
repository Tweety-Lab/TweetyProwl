// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.



using System;
using System.Collections.Generic;

using Prowl.Icons;
using Prowl.Runtime.GUI;
using Prowl.Runtime.GUI.Layout;

namespace Prowl.Runtime.Nodes;

/// <summary>
/// A single Node in a Node graph.
/// </summary>
public class Node
{
    /// <summary>
    /// Name/Title of the Node.
    /// </summary>
    public virtual string Name { get; } = $"{FontAwesome6.CircleNodes} Node";

    /// <summary>
    /// Visual Color of the Node.
    /// </summary>
    public virtual Color Color { get; set; } = new Color(51, 104, 176, 255);

    // IO
    public virtual List<NodePort> Inputs { get; } = [new NodePort("Test Input", PortDirection.Input, 0)];
    public virtual List<NodePort> Outputs { get; } = [new NodePort("Test Output", PortDirection.Output, 0)];

    /// <summary>
    /// Runs when a Node connection port is clicked.
    /// </summary>
    public Action<NodePort> OnPortClicked { get; set; }

    /// <summary>
    /// Position of the Node in the Graph.
    /// </summary>
    public Vector2 Position = new(100, 100);

    protected bool _isDragging = false;
    protected Vector2 _dragOffset;

    public void ConnectPorts(NodePort outputPort, NodePort inputPort) => outputPort.ConnectTo(inputPort);
    public void DisconnectPorts(NodePort outputPort, NodePort inputPort) => outputPort.DisconnectFrom(inputPort);

    /// <summary>
    /// Draw the visuals of the Node.
    /// </summary>
    public virtual void Draw(Gui gui, int id)
    {
        float height = CalculateNodeHeight(Inputs.Count, Outputs.Count);
        using (gui.Node("Node", id).Width(200).Height(height).Left(Position.x).Top(Position.y).Layout(LayoutType.Column).Enter())
        {
            DrawHeader(gui, id);
            DrawBody(gui, id);
        }
    }

    protected virtual void DrawHeader(Gui gui, int id)
    {
        using (gui.Node("Header", id).Height(20).ExpandWidth().Enter())
        {
            Interactable interact = gui.GetInteractable();
            Rect rect = gui.CurrentNode.LayoutData.Rect;

            gui.Draw2D.DrawRectFilled(rect, Color, 6, CornerRounding.Top);

            if (!_isDragging && interact.IsHovered() && gui.IsPointerDown(MouseButton.Left))
            {
                _isDragging = true;
                _dragOffset = gui.PointerPos - Position;
            }

            if (_isDragging)
            {
                if (gui.IsPointerDown(MouseButton.Left))
                    Position = gui.PointerPos - _dragOffset;
                else
                    _isDragging = false;
            }

            gui.Draw2D.DrawText(Name, 16, rect.Position + new Vector2(8, 4), Color * 2f);
        }
    }

    protected virtual void DrawBody(Gui gui, int id)
    {
        using (gui.Node("Body", id).Expand().Enter())
        {
            Rect bodyRect = gui.CurrentNode.LayoutData.Rect;
            gui.Draw2D.DrawRectFilled(bodyRect, Color * 0.8f, 6, CornerRounding.Bottom);

            gui.CurrentNode.Layout(LayoutType.Row);
            DrawInputPorts(gui);
            DrawOutputPorts(gui);
        }
    }

    protected virtual void DrawInputPorts(Gui gui)
    {
        using (gui.Node("Inputs").Width(100).Layout(LayoutType.Column).Spacing(5).Enter())
        {
            foreach (var input in Inputs)
                DrawPort(gui, input);
        }
    }

    protected virtual void DrawOutputPorts(Gui gui)
    {
        using (gui.Node("Outputs").Width(100).Layout(LayoutType.Column).Spacing(5).Enter())
        {
            foreach (var output in Outputs)
                DrawPort(gui, output, alignRight: true);
        }
    }

    private void DrawPort(Gui gui, NodePort port, bool alignRight = false)
    {
        using (gui.Node("Port", port.Index).Height(20).Layout(LayoutType.Row).Spacing(5).Enter())
        {
            Rect rect = gui.CurrentNode.LayoutData.Rect;
            Vector2 basePos = rect.Position;

            const float socketRadius = 5f;
            const float socketOffsetY = 10f;
            const float textOffsetY = 5f;

            if (!alignRight)
            {
                Vector2 textPos = basePos + new Vector2(10, textOffsetY);
                port.Position = basePos + new Vector2(0, socketOffsetY);

                gui.Draw2D.DrawText(port.Name, 14, textPos, Color.white);
                gui.Draw2D.DrawCircle(port.Position, socketRadius, Color.white, thickness: 2.5f);
                if (port.ConnectedPorts.Count > 0)
                    gui.Draw2D.DrawCircleFilled(port.Position, socketRadius, Color.white);
            }
            else
            {
                Vector2 textPos = basePos + new Vector2(40, textOffsetY);
                port.Position = rect.TopRight + new Vector2(100, socketOffsetY);

                gui.Draw2D.DrawText(port.Name, 14, textPos, Color.white);
                gui.Node("Spacer").Expand();
                gui.Draw2D.DrawCircle(port.Position, socketRadius, Color.white, thickness: 2.5f);
                if (port.ConnectedPorts.Count > 0)
                    gui.Draw2D.DrawCircleFilled(port.Position, socketRadius, Color.white);
            }

            Rect selectionRect = new Rect(port.Position.x - 5f, port.Position.y - 5f, 10f, 10f);
            if (gui.IsPointerClick() && gui.IsNodeHovered(selectionRect))
                OnPortClicked?.Invoke(port);
        }
    }

    private float CalculateNodeHeight(int inputCount, int outputCount)
    {
        const float portHeight = 20f;
        const float portSpacing = 5f;
        const float headerHeight = 20f;
        const float bodyPadding = 1f;

        float inputHeight = inputCount > 0 ? inputCount * portHeight + (inputCount - 1) * portSpacing : 0;
        float outputHeight = outputCount > 0 ? outputCount * portHeight + (outputCount - 1) * portSpacing : 0;

        float maxPortHeight = Math.Max(inputHeight, outputHeight);
        return headerHeight + maxPortHeight + bodyPadding * 2;
    }
}
