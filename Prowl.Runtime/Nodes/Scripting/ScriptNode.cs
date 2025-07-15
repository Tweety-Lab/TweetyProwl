// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Reflection;

using Prowl.Runtime.GUI;

namespace Prowl.Runtime.Nodes.Scripting;

/// <summary>
/// Visual Scripting Node.
/// </summary>
public class ScriptNode : Node
{
    /// <summary>
    /// Executes the node.
    /// </summary>
    public virtual object? Execute() { return null; }

    /// <summary>
    /// The node to run after this one.
    /// </summary>
    public ScriptNode NextNode;

    protected override void DrawHeader(Gui gui, int id)
    {
        using (gui.Node("Header", id).Height(20).ExpandWidth().Enter())
        {
            Interactable interact = gui.GetInteractable();
            Rect rect = gui.CurrentNode.LayoutData.Rect;

            // Draw background
            gui.Draw2D.DrawRectFilled(rect, Color, 6, CornerRounding.Top);

            // Dragging behavior
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

            // Draw NextNode input socket on header left
            Vector2 inputPos = rect.TopLeft + new Vector2(10, 11);
            const float size = 10f;
            Vector2 topLeft = inputPos - new Vector2(size / 2f, size / 2f);

            gui.Draw2D.DrawRect(topLeft, new Vector2(size, size), Color.white, thickness: 2f);

            // Draw node title
            gui.Draw2D.DrawText(Name, 16, rect.Position + new Vector2(20, 4), Color * 2f);

            // Draw NextNode output socket on header right
            const float triangleSize = 8f;
            Vector2 socketPos = rect.TopRight - new Vector2(10 + triangleSize / 2f, -triangleSize / 2f + 5) + new Vector2(0, 12);


            // Right-facing triangle
            Vector2 p1 = socketPos + new Vector2(-triangleSize / 2f, -triangleSize / 2f); // top-left
            Vector2 p2 = socketPos + new Vector2(-triangleSize / 2f, triangleSize / 2f); // bottom-left
            Vector2 p3 = socketPos + new Vector2(triangleSize / 2f, 0);                 // right tip

            if (NextNode != null)
                gui.Draw2D.DrawTriangleFilled(p1, p2, p3, Color.white);
            else
                gui.Draw2D.DrawTriangle(p1, p2, p3, Color.white, thickness: 1.5f);
        }
    }

    private List<NodePort> _inputs;
    private List<NodePort> _outputs;

    public override List<NodePort> Inputs
    {
        get
        {
            if (_inputs == null)
            {
                _inputs = new List<NodePort>();
                var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                foreach (var field in fields)
                    _inputs.Add(new NodePort(RuntimeUtils.Prettify(field.Name), PortDirection.Input, _inputs.Count));
            }
            return _inputs;
        }
    }

    public override List<NodePort> Outputs
    {
        get
        {
            if (_outputs == null)
            {
                _outputs = new List<NodePort>();
                var method = GetType().GetMethod("Execute", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (method != null)
                {
                    var returnType = method.ReturnType;
                    if (returnType != typeof(void) && returnType != typeof(object))
                        _outputs.Add(new NodePort(RuntimeUtils.Prettify(returnType.Name), PortDirection.Output, _outputs.Count));
                }
            }
            return _outputs;
        }
    }
}
