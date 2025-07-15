// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using Prowl.Icons;

namespace Prowl.Runtime.Nodes.Scripting;

/// <summary>
/// Logs an error to the console.
/// </summary>
public class ErrorNode : ScriptNode
{
    public override string Name => $"{FontAwesome6.X}   Log Error";

    public override Color Color => new(222, 70, 62);

    public override object? Execute()
    {
        Debug.LogError("Error Node Fired!");

        return null;
    }
}
