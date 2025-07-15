// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.


using Prowl.Icons;

namespace Prowl.Runtime.Nodes.Scripting;

/// <summary>
/// Node that defines the start of a Script.
/// </summary>
public class OnInvokeNode : ScriptNode
{
    public override string Name => $"{FontAwesome6.Play}   OnInvoke";

    public override Color Color => new(92, 85, 83);

    public override bool AcceptInput => false;
}
