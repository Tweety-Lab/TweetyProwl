// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Prowl.Runtime.Utils.Steam;

/// <summary>
/// Interaction with Steam.
/// </summary>
public static class SteamUtils
{
    /// <summary>
    /// Get the install directory of a Steam app across Windows, Linux, and macOS.
    /// </summary>
    /// <param name="appID">Steam App ID of the app.</param>
    /// <returns>File path to the install directory, or null if not found.</returns>
    public static string? GetAppInstallDir(int appID)
    {
        var steamPath = GetDefaultSteamPath();
        if (string.IsNullOrEmpty(steamPath))
            return null;

        var libraryVdfPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
        if (!File.Exists(libraryVdfPath))
            return null;

        var libraryPaths = ParseLibraryFoldersVdf(libraryVdfPath);
        foreach (var libPath in libraryPaths)
        {
            var appManifest = Path.Combine(libPath, "steamapps", $"appmanifest_{appID}.acf");
            if (File.Exists(appManifest))
            {
                var installDir = ParseInstallDirFromManifest(appManifest);
                if (!string.IsNullOrEmpty(installDir))
                    return Path.Combine(libPath, "steamapps", "common", installDir);
            }
        }

        return null;
    }

    /// <summary>
    /// Get the default Steam install path.
    /// </summary>
    /// <returns>Default Steam install path, or null if not found.</returns>
    public static string? GetDefaultSteamPath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
                return key?.GetValue("SteamPath")?.ToString()?.Replace("/", "\\");
            }
            catch
            {
                return null;
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var home = Environment.GetEnvironmentVariable("HOME");
            return home == null ? null : Path.Combine(home, ".steam", "steam");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var home = Environment.GetEnvironmentVariable("HOME");
            return home == null ? null : Path.Combine(home, "Library", "Application Support", "Steam");
        }

        return null;
    }

    private static List<string> ParseLibraryFoldersVdf(string vdfPath)
    {
        var paths = new List<string>();

        foreach (var line in File.ReadAllLines(vdfPath))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("\"path\"") || trimmed.StartsWith("\"1\"") || trimmed.StartsWith("\"2\"") || trimmed.StartsWith("\"0\""))
            {
                var parts = trimmed.Split('"');
                if (parts.Length >= 4)
                {
                    var path = parts[3].Replace("\\\\", "\\").Replace("\\", Path.DirectorySeparatorChar.ToString());
                    if (Directory.Exists(path))
                        paths.Add(path);
                }
            }
        }

        // Always include main Steam path
        var mainSteam = Path.GetDirectoryName(Path.GetDirectoryName(vdfPath));
        if (mainSteam != null && Directory.Exists(mainSteam) && !paths.Contains(mainSteam))
            paths.Insert(0, mainSteam);

        return paths;
    }

    private static string? ParseInstallDirFromManifest(string manifestPath)
    {
        foreach (var line in File.ReadLines(manifestPath))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("\"installdir\""))
            {
                var parts = trimmed.Split('"');
                if (parts.Length >= 4)
                    return parts[3];
            }
        }

        return null;
    }
}
