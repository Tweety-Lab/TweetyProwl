// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.



using Prowl.Icons;
using Prowl.Runtime.GUI;

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

    /// <summary>
    /// Position of the Node in the Graph.
    /// </summary>
    public Vector2 Position = new(100, 100);

    private bool _isDragging = false;
    private Vector2 _dragOffset;

    /// <summary>
    /// Draw the visuals of the Node.
    /// </summary>
    public virtual void Draw(Gui gui, int id)
    {
        using (gui.Node("Node", id).Width(200).Height(100).Left(Position.x).Top(Position.y).Layout(LayoutType.Column).Enter())
        {
            // Header
            using (gui.Node("Header", id).Height(20).ExpandWidth().Enter())
            {
                Interactable interact = gui.GetInteractable();
                Rect rect = gui.CurrentNode.LayoutData.Rect;

                gui.Draw2D.DrawRectFilled(rect, Color, 6, CornerRounding.Top);

                // Start dragging
                if (!_isDragging && interact.IsHovered() && gui.IsPointerDown(MouseButton.Left))
                {
                    _isDragging = true;
                    _dragOffset = gui.PointerPos - Position;
                }

                // Perform drag
                if (_isDragging)
                    if (gui.IsPointerDown(MouseButton.Left))
                        Position = gui.PointerPos - _dragOffset;
                    else
                        _isDragging = false;

                gui.Draw2D.DrawText(Name, 16, rect.Position + new Vector2(8, 4), Color * 2f);
            }

            // Body
            using (gui.Node("Body", id).Expand().Enter())
            {
                Rect bodyRect = gui.CurrentNode.LayoutData.Rect;
                gui.Draw2D.DrawRectFilled(bodyRect, Color * 0.8f, 6, CornerRounding.Bottom);
            }
        }
    }
}
