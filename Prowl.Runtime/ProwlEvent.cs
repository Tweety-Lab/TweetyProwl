// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;

namespace Prowl.Runtime;


/// <summary>
/// Prowl engine event.
/// </summary>
public class ProwlEvent
{
    /// <summary>
    /// Targeted GameObject.
    /// </summary>
    public GameObject Target = null;

    /// <summary>
    /// Name of the method to run.
    /// </summary>
    public string MethodName = string.Empty;

    /// <summary>
    /// Trigger the Prowl Event.
    /// </summary>
    public void Invoke() => Target.SendMessage(MethodName);
}
