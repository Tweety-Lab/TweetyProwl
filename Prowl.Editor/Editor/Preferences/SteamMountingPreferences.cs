// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using Prowl.Runtime;
using Prowl.Runtime.Utils;

namespace Prowl.Editor.Editor.Preferences;

[FilePath("SteamMonting.pref", FilePathAttribute.Location.EditorPreference)]
public class SteamMountingPreferences : ScriptableSingleton<SteamMountingPreferences>
{
    [Tooltip("Should this game mount steam games?")]
    public bool UseSteamMounting = false;

    [ShowIf("UseSteamMounting")]
    [Tooltip("List of game IDs to mount.")]
    public List<int> MountedGameIDs = new List<int>();
}
