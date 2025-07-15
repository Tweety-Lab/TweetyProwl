// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using Prowl.Runtime.Resources;

namespace Prowl.Runtime.Components.VisualScripting;

public class ScriptGraphInvoker : MonoBehaviour
{
    [Tooltip("The script graph to invoke on Start.")]
    public ScriptGraph OnStart;

    [Tooltip("The script graph to invoke on Update.")]
    public ScriptGraph OnUpdate;

    public override void Start() => OnStart?.Invoke();
    public override void Update() => OnUpdate?.Invoke();
}
