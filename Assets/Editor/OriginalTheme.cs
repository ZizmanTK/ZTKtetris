using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// One-off: puts the UI back on the original game's visual language:
// translucent white panels, bold dark grey text, near-black preview
// boxes, and the game's own block textures as UI (salmon cube checkbox,
// green cube check, grid-tile key caps, crimson I bar for Quit).
//   Unity.exe -batchmode -quit -projectPath . -executeMethod OriginalTheme.Run
public static class OriginalTheme
{
    const string ScenePath = "Assets/Scenes/Level0.unity";

    // Values from the original scene.
    static readonly Color Panel = new Color(1f, 1f, 1f, 0.392f);
    static readonly Color Ink = new Color(0.196f, 0.196f, 0.196f, 1f);
    static readonly Color InkSoft = new Color(0.196f, 0.196f, 0.196f, 0.75f);
    static readonly Color PreviewBox = new Color(0.047f, 0.047f, 0.047f, 1f);
    static readonly Color BoxTint = new Color(0.739f, 0.739f, 0.739f, 1f);
    static readonly Color Salmon = new Color(0.808f, 0.42f, 0.45f, 1f);

    static readonly Vector2 PanelSize = new Vector2(580f, 620f);

    static Font font;
    static Sprite cube, green, grid, arrow, iPiece;

    [MenuItem("ZKTris/Apply Original Theme")]
    public static void Run()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath);
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        cube = Load("Assets/Imports/BaseTetrisCube.png");
        green = Load("Assets/Imports/Green.png");
        grid = Load("Assets/Imports/TetrisGrid.png");
        arrow = Load("Assets/Imports/Arrow.png");
        iPiece = Load("Assets/Imports/Forms/I.png");

        var all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).Select(t => t.gameObject).ToList();
        foreach (var go in all) GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        GameObject Find(string name) => all.First(g => g != null && g.name == name);

        var canvas = (RectTransform)Find("Canvas").transform;
        var legend = all.FirstOrDefault(g => g != null && g.name == "ControlsLegend");
        if (legend != null) Object.DestroyImmediate(legend);
        all.RemoveAll(g => g == null);

        BuildLegend(canvas);
        BuildMenu(Object.FindAnyObjectByType<Controls>(FindObjectsInactive.Include));
        RethemeStats(Find("StatsPanel"));
        RethemeOverlays(Find);
        RethemeParticles(Find);

        // Preview labels back to the original "Next" (white, title case).
        foreach (var (box, text) in new[] { ("PreviewBackground", "Next"), ("HoldBackground", "Hold") })
        {
            var label = Find(box).GetComponentsInChildren<Text>(true).First();
            label.text = text;
            label.color = Color.white;
            EditorUtility.SetDirty(label);
        }

        foreach (var selectable in Object.FindObjectsByType<Selectable>(FindObjectsInactive.Include))
        {
            selectable.navigation = new Navigation { mode = Navigation.Mode.None };
            EditorUtility.SetDirty(selectable);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[OriginalTheme] done");
    }

    // ---------------------------------------------------------------- legend

    static void BuildLegend(RectTransform canvas)
    {
        var panel = Rect(canvas, "ControlsLegend", Panel, new Vector2(0f, 0f), new Vector2(22f, 20f), PanelSize);
        panel.pivot = Vector2.zero;
        panel.anchoredPosition = new Vector2(22f, 20f);
        Label(panel, "Title", "CONTROLS", 44, Ink, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0f, -55f), new Vector2(PanelSize.x, 60f));

        const float keyEdge = 250f;
        float y = -128f;
        void Line(string action, params string[] keys)
        {
            float x = keyEdge;
            foreach (var key in keys.Reverse())
            {
                float width = key.Length > 2 ? 130f : 54f;
                x -= width;
                KeyCap(panel, key, new Vector2(x + width / 2f, y), width);
                x -= 10f;
            }
            Label(panel, action, action, 28, Ink, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(keyEdge + 24f + 150f, y), new Vector2(300f, 54f));
            y -= 66f;
        }

        Line("MOVE", "←", "→");
        Line("ROTATE", "↑", "X");
        Line("ROTATE BACK", "Z");
        Line("SOFT DROP", "↓");
        Line("HARD DROP", "SPACE");
        Line("HOLD", "C");
        Line("PAUSE", "P");
    }

    // Key caps are grid tiles, the same light block the board is made of.
    static void KeyCap(RectTransform panel, string key, Vector2 center, float width)
    {
        var cap = Block(panel, "Key " + key, grid, new Vector2(0f, 1f), center, new Vector2(width, 54f), Color.white);
        float angle;
        switch (key)
        {
            case "←": angle = 180f; break;
            case "→": angle = 0f; break;
            case "↑": angle = 90f; break;
            case "↓": angle = -90f; break;
            default:
                Label(cap, "Label", key, key.Length > 2 ? 22 : 26, Ink, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(width, 54f));
                return;
        }
        var icon = Image(cap, "Arrow", arrow, Ink, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(30f, 20f));
        icon.localEulerAngles = new Vector3(0f, 0f, angle);
    }

    // ------------------------------------------------------------------ menu

    static void BuildMenu(Controls controls)
    {
        var panel = (RectTransform)controls.transform;
        for (int i = panel.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(panel.GetChild(i).gameObject);
        panel.localScale = Vector3.one;
        panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(1f, 0f);
        panel.anchoredPosition = new Vector2(-22f, 20f);
        panel.sizeDelta = PanelSize;
        var bg = panel.GetComponent<UnityEngine.UI.Image>();
        if (bg == null) bg = panel.gameObject.AddComponent<UnityEngine.UI.Image>();
        bg.sprite = null;
        bg.color = Panel;
        bg.raycastTarget = false;
        EditorUtility.SetDirty(bg);

        // Same layout as the original: a checkbox and a label per line,
        // Quit with the red I bar at the bottom.
        var pause = CheckRow(panel, "PausePlay", "Play", -95f, false);
        UnityEventTools.AddVoidPersistentListener(pause.onValueChanged, controls.TogglePause);
        var fullScreen = CheckRow(panel, "FullScreen", "Full Screen", -205f, false);
        UnityEventTools.AddVoidPersistentListener(fullScreen.onValueChanged, controls.ToggleFullScreen);
        var music = CheckRow(panel, "Music", "Music", -315f, true);
        UnityEventTools.AddVoidPersistentListener(music.onValueChanged, controls.ToggleMusic);
        var sounds = CheckRow(panel, "Sounds", "Sounds", -425f, true);
        UnityEventTools.AddVoidPersistentListener(sounds.onValueChanged, controls.ToggleSounds);

        var quit = Rect(panel, "Quit", Color.clear, new Vector2(0.5f, 1f), new Vector2(0f, -540f), new Vector2(480f, 90f));
        quit.GetComponent<UnityEngine.UI.Image>().raycastTarget = true;
        var quitButton = quit.gameObject.AddComponent<Button>();
        var bar = Image(quit, "Bar", iPiece, Color.white, new Vector2(0.5f, 0.5f), new Vector2(-70f, 0f), new Vector2(150f, 84f));
        bar.GetComponent<UnityEngine.UI.Image>().preserveAspect = true;
        quitButton.targetGraphic = bar.GetComponent<UnityEngine.UI.Image>();
        quitButton.colors = Tint(Color.white, new Color(1f, 0.8f, 0.8f), BoxTint);
        Label(quit, "Label", "QUIT", 48, Ink, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.5f), new Vector2(105f, 0f), new Vector2(150f, 90f));
        UnityEventTools.AddPersistentListener(quitButton.onClick, controls.Quit);

        controls.pauseText = pause.GetComponentsInChildren<Text>(true).First();
        controls.quitButton = quit.gameObject;
        controls.fullScreenToggle = fullScreen;
        EditorUtility.SetDirty(controls);
    }

    static Toggle CheckRow(RectTransform panel, string name, string text, float y, bool on)
    {
        // The whole line is the hit area; the visible parts are the box
        // (salmon cube, tinted like the original) and the green cube check.
        var row = Rect(panel, name, Color.clear, new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(480f, 84f));
        row.GetComponent<UnityEngine.UI.Image>().raycastTarget = true;
        var toggle = row.gameObject.AddComponent<Toggle>();

        var box = Block(row, "Box", cube, new Vector2(0f, 0.5f), new Vector2(36f, 0f), new Vector2(56f, 56f), Color.white);
        var check = Block(box, "Check", green, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(30f, 30f), Color.white);
        toggle.targetGraphic = box.GetComponent<UnityEngine.UI.Image>();
        toggle.graphic = check.GetComponent<UnityEngine.UI.Image>();
        toggle.colors = Tint(BoxTint, Color.white, new Color(0.6f, 0.6f, 0.6f, 1f));
        toggle.isOn = on;
        check.GetComponent<UnityEngine.UI.Image>().canvasRenderer.SetAlpha(on ? 1f : 0f);

        Label(row, "Label", text, 40, Ink, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f), new Vector2(90f + 170f, 0f), new Vector2(340f, 84f));
        return toggle;
    }

    // --------------------------------------------------------- other pieces

    static void RethemeStats(GameObject statsPanel)
    {
        statsPanel.GetComponent<UnityEngine.UI.Image>().color = Panel;
        foreach (var text in statsPanel.GetComponentsInChildren<Text>(true))
        {
            bool isLabel = text.name.EndsWith("Label");
            text.color = isLabel ? InkSoft : Ink;
            text.fontStyle = FontStyle.Bold;
            if (text.name == "Holes") text.color = Hud.HoleColor;
            EditorUtility.SetDirty(text);
        }
        EditorUtility.SetDirty(statsPanel.GetComponent<UnityEngine.UI.Image>());
    }

    static void RethemeOverlays(System.Func<string, GameObject> find)
    {
        // Overlays use the near-black of the Next box, with white text.
        foreach (var name in new[] { "PausedPanel", "GameOverPanel" })
        {
            var panel = find(name);
            panel.GetComponent<UnityEngine.UI.Image>().color = new Color(PreviewBox.r, PreviewBox.g, PreviewBox.b, 0.88f);
            foreach (var text in panel.GetComponentsInChildren<Text>(true))
            {
                if (text.name == "Hint") text.color = new Color(1f, 1f, 1f, 0.7f);
                else if (text.name == "Title" && name == "GameOverPanel") text.color = Hud.AccentColor;
                else if (text.transform.parent.name != "RestartButton") text.color = Color.white;
                EditorUtility.SetDirty(text);
            }
        }
        var restart = find("RestartButton");
        restart.GetComponent<UnityEngine.UI.Image>().color = Color.white;
        var button = restart.GetComponent<Button>();
        button.colors = Tint(Salmon, new Color(0.88f, 0.52f, 0.55f), new Color(0.65f, 0.33f, 0.36f));
        var restartLabel = restart.GetComponentInChildren<Text>(true);
        restartLabel.color = Color.white;
        EditorUtility.SetDirty(button);
        EditorUtility.SetDirty(restartLabel);

        // Banners sit on the light board: white outline for contrast.
        var banner = find("Banner").GetComponent<Text>();
        banner.color = Hud.AccentColor;
        var outline = banner.GetComponent<Outline>();
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(3f, -3f);
        EditorUtility.SetDirty(banner);
        EditorUtility.SetDirty(outline);
    }

    static void RethemeParticles(System.Func<string, GameObject> find)
    {
        // Line clears burst in board white and the salmon of the cubes.
        var line = find("LineBurst").GetComponent<ParticleSystem>().main;
        line.startColor = new ParticleSystem.MinMaxGradient(Color.white, Salmon);
        EditorUtility.SetDirty(find("LineBurst").GetComponent<ParticleSystem>());
    }

    // -------------------------------------------------------------- helpers

    static Sprite Load(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);

    // Block textures are sprite sheets whose sprite is cropped to the cube.
    static RectTransform Block(RectTransform parent, string name, Sprite sprite, Vector2 anchor, Vector2 center, Vector2 size, Color color)
    {
        var rt = Image(parent, name, sprite, color, anchor, center, size);
        return rt;
    }

    static RectTransform Image(RectTransform parent, string name, Sprite sprite, Color color, Vector2 anchor, Vector2 position, Vector2 size)
    {
        var rt = Rect(parent, name, color, anchor, position, size);
        rt.GetComponent<UnityEngine.UI.Image>().sprite = sprite;
        return rt;
    }

    static RectTransform Rect(RectTransform parent, string name, Color color, Vector2 anchor, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        var image = go.GetComponent<UnityEngine.UI.Image>();
        image.color = color;
        image.raycastTarget = false;
        return rt;
    }

    static ColorBlock Tint(Color normal, Color hover, Color pressed)
    {
        var block = ColorBlock.defaultColorBlock;
        block.normalColor = normal;
        block.highlightedColor = hover;
        block.selectedColor = normal;
        block.pressedColor = pressed;
        block.fadeDuration = 0.08f;
        return block;
    }

    static Text Label(RectTransform parent, string name, string text, int size, Color color, TextAnchor align, Vector2 anchor, Vector2 position, Vector2 box)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.anchoredPosition = position;
        rt.sizeDelta = box;
        var t = go.GetComponent<Text>();
        t.font = font;
        t.text = text;
        t.fontSize = size;
        t.fontStyle = FontStyle.Bold;
        t.color = color;
        t.alignment = align;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }
}
