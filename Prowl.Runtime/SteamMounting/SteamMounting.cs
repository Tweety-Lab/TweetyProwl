// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.IO;

namespace Prowl.Runtime.SteamMounting;

/// <summary>
/// Interaction with Steam Game mounting.
/// </summary>
public static class SteamMounting
{
    /// <summary>
    /// All registered GameMounters.
    /// </summary>
    public static List<GameMounter> Mounters { get; set; } = new List<GameMounter>();

    static SteamMounting()
    {
        new PortalMounter();
    }
}
