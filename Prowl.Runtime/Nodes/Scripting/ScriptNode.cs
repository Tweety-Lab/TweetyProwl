// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Reflection;

using Prowl.Echo;
using Prowl.Runtime.GUI;

namespace Prowl.Runtime.Nodes.Scripting;

/// <summary>
/// Visual Scripting Node.
/// </summary>
public class ScriptNode : Node
{
    /// <summary>
    /// Executes the node.
    /// </summary>
    public virtual object? Execute() => null;

    [SerializeField]
    private List<NodePort> _inputs;

    [SerializeField]
    private List<NodePort> _outputs;

    public override List<NodePort> Inputs
    {
        get
        {
            if (_inputs == null)
            {
                _inputs = new List<NodePort> { new FlowPort("Flow In", PortDirection.Input, -1, this) }; // Add flow port first
                var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                foreach (var field in fields)
                    _inputs.Add(new NodePort(RuntimeUtils.Prettify(field.Name), PortDirection.Input, _inputs.Count, this));
            }
            return _inputs;
        }
    }

    public override List<NodePort> Outputs
    {
        get
        {
            if (_outputs == null)
            {
                _outputs = new List<NodePort> { new FlowPort("Flow Out", PortDirection.Output, -1, this) }; // Add flow port first
                var method = GetType().GetMethod("Execute", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (method != null)
                {
                    var returnType = method.ReturnType;
                    if (returnType != typeof(void) && returnType != typeof(object))
                        _outputs.Add(new NodePort(RuntimeUtils.Prettify(returnType.Name), PortDirection.Output, _outputs.Count, this));
                }
            }
            return _outputs;
        }
    }
}
