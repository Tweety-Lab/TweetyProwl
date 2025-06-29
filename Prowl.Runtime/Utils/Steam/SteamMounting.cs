// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System.Collections.Generic;
using System.IO;

namespace Prowl.Runtime.Utils.Steam;

/// <summary>
/// Interaction with Steam game mounting.
/// </summary>
public static class SteamMounting
{
    /// <summary>
    /// List of mounted Steam games.
    /// </summary>
    public static Dictionary<int, DirectoryInfo> MountedGames { get; set; } = new(); // AppID -> Directory
}
