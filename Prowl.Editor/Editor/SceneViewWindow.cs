﻿// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using Prowl.Editor.Preferences;
using Prowl.Editor.Utilities;
using Prowl.Icons;
using Prowl.Runtime;
using Prowl.Runtime.Cloning;
using Prowl.Runtime.GUI;
using Prowl.Runtime.GUI.Widgets.Gizmo;
using Prowl.Runtime.Rendering;
using Prowl.Runtime.Rendering.Pipelines;
using Prowl.Runtime.SceneManagement;

namespace Prowl.Editor;

public class SceneViewWindow : EditorWindow
{
    public static Camera LastFocusedCamera;
    private static bool LastFocusedCameraChanged;

    readonly Camera Cam;
    RenderTexture RenderTarget;
    Vector2 WindowCenter;
    Vector2 mouseUV;
    int frames;
    double fpsTimer;
    double fps;
    double moveSpeed = 1;
    double camX, camY;

    readonly TransformGizmo gizmo;
    readonly ViewManipulatorGizmo viewManipulator;

    public enum GridType { None, XZ, XY, YZ }

    public SceneViewWindow() : base()
    {
        Title = FontAwesome6.Camera + " Viewport";

        GameObject CamObject = new("Editor-Camera");
        CamObject.hideFlags = HideFlags.HideAndDontSave | HideFlags.NoGizmos;
        CamObject.Transform.position = new Vector3(0, 5, -10);
        Cam = CamObject.AddComponent<Camera>();
        //SceneManager.Scene.Add(CamObject);
        LastFocusedCamera = Cam;

        TransformGizmoMode mode = TransformGizmoMode.TranslateX | TransformGizmoMode.TranslateY | TransformGizmoMode.TranslateZ | TransformGizmoMode.TranslateXY | TransformGizmoMode.TranslateXZ | TransformGizmoMode.TranslateYZ | TransformGizmoMode.TranslateView;
        mode |= TransformGizmoMode.RotateX | TransformGizmoMode.RotateY | TransformGizmoMode.RotateZ | TransformGizmoMode.RotateView;
        mode |= TransformGizmoMode.ScaleX | TransformGizmoMode.ScaleY | TransformGizmoMode.ScaleZ | TransformGizmoMode.ScaleUniform;

        gizmo = new TransformGizmo(EditorGuiManager.Gui, mode);
        viewManipulator = new ViewManipulatorGizmo(EditorGuiManager.Gui);
    }

    public void RefreshRenderTexture(int width, int height)
    {
        RenderTarget?.DestroyImmediate();

        RenderTarget = new RenderTexture(
            (uint)width, (uint)height,
            true);
    }

    protected override void Draw()
    {
        frames++;
        fpsTimer += Time.deltaTime;
        if (fpsTimer >= 1.0)
        {
            fps = frames / fpsTimer;
            frames = 0;
            fpsTimer = 0;
        }

        if (!Project.HasProject)
            return;

        gui.CurrentNode.Padding(5);

        Vector2 renderSize = gui.CurrentNode.LayoutData.Rect.Size;

        if (renderSize.x == 0 || renderSize.y == 0)
            return;

        if (RenderTarget == null || (int)renderSize.x != RenderTarget.Width || (int)renderSize.y != RenderTarget.Height)
            RefreshRenderTexture((int)renderSize.x, (int)renderSize.y);

        WindowCenter = gui.CurrentNode.LayoutData.Rect.Center;

        // Manually Render to the RenderTexture
        Cam.NearClipPlane = SceneViewPreferences.Instance.NearClip;
        Cam.FarClipPlane = SceneViewPreferences.Instance.FarClip;

        SceneViewPreferences.Instance.RenderResolution = Math.Clamp(SceneViewPreferences.Instance.RenderResolution, 0.1f, 8.0f);

        RenderingData data = new RenderingData
        {
            IsSceneViewCamera = true,
            DisplayGizmo = true,
        };

        if (SceneViewPreferences.Instance.GridType != GridType.None)
        {
            data.DisplayGrid = true;
            data.IsSceneViewCamera = true;

            data.GridColor = SceneViewPreferences.Instance.GridColor;
            data.GridSizes.z = SceneViewPreferences.Instance.LineWidth;
            data.GridSizes.x = SceneViewPreferences.Instance.PrimaryGridSize;
            data.GridSizes.y = SceneViewPreferences.Instance.SecondaryGridSize;

            double gX = Math.Round(Cam.Transform.position.x / data.GridSizes.y) * data.GridSizes.y;
            double gY = Math.Round(Cam.Transform.position.y / data.GridSizes.y) * data.GridSizes.y;
            double gZ = Math.Round(Cam.Transform.position.z / data.GridSizes.y) * data.GridSizes.y;

            data.GridMatrix = SceneViewPreferences.Instance.GridType switch
            {
                GridType.XZ => Matrix4x4.CreateLookTo(Vector3.zero, Vector3.right, Vector3.forward) *
                    Matrix4x4.CreateTranslation(new Vector3(gX, 0, gZ)),
                GridType.XY => Matrix4x4.CreateLookTo(Vector3.zero, Vector3.forward, Vector3.up) *
                    Matrix4x4.CreateTranslation(new Vector3(gX, gY, 0)),
                _ => Matrix4x4.CreateLookTo(Vector3.zero, Vector3.up, Vector3.right) *
                    Matrix4x4.CreateTranslation(new Vector3(0, gY, gZ)),
            };
        }

        Debug.ClearGizmos();
        List<GameObject> activeGOs = SceneManager.Scene.ActiveObjects.ToList();
        foreach (GameObject activeGO in activeGOs)
        {
            if (activeGO.hideFlags.HasFlag(HideFlags.NoGizmos)) continue;

            foreach (MonoBehaviour component in activeGO.GetComponents())
            {
                component.DrawGizmos();
                if (HierarchyWindow.SelectHandler.IsSelected(new WeakReference(activeGO)))
                    component.DrawGizmosSelected();
            }
        }

        List<WeakReference> selectedWeaks = HierarchyWindow.SelectHandler.Selected;
        var selectedGOs = new List<GameObject>();
        foreach (WeakReference weak in selectedWeaks)
            if (weak.Target is GameObject go)
            {
                selectedGOs.Add(go);

                // Get all MeshRenderers and draw their bounds
                foreach (MeshRenderer renderer in go.GetComponentsInChildren<MeshRenderer>())
                {
                    Bounds bounds = renderer.Mesh.Res?.bounds ?? new();
                    bounds = bounds.Transform(renderer.Transform.localToWorldMatrix);
                    Debug.DrawWireCube(bounds.center, bounds.extents, Color.yellow);
                }
            }

        RenderPipeline pipeline = Cam.Pipeline.Res ?? DefaultRenderPipeline.Default;

        Cam.Target = RenderTarget;
        pipeline.Render(Cam, data);

        Vector2 imagePos = gui.CurrentNode.LayoutData.Rect.Position;
        Vector2 imageSize = gui.CurrentNode.LayoutData.Rect.Size;
        gui.Draw2D.DrawImage(RenderTarget!.ColorBuffers[0], imagePos, imageSize, Color.white);

        Ray mouseRay = Cam.ScreenPointToRay(gui.PointerPos - imagePos, new Vector2(RenderTarget.Width, RenderTarget.Height));

        bool blockPicking = gui.IsBlockedByInteractable(gui.PointerPos);
        HandleGizmos(selectedGOs, mouseRay, Cam.ViewMatrix, Cam.ProjectionMatrix, blockPicking);

        Rect rect = gui.CurrentNode.LayoutData.Rect;
        rect.width = 100;
        rect.height = 100;
        rect.x = gui.CurrentNode.LayoutData.Rect.x + gui.CurrentNode.LayoutData.Rect.width - rect.width - 10;
        rect.y = gui.CurrentNode.LayoutData.Rect.y + 10;
        viewManipulator.SetRect(rect);
        viewManipulator.SetCamera(Cam.Transform.Forward, Cam.Transform.Up, Cam.projectionType == Camera.ProjectionType.Orthographic);

        if (viewManipulator.Update(blockPicking, out Vector3 newForward, out bool isOrtho))
        {
            //Cam.Transform.forward = newForward;
            if (newForward != Vector3.zero)
            {
                if (newForward == Vector3.up)
                    Cam.Transform.LookAt(Cam.Transform.position + newForward, Vector3.forward);
                else if (newForward == Vector3.down)
                    Cam.Transform.LookAt(Cam.Transform.position + newForward, -Vector3.forward);
                else
                    Cam.Transform.LookAt(Cam.Transform.position + newForward, Vector3.up);

                camX = Cam.Transform.eulerAngles.x;
                camY = Cam.Transform.eulerAngles.y;
            }
            Cam.projectionType = isOrtho ? Camera.ProjectionType.Orthographic : Camera.ProjectionType.Perspective;
        }

        mouseUV = gui.PointerPos - imagePos;

        Interactable viewportInteractable = gui.GetInteractable();

        HandleDragnDrop();
        gui.SetCursorVisibility(true);
        if (IsFocused && viewportInteractable.IsHovered())
        {
            if (gui.IsPointerClick(MouseButton.Left) && !gizmo.IsOver && !viewManipulator.IsOver)
            {
                GameObject? hit = SceneRaycaster.GetObject(Cam, mouseUV, new Vector2(RenderTarget.Width, RenderTarget.Height));

                // If the Scene Camera has no Render Graph, the gBuffer may not be initialized
                if (hit != null)
                {
                    if (hit.AffectedByPrefabLink == null || gui.IsPointerDoubleClick(MouseButton.Left))
                    {
                        HierarchyWindow.SelectHandler.Select(new WeakReference(hit));
                        HierarchyWindow.Ping(hit);
                    }
                    else
                    {
                        // Find Prefab go.IsPrefab
                        Transform prefab = hit.Transform;
                        while (prefab.Parent != null)
                        {
                            prefab = prefab.Parent;
                            if (prefab.gameObject.PrefabLink != null)
                                break;
                        }

                        HierarchyWindow.SelectHandler.Select(new WeakReference(prefab.gameObject));
                        HierarchyWindow.Ping(prefab.gameObject);
                    }
                }
                else
                {
                    HierarchyWindow.SelectHandler.Clear();
                }
            }
            else if (gui.IsPointerDown(MouseButton.Right))
            {
                gui.SetCursorVisibility(false);
                Vector3 moveDir = Vector3.zero;
                if (gui.IsKeyDown(Key.W)) moveDir += Cam.Transform.Forward;
                if (gui.IsKeyDown(Key.S)) moveDir -= Cam.Transform.Forward;
                if (gui.IsKeyDown(Key.A)) moveDir -= Cam.Transform.Right;
                if (gui.IsKeyDown(Key.D)) moveDir += Cam.Transform.Right;
                if (gui.IsKeyDown(Key.E)) moveDir += Cam.Transform.Up;
                if (gui.IsKeyDown(Key.Q)) moveDir -= Cam.Transform.Up;

                if (moveDir != Vector3.zero)
                {
                    moveDir = Vector3.Normalize(moveDir);
                    if (gui.IsKeyDown(Key.LeftShift))
                        moveDir *= 2.0f;
                    Cam.Transform.position += moveDir * (Time.deltaTimeF * 2.5f) * moveSpeed;

                    // Get Exponentially faster
                    moveSpeed += Time.deltaTimeF * 0.0001;
                    moveSpeed *= 1.0025;
                    moveSpeed = Math.Clamp(moveSpeed, 1, 1000);
                }
                else
                {
                    moveSpeed = 1;
                }

                if (gui.IsPointerMoving)
                {
                    if (LastFocusedCameraChanged)
                    {
                        camX = Cam.Transform.eulerAngles.x;
                        camY = Cam.Transform.eulerAngles.y;
                        LastFocusedCameraChanged = false;
                    }

                    Vector2 mouseDelta = gui.PointerDelta;
                    if (SceneViewPreferences.Instance.InvertLook)
                        mouseDelta.y = -mouseDelta.y;
                    camY += mouseDelta.x * 0.05f * SceneViewPreferences.Instance.LookSensitivity;
                    camX += mouseDelta.y * 0.05f * SceneViewPreferences.Instance.LookSensitivity;
                    camX = MathD.Clamp(camX, -89.9f, 89.9f);
                    Cam.Transform.eulerAngles = new Vector3(camX, camY, 0);

                    gui.PointerPos = WindowCenter;
                    // Input.MousePosition = WindowCenter;
                }
            }
            else
            {
                moveSpeed = 1;
                if (gui.IsPointerDown(MouseButton.Middle))
                {
                    gui.SetCursorVisibility(false);
                    Vector2 mouseDelta = gui.PointerDelta;
                    Vector3 pos = Cam.Transform.position;
                    pos -= Cam.Transform.Right * mouseDelta.x * (Time.deltaTimeF * 1f * SceneViewPreferences.Instance.PanSensitivity);
                    pos += Cam.Transform.Up * mouseDelta.y * (Time.deltaTimeF * 1f * SceneViewPreferences.Instance.PanSensitivity);
                    Cam.Transform.position = pos;
                    gui.PointerPos = WindowCenter;

                }

                if (Hotkeys.IsHotkeyDown("Duplicate", new() { Key = Key.D, Ctrl = true }))
                    HierarchyWindow.DuplicateSelected();

                if (gui.IsKeyDown(Key.F) && HierarchyWindow.SelectHandler.Selected.Any())
                {
                    float defaultZoomFactor = 2f;
                    if (HierarchyWindow.SelectHandler.Selected.Count == 1)
                    {
                        // If only one object is selected, set the camera position to the center of that object
                        if (HierarchyWindow.SelectHandler.Selected.First().Target is GameObject singleObject)
                        {
                            Cam.Transform.position = singleObject.Transform.position -
                                                                (Cam.Transform.Forward * defaultZoomFactor);
                            return;
                        }
                    }

                    // Calculate the bounding box based on the positions of selected objects
                    Bounds combinedBounds = new Bounds();
                    foreach (WeakReference obj in HierarchyWindow.SelectHandler.Selected)
                    {
                        if (obj.Target is GameObject go)
                        {
                            combinedBounds.Encapsulate(go.Transform.position);
                        }
                    }

                    // Calculate the zoom factor based on the size of the bounding box
                    float boundingBoxSize = (float)MathD.Max(combinedBounds.size.x, combinedBounds.size.y,
                        combinedBounds.size.z);
                    float zoomFactor = boundingBoxSize * defaultZoomFactor;

                    Vector3 averagePosition = combinedBounds.center;
                    Cam.Transform.position =
                        averagePosition - (Cam.Transform.Forward * zoomFactor);
                }
            }

            if (gui.PointerWheel != 0)
            {
                // Larger distance more zoom, but clamped
                double amount = 1f * SceneViewPreferences.Instance.ZoomSensitivity;
                Cam.Transform.position += mouseRay.direction * amount * gui.PointerWheel;

            }
        }

        DrawViewportSettings();

        if (SceneViewPreferences.Instance.ShowFPS)
        {
            //g.DrawRectFilled(imagePos + new Vector2(5, imageSize.y - 25), new Vector2(75, 20), new Color(0, 0, 0, 0.25f));
            gui.Draw2D.DrawText($"FPS: {fps:0.0}", imagePos + new Vector2(10, imageSize.y - 22));
        }
    }

    private void HandleGizmos(List<GameObject> selectedGOs, Ray mouseRay, Matrix4x4 view, Matrix4x4 projection, bool blockPicking)
    {
        gizmo.UpdateCamera(gui.CurrentNode.LayoutData.Rect, view, projection, Cam.Transform.Up, Cam.Transform.Forward, Cam.Transform.Right);

        gizmo.Snapping = Input.GetKey(Key.LeftControl);
        gizmo.SnapDistance = SceneViewPreferences.Instance.SnapDistance;
        gizmo.SnapAngle = SceneViewPreferences.Instance.SnapAngle;

        Vector3 centerOfAll = Vector3.zero;

        for (int i = 0; i < selectedGOs.Count; i++)
        {
            GameObject selectedGo = selectedGOs[i];
            centerOfAll += selectedGo.Transform.position;
        }

        centerOfAll /= selectedGOs.Count;

        Quaternion rotation = Quaternion.identity;
        Vector3 scale = Vector3.one;
        if (selectedGOs.Count == 1)
        {
            rotation = selectedGOs[0].Transform.rotation;
            scale = selectedGOs[0].Transform.localScale;
        }

        gizmo.SetTransform(centerOfAll, rotation, scale);
        GizmoResult? result = gizmo.Update(mouseRay, gui.PointerPos, blockPicking);
        if (result.HasValue)
        {
            foreach (GameObject selectedGo in selectedGOs)
            {
                if (result.Value.TranslationDelta.HasValue || result.Value.RotationDelta.HasValue ||
                    result.Value.ScaleDelta.HasValue)
                {
                    var newPos = selectedGo.Transform.position + (result.Value.TranslationDelta ?? Vector3.zero);
                    newPos = selectedGo.Transform.Parent?.InverseTransformPoint(newPos) ?? newPos;

                    Quaternion newRot = selectedGo.Transform.rotation;
                    if (result.Value.RotationDelta.HasValue && result.Value.RotationAxis.HasValue)
                    {
                        Vector3 centerToSelected = selectedGo.Transform.position - centerOfAll;
                        Vector3 rotated =
                            Quaternion.AngleAxis(result.Value.RotationDelta.Value, result.Value.RotationAxis.Value) *
                            centerToSelected;
                        selectedGo.Transform.position = centerOfAll + rotated;
                        Quaternion rotationDelta = Quaternion.AngleAxis(result.Value.RotationDelta.Value,
                            result.Value.RotationAxis.Value);

                        newRot = rotationDelta * selectedGo.Transform.rotation;
                    }

                    Vector3 newScale = selectedGo.Transform.localScale * (result.Value.ScaleDelta ?? Vector3.one);

                    UndoRedoManager.RecordAction(new ChangeTransformAction(selectedGo, newPos, newRot, newScale));

                    Prefab.OnFieldChange(selectedGo, "_transform");
                }
            }
        }
        gizmo.Draw();
    }

    private void HandleDragnDrop()
    {
        if (DragnDrop.Drop(out GameObject? original))
        {
            if (original.AssetID == Guid.Empty) return;

            // Dropping a prefab into the scene
            GameObject go = original.DeepClone();
            go.AssetID = original.AssetID;
            SceneManager.Scene.Add(go);
            if (go != null)
            {
                Vector3? hit = SceneRaycaster.GetPosition(Cam, mouseUV, new Vector2(RenderTarget.Width, RenderTarget.Height));

                if (hit == null)
                    go.Transform.position = Cam.Transform.position + Cam.Transform.Forward * 10;
                else
                    go.Transform.position = hit.Value;
            }
            HierarchyWindow.SelectHandler.SetSelection(new WeakReference(go));
        }
        else if (DragnDrop.Drop(out Prefab? prefab))
        {
            GameObject go = prefab.Instantiate();
            SceneManager.Scene.Add(go);
            GameObject t = go;

            if (t != null)
            {
                Vector3? hit = SceneRaycaster.GetPosition(Cam, mouseUV, new Vector2(RenderTarget.Width, RenderTarget.Height));

                if (hit == null)
                    t.Transform.position = Cam.Transform.position + Cam.Transform.Forward * 10;
                else
                    go.Transform.position = hit.Value;
            }

            HierarchyWindow.SelectHandler.SetSelection(new WeakReference(go));
        }
        else if (DragnDrop.Drop(out Scene? scene))
        {
            SceneManager.LoadScene(scene);
        }
        else if (DragnDrop.Drop(out Material? material))
        {
            GameObject? hit = SceneRaycaster.GetObject(Cam, mouseUV, new Vector2(RenderTarget.Width, RenderTarget.Height));

            if (hit != null)
            {
                // Look for a MeshRenderer
                MeshRenderer? renderer = hit.GetComponent<MeshRenderer>();
                if (renderer != null)
                    renderer.Material = material;
            }
        }
    }

    private void DrawViewportSettings()
    {
        // TODO: Support custom Viewport Settings for tooling like A Terrain Editor having Brush Size, Strength, etc all in the Viewport

        int buttonCount = 4;
        double buttonSize = EditorStylePrefs.Instance.ItemSize;

        bool vertical = true;

        double width = (vertical ? buttonSize : buttonSize * buttonCount) + 8;
        double height = (vertical ? buttonSize * buttonCount : buttonSize) + 8;

        Color bgColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);

        using (gui.Node("VpSettings").TopLeft(5).Scale(width, height).Padding(4).Layout(vertical ? LayoutType.Column : LayoutType.Row).Enter())
        {
            gui.Draw2D.DrawRectFilled(gui.CurrentNode.LayoutData.Rect, bgColor, (float)EditorStylePrefs.Instance.WindowRoundness);

            using (gui.Node("EditorCam").Scale(buttonSize).Enter())
            {
                if (gui.IsNodePressed())
                    GlobalSelectHandler.Select(new WeakReference(Cam.GameObject));

                gui.TextNode("Label", FontAwesome6.Camera).Expand();
                if (gui.IsNodeHovered())
                    gui.Draw2D.DrawRectFilled(gui.CurrentNode.LayoutData.Rect, EditorStylePrefs.Instance.Hovering, (float)EditorStylePrefs.Instance.ButtonRoundness);
            }
            gui.Tooltip("Select Editor Camera", align: TooltipAlign.Right);

            GridType gridType = SceneViewPreferences.Instance.GridType;
            int gridTypeIndex = (int)gridType;

            Gui.WidgetStyle style = EditorGUI.InputStyle;
            style.BGColor = Color.clear;
            style.BorderColor = Color.clear;

            Gui.WidgetStyle popupStyle = style;
            popupStyle.BGColor = bgColor;

            if (gui.Combo("GridType", "_GridTypePopup", ref gridTypeIndex, Enum.GetNames(typeof(GridType)), 0, 0, buttonSize, buttonSize, style, popupStyle, FontAwesome6.TableCells))
                SceneViewPreferences.Instance.GridType = (GridType)gridTypeIndex;

            using (gui.Node("GizmoMode").Scale(buttonSize).Enter())
            {
                if (gui.IsNodePressed())
                    gizmo.Orientation = (TransformGizmo.GizmoOrientation)((int)gizmo.Orientation == 1 ? 0 : 1);

                gui.TextNode("Label", gizmo.Orientation == 0 ? FontAwesome6.Globe : FontAwesome6.Cube).Expand();
                if (gui.IsNodeHovered())
                    gui.Draw2D.DrawRectFilled(gui.CurrentNode.LayoutData.Rect, EditorStylePrefs.Instance.Hovering, (float)EditorStylePrefs.Instance.ButtonRoundness);
            }
            gui.Tooltip("Gizmo Mode: " + (gizmo.Orientation == 0 ? "World" : "Local"), align: TooltipAlign.Right);

            using (gui.Node("OpenPreferences").Scale(buttonSize).Enter())
            {
                if (gui.IsNodePressed())
                    new PreferencesWindow(typeof(SceneViewPreferences));

                gui.TextNode("Label", FontAwesome6.Gear).Expand();
                if (gui.IsNodeHovered())
                    gui.Draw2D.DrawRectFilled(gui.CurrentNode.LayoutData.Rect, EditorStylePrefs.Instance.Hovering, (float)EditorStylePrefs.Instance.ButtonRoundness);
            }
            gui.Tooltip("Open Editor Preferences", align: TooltipAlign.Right);
        }
    }

    internal static void SetCamera(Vector3 position, Quaternion rotation)
    {
        LastFocusedCamera.GameObject.Transform.position = position;
        LastFocusedCamera.GameObject.Transform.rotation = rotation;
        LastFocusedCameraChanged = true;
    }
}
