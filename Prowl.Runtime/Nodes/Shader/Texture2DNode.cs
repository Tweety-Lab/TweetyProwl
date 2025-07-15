// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System.Collections.Generic;

using Prowl.Icons;

namespace Prowl.Runtime.Nodes.Shader;

public class Texture2DNode : Node
{
    public override string Name => $"{FontAwesome6.Camera}  Texture2D";

    public override List<NodePort> Outputs => [new NodePort("Texture", PortDirection.Output, 0), new NodePort("Sampler", PortDirection.Output, 1)];

    public override Color Color => new(235, 64, 52, 255);
}
