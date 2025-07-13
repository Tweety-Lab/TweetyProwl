// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using Prowl.Editor.Utilities;
using Prowl.Icons;
using Prowl.Runtime;
using Prowl.Runtime.GUI;
using Prowl.Runtime.Rendering;
using Prowl.Runtime.Rendering.Pipelines;
using Prowl.Runtime.SceneManagement;

namespace Prowl.Editor.Editor;

public class GeometryToolsWindow : EditorWindow
{
    public enum GeometryTool { Block, Clipping }

    /// <summary>
    /// Currently selected tool.
    /// </summary>
    public static GeometryTool Tool { get; set; }

    private SceneViewWindow sceneEditorWindow = null;
    private GameObject currentGeometry = null;
    private List<System.Numerics.Vector3> lassoPoints = new();

    public GeometryToolsWindow() : base()
    {
        Title = "Geo Tools";
    }

    protected override void Draw()
    {
        gui.CurrentNode.Layout(LayoutType.Column);
        gui.CurrentNode.ScaleChildren();

        if (EditorGUI.StyledButton(FontAwesome6.Cube))
            Tool = GeometryTool.Block;
        gui.Tooltip("Block Tool");

        EditorGUI.StyledButton(FontAwesome6.SheetPlastic);
        gui.Tooltip("Material Editor");

        if (EditorGUI.StyledButton(FontAwesome6.StarHalf))
            Tool = GeometryTool.Clipping;
        gui.Tooltip("Clipping Tool");

        HandleToolUsage();
    }

    private void HandleToolUsage()
    {
        if (sceneEditorWindow == null)
            sceneEditorWindow = (SceneViewWindow)EditorGuiManager.Windows.FirstOrDefault(w => w is SceneViewWindow);

        if (Tool == GeometryTool.Block && gui.IsPointerClick())
        {
            Ray ray = sceneEditorWindow.Cam.ScreenPointToRay(gui.PointerPos, Screen.Size);
            Plane groundPlane = new(new Vector3(0, 1, 0), 0f);

            double? t = ray.Intersects(groundPlane);
            if (t >= 0)
            {
                Vector3 intersection = ray.origin + ray.direction * (float)t;
                lassoPoints.Add(intersection);
                Console.WriteLine($"Added lasso point: {intersection}");
            }
        }

        if (Tool == GeometryTool.Block && gui.IsKeyPressed(Key.Return))
            FinalizeLassoMesh();
    }

    private void FinalizeLassoMesh()
    {
        if (lassoPoints.Count < 3)
        {
            Console.WriteLine("Lasso must have at least 3 points.");
            return;
        }

        if (currentGeometry == null)
        {
            currentGeometry = new GameObject("LassoGeometry");
            currentGeometry.AddComponent<MeshRenderer>().Mesh = new Mesh { Name = "Lasso" };
            UndoRedoManager.RecordAction(new AddGameObjectToSceneAction(currentGeometry, null));
        }

        Mesh mesh = currentGeometry.GetComponent<MeshRenderer>().Mesh.Res;

        // Close the loop by repeating the first point at the end
        if (lassoPoints[0] != lassoPoints[lassoPoints.Count - 1])
            lassoPoints.Add(lassoPoints[0]);


        // Assign new vertex array
        mesh.Vertices = lassoPoints.ToArray();

        // Build triangle indices using triangle fan (0, i, i+1)
        List<uint> indices = new();
        for (uint i = 1; i < lassoPoints.Count - 1; i++)
        {
            indices.Add(0);
            indices.Add(i);
            indices.Add(i + 1);
        }

        mesh.Indices32 = indices.ToArray();

        if (mesh.RecalculateNormals != null)
            mesh.RecalculateNormals();

        lassoPoints.Clear();
    }
}
