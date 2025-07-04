// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;

using Prowl.Icons;

using Prowl.Runtime.Rendering;
using Prowl.Runtime.Rendering.Pipelines;

namespace Prowl.Runtime;


[ExecuteAlways]
[AddComponentMenu($"{FontAwesome6.Tv}  Rendering/{FontAwesome6.Shapes}  Mesh Renderer")]
public class MeshRenderer : MonoBehaviour, IRenderable
{
    [Tooltip("If this mesh should act like a Source Engine brush.")]
    public bool Brush = false;

    [ShowIf("Brush")]
    public float UVScale = 0.25f;

    public AssetRef<Mesh> Mesh;
    public AssetRef<Material> Material;

    public PropertyState Properties;

    public override void Update()
    {
        if (!Mesh.IsAvailable) return;
        if (!Material.IsAvailable) return;

        Properties ??= new();

        Properties.SetInt("_ObjectID", InstanceID);

        RenderPipeline.AddRenderable(this);
    }

    public Material GetMaterial() => Material.Res;
    public int GetLayer() => GameObject.LayerIndex;

    public void GetRenderingData(out PropertyState properties, out IGeometryDrawData drawData, out Matrix4x4 model)
    {
        drawData = Mesh.Res;
        properties = Properties;
        model = Transform.localToWorldMatrix;
    }

    public void GetCullingData(out bool isRenderable, out Bounds bounds)
    {
        isRenderable = _enabledInHierarchy;
        bounds = Mesh.Res.bounds.Transform(Transform.localToWorldMatrix);
    }

    public override void OnValidate()
    {
        if (Brush)
        {
            Mesh mesh = Mesh.Res;
            if (mesh == null)
                return;

            var vertices = mesh.Vertices;
            var normals = mesh.Normals;
            var uvs = new System.Numerics.Vector2[vertices.Length];
            var localToWorld = Transform.localToWorldMatrix;


            for (int i = 0; i < vertices.Length; i++)
            {
                var worldPos = Vector3.Transform(vertices[i], localToWorld);
                var worldNormal = Vector3.TransformNormal(normals[i], localToWorld).normalized;

                Vector2 uv;

                // Determine dominant axis
                var absNormal = new Vector3(MathF.Abs((float)worldNormal.x), MathF.Abs((float)worldNormal.y), MathF.Abs((float)worldNormal.z));

                if (absNormal.z >= absNormal.x && absNormal.z >= absNormal.y)
                    uv = new Vector2(worldPos.x, worldPos.y);
                else if (absNormal.y >= absNormal.x)
                    uv = new Vector2(worldPos.x, worldPos.z);
                else
                    uv = new Vector2(worldPos.y, worldPos.z);

                uv *= UVScale;
                uvs[i] = uv;
            }

            mesh.UV = uvs;
        }
    }
}
