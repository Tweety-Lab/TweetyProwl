// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;
using System.Collections.Generic;

using Prowl.Echo;

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

    public List<NodePort> ConnectedPorts = new List<NodePort>();

    public NodePort(string name, PortDirection dir, int index)
    {
        Name = name;
        Direction = dir;
        Index = index;
    }

    public void ConnectTo(NodePort other)
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

    public void DisconnectFrom(NodePort other)
    {
        ConnectedPorts.Remove(other);
        other.ConnectedPorts.Remove(this);
    }
}
