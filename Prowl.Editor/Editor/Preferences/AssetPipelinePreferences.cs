// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using Prowl.Runtime;
using Prowl.Runtime.Utils;

namespace Prowl.Editor.Preferences;

[FilePath("AssetPipeline.pref", FilePathAttribute.Location.EditorPreference)]
public class AssetPipelinePreferences : ScriptableSingleton<AssetPipelinePreferences>
{
    [Header("Asset Browser")]
    public bool HideExtensions = true;
    public float ThumbnailSize = 0.0f;

    [Header("Pipeline")]
    public bool AutoImport = true;
}
