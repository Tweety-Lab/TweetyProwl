// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;
using System.Collections.Generic;


namespace Prowl.Runtime.SteamMounting;

public class PortalMounter : GameMounter
{
    public override int AppID => 620;

    public override List<Material> MountMaterials(string gamePath)
    {
        List<Material> materials = new List<Material>();
        Material asset = Material.CreateDefaultMaterial();
        asset.AssetID = Guid.NewGuid();

        materials.Add(asset);

        return materials;
    }
}
