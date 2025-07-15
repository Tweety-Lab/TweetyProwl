// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System.Linq;

using Prowl.Runtime.Nodes;
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
        // Start from the first OnInvokeNode
        var entryNode = Nodes.FirstOrDefault(n => n is OnInvokeNode) as OnInvokeNode;
        if (entryNode == null)
            return;

        // Find the first output-type NextNodePort from the entry node
        var startingPort = entryNode.Outputs.OfType<NextNodePort>().FirstOrDefault(p => p.Direction == PortDirection.Output);
        if (startingPort == null)
            return;

        // Traverse and execute each node in sequence
        ScriptNode current = GetNextScriptNode(startingPort);
        while (current != null)
        {
            current.Execute();

            var nextOutputPort = current.Outputs.OfType<NextNodePort>().FirstOrDefault(p => p.Direction == PortDirection.Output);
            current = GetNextScriptNode(nextOutputPort);
        }
    }

    /// <summary>
    /// Gets the next ScriptNode connected to a given NextNodePort.
    /// </summary>
    private ScriptNode GetNextScriptNode(NextNodePort port)
    {
        if (port == null)
            return null;

        var connected = port.ConnectedPorts?.FirstOrDefault();
        if (connected == null)
            return null;

        var nextNode = connected.Node as ScriptNode;
        if (nextNode == null)
            return null;

        return nextNode;
    }
}
