﻿// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using Prowl.Runtime;
using Prowl.Runtime.Utils;

namespace Prowl.Editor.Preferences;

[FilePath("General.pref", FilePathAttribute.Location.EditorPreference)]
public class GeneralPreferences : ScriptableSingleton<GeneralPreferences>
{
    [Header("General")]
    public bool LockFPS = false;
    [ShowIf("LockFPS")]
    public int TargetFPS = 0;
    [ShowIf("LockFPS", true)]
    public bool VSync = true;

    [Header("Debugging")]
    public bool ShowDebugLogs = true;
    public bool ShowDebugWarnings = true;
    public bool ShowDebugErrors = true;
    public bool ShowDebugSuccess = true;

    [Header("Game View")]
    public bool AutoFocusGameView = true;
    public bool AutoRefreshGameView = true;
    public GameWindow.Resolutions Resolution = GameWindow.Resolutions.fit;
    [HideInInspector]
    public int CurrentWidth = 1280;
    [HideInInspector]
    public int CurrentHeight = 720;

}

[FilePath("Editor.pref", FilePathAttribute.Location.EditorPreference)]
public class EditorPreferences : ScriptableSingleton<EditorPreferences>
{
    [Header("UI")]
    public bool AntiAliasing = true;

    [Header("File Editor")]
    public string fileEditor = "";     // code
    public string fileEditorArgs = ""; // "${ProjectDirectory}" -g "${File}":${Line}:${Character}
}

[FilePath("EditorStyle.pref", FilePathAttribute.Location.EditorPreference)]
public class EditorStylePrefs : ScriptableSingleton<EditorStylePrefs>
{
    [Header("Colors")]
    public double Disabled = 0.7;
    public Color LesserText = new(110, 110, 120);
    public Color Background = new(15, 15, 18);
    public Color WindowBGOne = new(0.14814814925193787f, 0.15755437314510345f, 0.190476194024086f, 1.0f);
    public Color WindowBGTwo = new Color(25, 27, 32);
    public Color Borders = new(49, 52, 66);
    public Color Hovering = new (0.27f, 0.29f, 0.33f, 0.80f);
    public Color Highlighted = new(25, 72, 133);
    public Color Ping = Yellow;
    public Color DropHighlight = Orange;
    public Color Warning = Red;

    [Header("Sizing")]
    public double Scale = 1;
    public double ItemSize = 25;

    [Header("Spacing")]
    public double DockSpacing = 3;

    [Header("Rounding")]
    public double WindowRoundness = 2;
    public double TabRoundness = 4;
    public double AssetRoundness = 4;
    public double ButtonRoundness = 2;

    public enum NoodlePath { Straight, Curvy, Angled, ShaderLab }
    public enum NoodleStroke { Basic, Dashed }

    public enum ComponentHeaderColor
    {
        Flat,
        Pastel
    }

    [Header("Component Headers")]
    public ComponentHeaderColor HeaderColor = ComponentHeaderColor.Flat;

    [Header("Node Editor")]
    public NoodlePath NoodlePathType = NoodlePath.Curvy;
    public NoodleStroke NoodleStrokeType = NoodleStroke.Basic;
    public double NoodleStrokeWidth = 4;

    // Base Colors
    public static Color Black => new(0, 0, 0, 255);
    public static Color Base4 => new(100, 100, 110);
    public static Color Base5 => new(139, 139, 147);
    public static Color Base6 => new(112, 112, 124);
    public static Color Base7 => new(138, 138, 152);
    public static Color Base8 => new(169, 169, 183);
    public static Color Base9 => new(208, 208, 218);
    public static Color Base10 => new(234, 234, 244);
    public static Color Base11 => new(255, 255, 255);

    // Accents
    public static Color Blue => new(107, 144, 212);
    public static Color Green => new(168, 204, 96);
    public static Color Violet => new(119, 71, 202);
    public static Color Orange => new(236, 140, 85);
    public static Color Yellow => new(236, 230, 99);
    public static Color Indigo => new(84, 21, 241);
    public static Color Emerald => new(94, 234, 141);
    public static Color Fuchsia => new(221, 80, 214);
    public static Color Red => new(226, 110, 110);
    public static Color Sky => new(11, 214, 244);
    public static Color Pink => new(251, 123, 184);

    public static Color RandomPastel(Type type, float alpha = 1f, float pastelStrength = 0.5f)
    {
        System.Random random = new System.Random(type.GetHashCode());
        Color pastel = Color.FromHSV(random.Next(0, 360), pastelStrength, 0.75f, alpha);
        return pastel;
        // var inverted = 1.0f - pastelStrength;
        // float r = (float)(random.NextDouble() * inverted + pastelStrength);
        // float g = (float)(random.NextDouble() * inverted + pastelStrength);
        // float b = (float)(random.NextDouble() * inverted + pastelStrength);
        // return new Color(r, g, b, alpha);
    }

    public static Color RandomPastelColor(int seed, float alpha = 1f)
    {
        System.Random random = new System.Random(seed);
        float r = (float)(random.NextDouble() * 0.5 + 0.5) * 0.8f;
        float g = (float)(random.NextDouble() * 0.5 + 0.5) * 0.8f;
        float b = (float)(random.NextDouble() * 0.5 + 0.5) * 0.8f;
        return new Color(r, g, b, alpha);
    }

    public override void OnValidate()
    {
        Scale = MathD.Clamp(Scale, 0.5, 2);
        ItemSize = MathD.Clamp(ItemSize, 20, 40);

        DockSpacing = MathD.Clamp(DockSpacing, 0, 8);
    }

    [GUIButton("Reset to Default")]
    public static void ResetDefault()
    {
        // Shortcut to reset values
        _instance = new EditorStylePrefs();
        _instance.Save();
    }
}
