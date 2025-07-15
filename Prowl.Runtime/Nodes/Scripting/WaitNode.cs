// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using Prowl.Icons;

namespace Prowl.Runtime.Nodes.Scripting;

public class WaitNode : ScriptNode
{
    public override string Name => $"{FontAwesome6.Hourglass}   Wait";

    public override Color Color => new(52, 158, 235);
}
