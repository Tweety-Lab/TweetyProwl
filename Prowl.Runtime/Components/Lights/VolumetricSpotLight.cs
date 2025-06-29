// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Xml.Linq;

using Prowl.Runtime.Rendering;
using Prowl.Runtime.Rendering.Pipelines;
using Prowl.Runtime.Utils;

namespace Prowl.Runtime.Components.Lights;

[Icon("Lightbulb")]
[Name("Volumetrics/Volumetric Spot Light")]
[ExecuteAlways]
public class VolumetricSpotLight : SpotLight, IRenderable
{
    public PropertyState Properties;

    [Tooltip("The amount of scattering to apply to the volumetric cone.")]
    public float Scattering = 1f;

    [Tooltip("The number of steps to use when rendering the volumetric cone.")]
    public float StepCount = 64;

    private Material volumetricMaterial = null;
    private Mesh volumetricCone = null;

    public void GetCullingData(out bool isRenderable, out Bounds bounds)
    {
        isRenderable = _enabledInHierarchy;
        bounds = volumetricCone.bounds.Transform(Transform.localToWorldMatrix);
    }

    public int GetLayer() => GameObject.layerIndex;
    public Material GetMaterial() => volumetricMaterial;
    public void GetRenderingData(out PropertyState properties, out IGeometryDrawData drawData, out Matrix4x4 model)
    {
        drawData = volumetricCone;
        properties = Properties;
        model = GetModelMatrix();
    }

    public override void OnEnable()
    {
        var obj = Application.AssetProvider.LoadAsset<GameObject>("Defaults/Cone.obj").Res;
        volumetricCone = obj.GetComponentInChildren<MeshRenderer>().Mesh.Res;
        volumetricMaterial = new Material(Application.AssetProvider.LoadAsset<Shader>("Defaults/VolumetricSpot.shader"));

        UpdateMaterialProperties();
    }

    public override void Update()
    {
        if (volumetricCone == null || volumetricMaterial == null)
        {
            // Log which one is missing
            if (volumetricCone == null)
                Console.WriteLine($"Volumetric Spot Light {InstanceID} is missing a mesh.");

            if (volumetricMaterial == null)
                Console.WriteLine($"Volumetric Spot Light {InstanceID} is missing a material.");
            return;
        }

        Properties ??= new();
        Properties.SetInt("_ObjectID", InstanceID);

        RenderPipeline.AddRenderable(this);
        RenderPipeline.AddLight(this);
    }

    public override void OnValidate()
    {
        if (volumetricMaterial == null)
            return;

        UpdateMaterialProperties();
    }

    private void UpdateMaterialProperties()
    {
        if (volumetricMaterial == null)
            return;

        // Update all shader properties
        volumetricMaterial.SetColor("_MainColor", new Color(color.r, color.g, color.b, 0.25f));
        volumetricMaterial.SetFloat("_LightIntensity", intensity);
        volumetricMaterial.SetFloat("_Scattering", Scattering);
        volumetricMaterial.SetFloat("_StepCount", StepCount);

        // Convert prowl.runtime.matrix4x4 to numerics.matrix4x4
        var src = GetModelMatrix();

        var modelMatrix = new System.Numerics.Matrix4x4(
            (float)src.M11, (float)src.M12, (float)src.M13, (float)src.M14,
            (float)src.M21, (float)src.M22, (float)src.M23, (float)src.M24,
            (float)src.M31, (float)src.M32, (float)src.M33, (float)src.M34,
            (float)src.M41, (float)src.M42, (float)src.M43, (float)src.M44
        );

        volumetricMaterial.SetMatrix("_ModelMatrix", modelMatrix);
    }

    // Get the transformed model matrix of the cone 
    private Matrix4x4 GetModelMatrix()
    {
        float coneLength = distance;

        // Treat angle as cosine of half angle
        float halfAngleCos = angle;  // if angle=1 narrow, angle=0 wide
        float halfAngleRad = MathF.Acos(halfAngleCos);
        float fudge = 2.25f;
        float coneRadius = MathF.Tan(halfAngleRad) * coneLength * fudge;

        var scale = Matrix4x4.CreateScale(coneRadius, coneLength, coneRadius);
        var rotation = Matrix4x4.CreateRotationX(-MathF.PI / 2f);
        var matrix = new Matrix4x4();
        matrix = scale * rotation * Transform.localToWorldMatrix;

        return matrix;
    }
}
