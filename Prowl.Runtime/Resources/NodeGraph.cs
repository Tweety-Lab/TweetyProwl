// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System.Collections.Generic;
using Prowl.Runtime.Nodes;

namespace Prowl.Runtime.Resources;

/// <summary>
/// Node Graph.
/// </summary>
public class NodeGraph : EngineObject
{
    /// <summary>
    /// All nodes in this graph.
    /// </summary>
    public List<Node> Nodes { get; set; } = new();
}
