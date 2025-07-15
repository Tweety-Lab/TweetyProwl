// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System.Linq;

using Prowl.Runtime.Nodes.Scripting;

namespace Prowl.Runtime.Resources;

/// <summary>
/// A Visual Scripting Node Graph.
/// </summary>
public class ScriptGraph : NodeGraph
{
    /// <summary>
    /// Runs this graph's script.
    /// </summary>
    public void Invoke()
    {
        // The start point of the script is the first found OnInvokeNode
        OnInvokeNode node = Nodes.First(n => n is OnInvokeNode) as OnInvokeNode;
    }
}
