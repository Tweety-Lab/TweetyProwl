// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;
using System.Collections.Generic;

using Prowl.Echo;
using Prowl.Runtime.GUI;

namespace Prowl.Runtime.Nodes;

public enum PortDirection
{
    Input,
    Output
}

public class NodePort
{
    public string Name;
    public PortDirection Direction;
    public int Index;

    [SerializeIgnore]
    public Vector2 Position;

    // Owning Node
    public Node Node;

    public List<NodePort> ConnectedPorts = new List<NodePort>();

    public NodePort()
    {

    }

    public NodePort(string name, PortDirection dir, int index, Node node)
    {
        Name = name;
        Direction = dir;
        Index = index;
        Node = node;
    }

    public virtual void Draw(Gui gui)
    {
        if (ConnectedPorts.Count > 0)
            gui.Draw2D.DrawCircleFilled(Position, 5f, Color.white);
        else
            gui.Draw2D.DrawCircle(Position, 5f, Color.white, thickness: 2.5f);
    }

    /// <summary>
    /// Returns true if the port can be connected to.
    /// </summary>
    public virtual bool CanConnectTo(NodePort other) => true;

    public virtual void ConnectTo(NodePort other)
    {
        if (!ConnectedPorts.Contains(other))
        {
            ConnectedPorts.Add(other);
            Console.WriteLine($"[Connect] {Name} -> {other.Name}");
        }

        if (!other.ConnectedPorts.Contains(this))
        {
            other.ConnectedPorts.Add(this);
            Console.WriteLine($"[Connect] {other.Name} <- {Name}");
        }
    }

    public virtual void DisconnectFrom(NodePort other)
    {
        ConnectedPorts.Remove(other);
        other.ConnectedPorts.Remove(this);
    }
}
