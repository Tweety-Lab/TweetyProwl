// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

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
    public Color Color;
    public int Index;

    /// <summary>
    /// The Input node that this Output port is connected to.
    /// </summary>
    public NodePort ConnectedTo;

    public NodePort(string name, PortDirection dir, Color color, int index)
    {
        Name = name;
        Direction = dir;
        Color = color;
        Index = index;
    }
}
