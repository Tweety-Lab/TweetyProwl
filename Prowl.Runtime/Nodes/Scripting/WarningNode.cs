// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using Prowl.Icons;

namespace Prowl.Runtime.Nodes.Scripting;

/// <summary>
/// Logs a warning to the Console.
/// </summary>
public class WarningNode : ScriptNode
{
    public override string Name => $"{FontAwesome6.TriangleExclamation}    Log Warning";

    public override Color Color => new(222, 195, 62);

    public string Message;

    public override object? Execute()
    {
        Debug.LogWarning("Warning Node Fired!");

        return null;
    }
}
