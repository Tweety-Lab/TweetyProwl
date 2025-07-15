// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

namespace Prowl.Runtime.Nodes.Scripting;

/// <summary>
/// Logs a message to the console.
/// </summary>
public class LogNode : ScriptNode
{
    public string Message;

    public override string Execute()
    {
        Debug.Log(Message);

        return "None";
    }
}
