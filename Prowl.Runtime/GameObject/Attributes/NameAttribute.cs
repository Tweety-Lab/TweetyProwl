// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prowl.Runtime;

/// <summary>
/// Specifies the name of a component in the Prowl Game Engine's editor interface.
/// </summary>
/// <remarks>
/// This attribute can only be applied to classes and cannot be used multiple times on the same class.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class NameAttribute : Attribute
{
    /// <summary>
    /// The name of the component in the Prowl Game Engine's editor interface.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Initializes a new instance of the NameAttribute class.
    /// </summary>
    /// <param name="name">Name of the component.</param>
    public NameAttribute(string name) => Name = name;
}
