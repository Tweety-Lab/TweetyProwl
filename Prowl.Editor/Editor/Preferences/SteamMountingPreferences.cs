// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System.Diagnostics.Metrics;
using System.Reflection;

using Prowl.Echo;
using Prowl.Editor.Assets;
using Prowl.Runtime;
using Prowl.Runtime.SteamMounting;
using Prowl.Runtime.Utils;
using Prowl.Runtime.Utils.Steam;

namespace Prowl.Editor.Editor.Preferences;

[FilePath("SteamMonting.pref", FilePathAttribute.Location.EditorPreference)]
public class SteamMountingPreferences : ScriptableSingleton<SteamMountingPreferences>
{
    [Tooltip("Should this game mount steam games?")]
    public bool UseSteamMounting = false;

    [Header("Mounting")]
    [ShowIf("UseSteamMounting")]
    [Tooltip("List of game IDs to mount.")]
    public List<int> MountedGameIDs = new List<int>();

    [ShowIf("UseSteamMounting")]
    [Tooltip("Folder to save mounted asset references to.")]
    public string AssetFolderName = "Mounted";

    [GUIButton("Refresh Mounted Assets")]
    public static void RefreshMountedAssets()
    {
        if (!Instance.UseSteamMounting)
            return;

        Console.WriteLine("[SteamMount] Refreshing Mounted Steam Assets...");

        // Ensure "Assets/Mounted" folder exists
        var assetsFolder = AssetDatabase.GetRootFolders()
            .FirstOrDefault(x => x.Name == "Assets");

        if (assetsFolder == null)
            return;

        var mountedFolderPath = Path.Combine(assetsFolder.FullName, Instance.AssetFolderName);
        Directory.CreateDirectory(mountedFolderPath);

        // Create a folder for each mounted game inside the mounted folder
        foreach (var gameID in Instance.MountedGameIDs)
        {
            var installDir = SteamUtils.GetAppInstallDir(gameID);
            if (installDir == null)
            {
                Console.Error.WriteLine($"[SteamMount] Could not find install directory for AppID {gameID}.");
                continue;
            }

            var gameFolderPath = Path.Combine(mountedFolderPath, gameID.ToString());
            Directory.CreateDirectory(gameFolderPath);

            foreach (GameMounter mounter in SteamMounting.Mounters)
            {
                if (mounter.AppID == gameID)
                {
                    List<Material> materials = mounter.MountMaterials(installDir);
                    foreach (Material mat in materials)
                    {
                        // Generate a unique file path in the mounted game folder
                        FileInfo file = new FileInfo(Path.Combine(gameFolderPath, $"{mat.Name}.mat"));
                        AssetDatabase.GenerateUniqueAssetPath(ref file);

                        // Serialize asset to the file
                        Serializer.Serialize(mat).WriteToString(file);

                        // Refresh asset database so editor recognizes the new asset
                        AssetDatabase.Update();
                        AssetDatabase.Ping(file);

                        Console.WriteLine($"[SteamMount] Added {mat.Name} at {file.FullName}");
                    }

                    break;
                }
            }
        }
    }
}
