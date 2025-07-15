// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Reflection;

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

    public override List<NodePort> Inputs
    {
        get
        {
            var ports = new List<NodePort>();

            // Use reflection to find all instance fields on this type
            var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            foreach (var field in fields)
                ports.Add(new NodePort(RuntimeUtils.Prettify(field.Name), PortDirection.Input, ports.Count));

            return ports;
        }
    }

    public override List<NodePort> Outputs
    {
        get
        {
            var ports = new List<NodePort>();

            // Use reflection to output the return value of Execute()
            var method = GetType().GetMethod("Execute", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (method != null)
            {
                var returnType = method.ReturnType;

                // Only add a port if the return type is not void or explicitly object (aka unknown)
                if (returnType != typeof(void) && returnType != typeof(object))
                    ports.Add(new NodePort(RuntimeUtils.Prettify(returnType.Name), PortDirection.Output, ports.Count));
            }

            return ports;
        }
    }
}
