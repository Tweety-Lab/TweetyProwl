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

    /// <summary>
    /// Draw the visuals of the Node.
    /// </summary>
    public virtual void Draw(Gui gui, int id)
    {
        using (gui.Node("NodeBody", id).Width(200).Height(100).Enter())
        {
            gui.Draw2D.DrawRectFilled(gui.CurrentNode.LayoutData.Rect, Color.white);
        }
    }
}
