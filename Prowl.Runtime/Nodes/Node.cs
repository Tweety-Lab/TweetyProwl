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
    public virtual string Name { get; set; } = $"{FontAwesome6.CircleNodes} Node";

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
            using (gui.Node("Header", id).Height(25).ExpandWidth().Enter())
            {
                Interactable interact = gui.GetInteractable();
                Rect rect = gui.CurrentNode.LayoutData.Rect;

                gui.Draw2D.DrawRectFilled(rect, Color.white, 6, CornerRounding.Top);

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

                gui.Draw2D.DrawText(Name, 16, rect.Position + new Vector2(8, 4), Color.white);
            }

            // Body
            using (gui.Node("Body", id).Expand().Enter())
            {
                Rect bodyRect = gui.CurrentNode.LayoutData.Rect;
                gui.Draw2D.DrawRectFilled(bodyRect, HashToColor(this.GetHashCode()), 6, CornerRounding.Bottom);
            }
        }
    }

    private static Color HashToColor(int hash)
    {
        byte r = (byte)(128 + (hash >> 16 & 0x7F));
        byte g = (byte)(128 + (hash >> 8 & 0x7F));
        byte b = (byte)(128 + (hash & 0x7F));

        return new Color32(r, g, b, 255);
    }
}
