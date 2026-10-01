using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// One-off: replaces the old controls hint (scaled arrow images) and the
// menu toggles with a key-cap legend and switch-style buttons that match
// the black Next/Hold/Stats boxes.
//   Unity.exe -batchmode -quit -projectPath . -executeMethod MenuRestyle.Run
public static class MenuRestyle
{
    const string ScenePath = "Assets/Scenes/Level0.unity";

    static readonly Color Panel = new Color(0.047f, 0.047f, 0.047f, 1f);
    static readonly Color Row = new Color(0.13f, 0.13f, 0.13f, 1f);
    static readonly Color RowHover = new Color(0.19f, 0.19f, 0.19f, 1f);
    static readonly Color RowPressed = new Color(0.09f, 0.09f, 0.09f, 1f);
    // Light caps like real keys; also keeps the black arrow sprite visible.
    static readonly Color KeyCap = new Color(0.86f, 0.86f, 0.86f, 1f);
    static readonly Color Dim = new Color(1f, 1f, 1f, 0.55f);
    static readonly Color Text = new Color(1f, 1f, 1f, 0.92f);

    static readonly Vector2 PanelSize = new Vector2(580f, 620f);
    const float RowWidth = 500f;

    static Font font;
    static Sprite rounded;
    static Sprite knob;
    static Sprite arrow;

    [MenuItem("ZKTris/Restyle Menu Panels")]
    public static void Run()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath);
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        arrow = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Imports/Arrow.png");

        var all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).Select(t => t.gameObject).ToList();
        var canvas = (RectTransform)all.First(g => g.name == "Canvas").transform;
        foreach (var name in new[] { "Instructions", "MoreControls", "ControlsLegend" })
        {
            var old = all.FirstOrDefault(g => g != null && g.name == name);
            if (old != null) Object.DestroyImmediate(old);
        }

        BuildLegend(canvas);
        BuildMenu(Object.FindAnyObjectByType<Controls>(FindObjectsInactive.Include));

        // Match the new labels: uppercase and dimmed, like the stats panel.
        foreach (var box in new[] { "PreviewBackground", "HoldBackground" })
        {
            var label = all.First(g => g != null && g.name == box).GetComponentsInChildren<UnityEngine.UI.Text>(true).First();
            label.text = box == "HoldBackground" ? "HOLD" : "NEXT";
            label.color = Dim;
            EditorUtility.SetDirty(label);
        }

        // The board is narrower on 16:9 than on 16:10; keep overlays inside it.
        foreach (var overlay in new[] { "PausedPanel", "GameOverPanel" })
        {
            var rt = (RectTransform)all.First(g => g != null && g.name == overlay).transform;
            rt.sizeDelta = new Vector2(530f, rt.sizeDelta.y);
            EditorUtility.SetDirty(rt);
        }

        foreach (var selectable in Object.FindObjectsByType<Selectable>(FindObjectsInactive.Include))
        {
            selectable.navigation = new Navigation { mode = Navigation.Mode.None };
            EditorUtility.SetDirty(selectable);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[MenuRestyle] done");
    }

    // ---------------------------------------------------------------- legend

    static void BuildLegend(RectTransform canvas)
    {
        var panel = MakePanel(canvas, "ControlsLegend", new Vector2(0f, 0f), new Vector2(22f, 20f));
        Label(panel, "Title", "CONTROLS", 30, FontStyle.Bold, Dim, TextAnchor.MiddleCenter, new Vector2(0f, -50f), new Vector2(PanelSize.x, 40f));

        // Keys are right-aligned against this x, actions start after it.
        const float keyEdge = 250f;
        float y = -118f;
        void Line(string action, params string[] keys)
        {
            float x = keyEdge;
            foreach (var key in keys.Reverse())
            {
                float width = key.Length > 2 ? 130f : 56f;
                x -= width;
                KeyCapAt(panel, key, new Vector2(x + width / 2f, y), width);
                x -= 10f;
            }
            Label(panel, action, action, 28, FontStyle.Normal, Text, TextAnchor.MiddleLeft, new Vector2(keyEdge + 30f + 140f, y), new Vector2(280f, 56f), topLeft: true);
            y -= 68f;
        }

        Line("MOVE", "←", "→");
        Line("ROTATE", "↑", "X");
        Line("ROTATE BACK", "Z");
        Line("SOFT DROP", "↓");
        Line("HARD DROP", "SPACE");
        Line("HOLD", "C");
        Line("PAUSE", "P");
    }

    static void KeyCapAt(RectTransform panel, string key, Vector2 center, float width)
    {
        var cap = Box(panel, "Key " + key, KeyCap, rounded, new Vector2(0f, 1f), center, new Vector2(width, 56f));
        // Arrows use the game's arrow sprite; the font has no arrow glyphs
        // we can rely on in every build.
        float angle;
        switch (key)
        {
            case "←": angle = 180f; break;
            case "→": angle = 0f; break;
            case "↑": angle = 90f; break;
            case "↓": angle = -90f; break;
            default:
                Label(cap, "Label", key, key.Length > 2 ? 22 : 26, FontStyle.Bold, Panel, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(width, 56f), centered: true);
                return;
        }
        var icon = Box(cap, "Arrow", Panel, arrow, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(30f, 20f));
        icon.localEulerAngles = new Vector3(0f, 0f, angle);
        icon.GetComponent<Image>().type = Image.Type.Simple;
    }

    // ------------------------------------------------------------------ menu

    static void BuildMenu(Controls controls)
    {
        var panel = (RectTransform)controls.transform;
        // Old toggles were built with wild non-uniform scales; start clean.
        for (int i = panel.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(panel.GetChild(i).gameObject);
        panel.localScale = Vector3.one;
        panel.localRotation = Quaternion.identity;
        panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(1f, 0f);
        panel.anchoredPosition = new Vector2(-22f, 20f);
        panel.sizeDelta = PanelSize;
        var bg = panel.GetComponent<Image>();
        if (bg == null) bg = panel.gameObject.AddComponent<Image>();
        bg.sprite = null;
        bg.color = Panel;
        bg.raycastTarget = false;
        EditorUtility.SetDirty(bg);

        Label(panel, "Title", "MENU", 30, FontStyle.Bold, Dim, TextAnchor.MiddleCenter, new Vector2(0f, -50f), new Vector2(PanelSize.x, 40f));

        // Pause / play: the main call to action, in the accent color.
        var pause = RowRect(panel, "PausePlay", -128f, 84f);
        var pauseImage = pause.GetComponent<Image>();
        var pauseToggle = pause.gameObject.AddComponent<Toggle>();
        pauseToggle.isOn = false;
        pauseToggle.targetGraphic = pauseImage;
        pauseToggle.colors = Tint(Hud.AccentColor, new Color(1f, 0.86f, 0.45f), new Color(0.85f, 0.62f, 0.15f));
        var pauseLabel = Label(pause, "Label", "PLAY", 34, FontStyle.Bold, Panel, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(RowWidth, 84f), centered: true);
        UnityEventTools.AddVoidPersistentListener(pauseToggle.onValueChanged, controls.TogglePause);

        var fullScreen = SwitchRow(panel, "FullScreen", "FULL SCREEN", -234f, false);
        UnityEventTools.AddVoidPersistentListener(fullScreen.onValueChanged, controls.ToggleFullScreen);
        var music = SwitchRow(panel, "Music", "MUSIC", -322f, true);
        UnityEventTools.AddVoidPersistentListener(music.onValueChanged, controls.ToggleMusic);
        var sounds = SwitchRow(panel, "Sounds", "SOUNDS", -410f, true);
        UnityEventTools.AddVoidPersistentListener(sounds.onValueChanged, controls.ToggleSounds);

        var quit = RowRect(panel, "Quit", -530f, 72f);
        var quitButton = quit.gameObject.AddComponent<Button>();
        quitButton.targetGraphic = quit.GetComponent<Image>();
        quitButton.colors = Tint(Row, RowHover, RowPressed);
        Label(quit, "Label", "QUIT", 28, FontStyle.Bold, Hud.HoleColor, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(RowWidth, 72f), centered: true);
        UnityEventTools.AddPersistentListener(quitButton.onClick, controls.Quit);

        controls.pauseText = pauseLabel;
        controls.quitButton = quit.gameObject;
        controls.fullScreenToggle = fullScreen;
        EditorUtility.SetDirty(controls);
    }

    static Toggle SwitchRow(RectTransform panel, string name, string text, float y, bool on)
    {
        var row = RowRect(panel, name, y, 72f);
        var toggle = row.gameObject.AddComponent<Toggle>();
        toggle.isOn = on;
        toggle.targetGraphic = row.GetComponent<Image>();
        toggle.colors = Tint(Row, RowHover, RowPressed);
        Label(row, "Label", text, 28, FontStyle.Bold, Text, TextAnchor.MiddleLeft, new Vector2(28f + 150f, 0f), new Vector2(300f, 72f), leftMiddle: true);

        var track = Box(row, "Track", on ? Hud.AccentColor : new Color(0.3f, 0.3f, 0.3f), rounded, new Vector2(1f, 0.5f), new Vector2(-28f - 42f, 0f), new Vector2(84f, 40f));
        track.GetComponent<Image>().pixelsPerUnitMultiplier = 0.25f; // pill ends
        track.GetComponent<Image>().raycastTarget = false;
        // Painted in its starting state so the scene view matches the game.
        var dot = Box(track, "Knob", Color.white, knob, new Vector2(0.5f, 0.5f), new Vector2(on ? 20f : -20f, 0f), new Vector2(32f, 32f));
        dot.GetComponent<Image>().type = Image.Type.Simple;
        dot.GetComponent<Image>().raycastTarget = false;

        var view = row.gameObject.AddComponent<ToggleSwitch>();
        view.track = track.GetComponent<Image>();
        view.knob = dot;
        view.onColor = Hud.AccentColor;
        return toggle;
    }

    // -------------------------------------------------------------- helpers

    static RectTransform MakePanel(RectTransform canvas, string name, Vector2 corner, Vector2 offset)
    {
        var rt = Box(canvas, name, Panel, null, corner, offset, PanelSize);
        rt.pivot = corner;
        rt.anchoredPosition = offset;
        rt.GetComponent<Image>().raycastTarget = false;
        return rt;
    }

    static RectTransform RowRect(RectTransform panel, string name, float y, float height)
    {
        // Row images are white; the Selectable color block supplies the
        // actual normal / hover / pressed colors.
        var row = Box(panel, name, Color.white, rounded, new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(RowWidth, height));
        row.GetComponent<Image>().raycastTarget = true;
        return row;
    }

    static ColorBlock Tint(Color normal, Color hover, Color pressed)
    {
        var block = ColorBlock.defaultColorBlock;
        block.normalColor = normal;
        block.highlightedColor = hover;
        block.selectedColor = normal;
        block.pressedColor = pressed;
        block.disabledColor = normal * 0.5f;
        block.fadeDuration = 0.08f;
        return block;
    }

    static RectTransform Box(RectTransform parent, string name, Color color, Sprite sprite, Vector2 anchor, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        if (sprite != null)
        {
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
        }
        return rt;
    }

    // Default placement is centered under the top edge of the parent;
    // topLeft / leftMiddle / centered pick other anchors.
    static UnityEngine.UI.Text Label(RectTransform parent, string name, string text, int size, FontStyle style, Color color,
        TextAnchor align, Vector2 position, Vector2 box, bool topLeft = false, bool leftMiddle = false, bool centered = false)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Text));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = topLeft ? new Vector2(0f, 1f)
            : leftMiddle ? new Vector2(0f, 0.5f)
            : centered ? new Vector2(0.5f, 0.5f)
            : new Vector2(0.5f, 1f);
        rt.anchoredPosition = position;
        rt.sizeDelta = box;
        var t = go.GetComponent<UnityEngine.UI.Text>();
        t.font = font;
        t.text = text;
        t.fontSize = size;
        t.fontStyle = style;
        t.color = color;
        t.alignment = align;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }
}
