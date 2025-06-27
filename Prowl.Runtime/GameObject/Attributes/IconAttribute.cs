// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;
using System.Reflection;

using Prowl.Icons;

namespace Prowl.Runtime;

/// <summary>
/// Specifies the icon for a component in the Prowl Game Engine's editor interface.
/// </summary>
/// <remarks>
/// This attribute can only be applied to classes and cannot be used multiple times on the same class.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class IconAttribute : Attribute
{
    /// <summary>
    /// Icon character code.
    /// </summary>
    public string IconCode { get; private set; }

    /// <summary>
    /// Initializes a new instance of the IconAttribute class.
    /// </summary>
    /// <param name="iconName">The icon name.</param>
    public IconAttribute(string iconName)
    {
        // Use reflection to convert icon name to code
        var field = typeof(FontAwesome6).GetField(iconName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        if (field is not null && field.IsLiteral && !field.IsInitOnly)
            IconCode = field.GetRawConstantValue().ToString();
    }
}
