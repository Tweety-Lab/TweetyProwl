// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System.Collections.Generic;

namespace Prowl.Runtime.SteamMounting;

/// <summary>
/// Define behaviour for mounting specific games.
/// </summary>
public abstract class GameMounter
{
    /// <summary>
    /// The AppID of the game this class is responsible for.
    /// </summary>
    public abstract int AppID { get; }

    /// <summary>
    /// Mount materials from the game.
    /// </summary>
    public abstract List<Material> MountMaterials(string gamePath);

    public GameMounter()
    {
        SteamMounting.Mounters.Add(this);
    }
}
