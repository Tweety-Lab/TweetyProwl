// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;

using Prowl.Runtime.GUI;

namespace Prowl.Runtime.Nodes.Scripting;

/// <summary>
/// Node Port for Node flow.
/// </summary>
public class FlowPort : NodePort
{
    public FlowPort(string name, PortDirection dir, int index, Node node) : base(name, dir, index, node) { }

    public FlowPort() : base() { }

    public override void Draw(Gui gui)
    {
        const float size = 10f;

        if (Direction == PortDirection.Input)
        {
            // Centered square
            Vector2 topLeft = Position - new Vector2(size / 2f, size / 2f);
            gui.Draw2D.DrawRect(topLeft, new Vector2(size, size), Color.white, thickness: 2f);

            if (ConnectedPorts.Count > 0)
                gui.Draw2D.DrawRectFilled(topLeft, new Vector2(size, size), Color.white);
        }
        else
        {
            // Right facing triangle 
            const float triangleSize = 8f;

            Vector2 p1 = Position + new Vector2(-triangleSize / 2f, -triangleSize / 2f); // top-left
            Vector2 p2 = Position + new Vector2(-triangleSize / 2f, triangleSize / 2f);  // bottom-left
            Vector2 p3 = Position + new Vector2(triangleSize / 2f, 0);                   // right tip

            if (ConnectedPorts.Count > 0)
                gui.Draw2D.DrawTriangleFilled(p1, p2, p3, Color.white);
            else
                gui.Draw2D.DrawTriangle(p1, p2, p3, Color.white, thickness: 1.5f);
        }
    }

    // Flow Ports can only connect to Flow ports
    public override bool CanConnectTo(NodePort other) => other is FlowPort;
}
