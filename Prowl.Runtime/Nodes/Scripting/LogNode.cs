// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using Prowl.Icons;

namespace Prowl.Runtime.Nodes.Scripting;

/// <summary>
/// Logs a message to the console.
/// </summary>
public class LogNode : ScriptNode
{
    public override string Name => $"{FontAwesome6.EnvelopeOpenText}    Log Message";
    public override Color Color => new(50, 135, 80);

    public string Message;

    public override string Execute()
    {
        Debug.Log(Message);
        return "Output";
    }
}
