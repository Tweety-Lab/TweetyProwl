// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System.Collections.Generic;

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
            ConnectedPorts.Add(other);
        if (!other.ConnectedPorts.Contains(this))
            other.ConnectedPorts.Add(this);
    }

    public void DisconnectFrom(NodePort other)
    {
        ConnectedPorts.Remove(other);
        other.ConnectedPorts.Remove(this);
    }
}
