﻿// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.


using Prowl.Editor.Assets;
using Prowl.Runtime;
using Prowl.Runtime.Utils;
using Prowl.Runtime.Utils.Steam;

namespace Prowl.Editor.Editor.ProjectSettings;

[FilePath("SteamMonting.projsetting", FilePathAttribute.Location.EditorSetting)]
public class SteamMountingPreferences : ScriptableSingleton<SteamMountingPreferences>
{
    [Tooltip("Should this game mount steam games?")]
    public bool UseSteamMounting = false;

    [Header("Mounting")]
    [ShowIf("UseSteamMounting")]
    [Tooltip("List of game IDs to mount.")]
    public List<int> MountedGameIDs = new List<int>();

    [GUIButton("Refresh Mounted Assets")]
    public static void RefreshMountedAssets()
    {
        if (!Instance.UseSteamMounting)
            return;

        Console.WriteLine("[SteamMount] Refreshing Mounted Steam Assets...");

        // Create a folder for each mounted game inside the mounted folder
        foreach (var gameID in Instance.MountedGameIDs)
        {
            var installDir = SteamUtils.GetAppInstallDir(gameID);
            if (installDir == null)
            {
                Console.Error.WriteLine($"[SteamMount] Could not find install directory for AppID {gameID}.");
                continue;
            }

            // Add to mounted games
            SteamMounting.MountedGames.Add(gameID, new DirectoryInfo(installDir));

            // Add to asset database
            foreach (var mounted in SteamMounting.MountedGames.Values)
                AssetDatabase.AddExternalRootFolder(mounted);
        }
    }
}
