using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// One-off: lays the whole UI out on one system (original visual language):
//   - two side columns, 580 wide, 22 from the screen edge, board centered;
//   - top row of four equal boxes, mirrored: SCORE | HOLD  ...  NEXT | STATS;
//   - bottom row: CONTROLS and MENU filling the remaining height;
//   - one panel style (white 39%), 28 padding, left-aligned titles,
//     one type scale, 54px blocks on a 66px row pitch.
//   Unity.exe -batchmode -quit -projectPath . -executeMethod HarmonizeUi.Run
public static class HarmonizeUi
{
    const string ScenePath = "Assets/Scenes/Level0.unity";

    // Layout (1920-wide reference canvas).
    const float Margin = 22f, Gap = 20f, Pad = 28f;
    const float ColumnWidth = 580f, BoxWidth = (ColumnWidth - Gap) / 2f, TopHeight = 370f;
    const float Block = 54f, RowPitch = 66f, FirstRow = -112f;

    // Type scale.
    const int TitleSize = 26, RowSize = 28, ValueSize = 44, ScoreSize = 72;

    // Colors from the original scene.
    static readonly Color PanelColor = new Color(1f, 1f, 1f, 0.392f);
    static readonly Color Ink = new Color(0.196f, 0.196f, 0.196f, 1f);
    static readonly Color InkMuted = new Color(0.196f, 0.196f, 0.196f, 0.7f);
    static readonly Color Well = new Color(0.047f, 0.047f, 0.047f, 1f);
    static readonly Color BoxTint = new Color(0.739f, 0.739f, 0.739f, 1f);

    static Font font;
    static Sprite cube, green, grid, arrow, cross;

    [MenuItem("ZKTris/Harmonize UI")]
    public static void Run()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath);
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        cube = Load("Assets/Imports/BaseTetrisCube.png");
        green = Load("Assets/Imports/Green.png");
        grid = Load("Assets/Imports/TetrisGrid.png");
        arrow = Load("Assets/Imports/Arrow.png");
        cross = Load("Assets/Imports/ErrorBloc.png");

        var all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).Select(t => t.gameObject).ToList();
        GameObject Find(string name) => all.First(g => g != null && g.name == name);

        // Center the board: its cells span x -10..11, the camera sat at 1.67.
        var camera = Find("Main Camera").transform;
        camera.localPosition = new Vector3(0.5f, camera.localPosition.y, camera.localPosition.z);
        EditorUtility.SetDirty(camera);

        var canvas = (RectTransform)Find("Canvas").transform;
        var hudRoot = (RectTransform)Find("HUD").transform;
        var hud = hudRoot.GetComponent<Hud>();
        var form = Object.FindAnyObjectByType<Form>(FindObjectsInactive.Include);
        var controls = Object.FindAnyObjectByType<Controls>(FindObjectsInactive.Include);

        // Keep the two preview images (Form references them), drop the rest.
        var nextImage = (RectTransform)Find("NextFormPreview").transform;
        var holdImage = (RectTransform)Find("HoldFormPreview").transform;
        nextImage.SetParent(hudRoot, false);
        holdImage.SetParent(hudRoot, false);
        foreach (var name in new[] { "PreviewBackground", "HoldBackground", "StatsPanel", "ControlsLegend", "ScorePanel", "NextPanel", "HoldPanel" })
        {
            var old = all.FirstOrDefault(g => g != null && g.name == name);
            if (old != null) Object.DestroyImmediate(old);
        }
        all.RemoveAll(g => g == null);

        // ---- top row
        var scorePanel = TopBox(hudRoot, "ScorePanel", left: true, inner: false);
        Title(scorePanel, "SCORE", -43f);
        hud.score = Value(scorePanel, "Score", ScoreSize, Ink, -112f);
        Title(scorePanel, "BEST", -273f);
        hud.best = Value(scorePanel, "Best", ValueSize, Ink, -322f);

        var holdPanel = TopBox(hudRoot, "HoldPanel", left: true, inner: true);
        Title(holdPanel, "HOLD", -43f);
        PlaceInWell(holdPanel, holdImage);

        var nextPanel = TopBox(hudRoot, "NextPanel", left: false, inner: true);
        Title(nextPanel, "NEXT", -43f);
        PlaceInWell(nextPanel, nextImage);

        var statsPanel = TopBox(hudRoot, "StatsPanel", left: false, inner: false);
        Title(statsPanel, "LEVEL", -43f);
        hud.level = Value(statsPanel, "Level", ValueSize, Ink, -92f);
        Title(statsPanel, "LINES", -158f);
        hud.lines = Value(statsPanel, "Lines", ValueSize, Ink, -207f);
        Title(statsPanel, "HOLES", -273f);
        hud.holes = Value(statsPanel, "Holes", ValueSize, Hud.HoleColor, -322f);
        EditorUtility.SetDirty(hud);
        EditorUtility.SetDirty(form);

        // ---- bottom row
        BuildLegend(BottomBox(canvas, "ControlsLegend", null, left: true));
        BuildMenu(BottomBox(canvas, "Controls", (RectTransform)controls.transform, left: false), controls);

        // ---- overlays, centered on the (now centered) board
        foreach (var name in new[] { "PausedPanel", "GameOverPanel" })
        {
            var rt = (RectTransform)Find(name).transform;
            rt.anchoredPosition = new Vector2(0f, rt.anchoredPosition.y);
            rt.sizeDelta = new Vector2(480f, rt.sizeDelta.y);
            EditorUtility.SetDirty(rt);
        }
        var banner = (RectTransform)Find("Banner").transform;
        banner.anchoredPosition = new Vector2(0f, banner.anchoredPosition.y);
        SetText(Find("PausedPanel"), "Hint", "PRESS ENTER OR P", 26);
        SetText(Find("GameOverPanel"), "Hint", "OR PRESS ENTER", 24);

        foreach (var selectable in Object.FindObjectsByType<Selectable>(FindObjectsInactive.Include))
        {
            selectable.navigation = new Navigation { mode = Navigation.Mode.None };
            EditorUtility.SetDirty(selectable);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[HarmonizeUi] done");
    }

    // ------------------------------------------------------------- top row

    static RectTransform TopBox(RectTransform parent, string name, bool left, bool inner)
    {
        // left column: outer box at the screen edge, inner box next to the
        // board; the right column mirrors it.
        float edge = Margin + (inner ? BoxWidth + Gap : 0f);
        var rt = Panel(parent, name);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(left ? 0f : 1f, 1f);
        rt.anchoredPosition = new Vector2(left ? edge : -edge, -Margin);
        rt.sizeDelta = new Vector2(BoxWidth, TopHeight);
        return rt;
    }

    static void PlaceInWell(RectTransform panel, RectTransform preview)
    {
        var well = Rect(panel, "Well", Well, new Vector2(0.5f, 1f), new Vector2(0f, -76f - 133f), new Vector2(BoxWidth - 2f * Pad, 266f));
        preview.SetParent(well, false);
        preview.anchorMin = preview.anchorMax = preview.pivot = new Vector2(0.5f, 0.5f);
        preview.anchoredPosition = Vector2.zero;
        preview.localScale = Vector3.one;
        preview.sizeDelta = new Vector2(208f, 117f); // sprites are 16:9
        preview.GetComponent<Image>().preserveAspect = true;
        EditorUtility.SetDirty(preview);
    }

    // ---------------------------------------------------------- bottom row

    static RectTransform BottomBox(RectTransform canvas, string name, RectTransform existing, bool left)
    {
        RectTransform rt;
        if (existing != null)
        {
            rt = existing;
            for (int i = rt.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(rt.GetChild(i).gameObject);
            var image = rt.GetComponent<Image>();
            if (image == null) image = rt.gameObject.AddComponent<Image>();
            image.sprite = null;
            image.color = PanelColor;
            image.raycastTarget = false;
            EditorUtility.SetDirty(image);
        }
        else
        {
            rt = Panel(canvas, name);
        }
        // Stretches vertically: from below the top row to the bottom margin.
        float x = left ? 0f : 1f;
        rt.localScale = Vector3.one;
        rt.anchorMin = new Vector2(x, 0f);
        rt.anchorMax = new Vector2(x, 1f);
        rt.pivot = new Vector2(x, 1f);
        rt.offsetMin = new Vector2(left ? Margin : -Margin - ColumnWidth, Margin);
        rt.offsetMax = new Vector2(left ? Margin + ColumnWidth : -Margin, -(Margin + TopHeight + Gap));
        EditorUtility.SetDirty(rt);
        return rt;
    }

    static void BuildLegend(RectTransform panel)
    {
        Title(panel, "CONTROLS", -43f);
        // Keys start at the padding; actions line up after the widest group.
        const float labelX = Pad + 130f + 22f;
        float y = FirstRow;
        void Line(string action, params string[] keys)
        {
            float x = Pad;
            foreach (var key in keys)
            {
                float width = key.Length > 2 ? 130f : Block;
                KeyCap(panel, key, new Vector2(x + width / 2f, y), width);
                x += width + 8f;
            }
            MakeText(panel, action, action, RowSize, Ink, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(labelX + 170f, y), new Vector2(340f, Block));
            y -= RowPitch;
        }
        Line("MOVE", "←", "→");
        Line("ROTATE", "↑", "X");
        Line("ROTATE BACK", "Z");
        Line("SOFT DROP", "↓");
        Line("HARD DROP", "SPACE");
        Line("HOLD", "C");
        Line("PAUSE", "P");
    }

    static void KeyCap(RectTransform panel, string key, Vector2 center, float width)
    {
        var cap = MakeImage(panel, "Key " + key, grid, Color.white, new Vector2(0f, 1f), center, new Vector2(width, Block));
        float angle;
        switch (key)
        {
            case "←": angle = 180f; break;
            case "→": angle = 0f; break;
            case "↑": angle = 90f; break;
            case "↓": angle = -90f; break;
            default:
                MakeText(cap, "Label", key, key.Length > 2 ? 22 : 26, Ink, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(width, Block));
                return;
        }
        var icon = MakeImage(cap, "Arrow", arrow, Ink, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(30f, 20f));
        icon.localEulerAngles = new Vector3(0f, 0f, angle);
    }

    static void BuildMenu(RectTransform panel, Controls controls)
    {
        Title(panel, "MENU", -43f);
        var pause = CheckRow(panel, "PausePlay", "PLAY", FirstRow, false);
        UnityEventTools.AddVoidPersistentListener(pause.onValueChanged, controls.TogglePause);
        var fullScreen = CheckRow(panel, "FullScreen", "FULL SCREEN", FirstRow - RowPitch, false);
        UnityEventTools.AddVoidPersistentListener(fullScreen.onValueChanged, controls.ToggleFullScreen);
        var music = CheckRow(panel, "Music", "MUSIC", FirstRow - 2f * RowPitch, true);
        UnityEventTools.AddVoidPersistentListener(music.onValueChanged, controls.ToggleMusic);
        var sounds = CheckRow(panel, "Sounds", "SOUNDS", FirstRow - 3f * RowPitch, true);
        UnityEventTools.AddVoidPersistentListener(sounds.onValueChanged, controls.ToggleSounds);

        // Quit: same row shape, the crossed "hole" block as its icon.
        var quit = Row(panel, "Quit", FirstRow - 4f * RowPitch);
        var icon = MakeImage(quit, "Icon", cross, Color.white, new Vector2(0f, 0.5f), new Vector2(Block / 2f, 0f), new Vector2(Block, Block));
        var button = quit.gameObject.AddComponent<Button>();
        button.targetGraphic = icon.GetComponent<Image>();
        button.colors = Tint(BoxTint, Color.white, new Color(0.6f, 0.6f, 0.6f, 1f));
        RowLabel(quit, "QUIT");
        UnityEventTools.AddPersistentListener(button.onClick, controls.Quit);

        controls.pauseText = pause.GetComponentsInChildren<Text>(true).First();
        controls.quitButton = quit.gameObject;
        controls.fullScreenToggle = fullScreen;
        EditorUtility.SetDirty(controls);
    }

    static Toggle CheckRow(RectTransform panel, string name, string label, float y, bool on)
    {
        var row = Row(panel, name, y);
        var toggle = row.gameObject.AddComponent<Toggle>();
        var box = MakeImage(row, "Box", cube, Color.white, new Vector2(0f, 0.5f), new Vector2(Block / 2f, 0f), new Vector2(Block, Block));
        var check = MakeImage(box, "Check", green, Color.white, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(28f, 28f));
        toggle.targetGraphic = box.GetComponent<Image>();
        toggle.graphic = check.GetComponent<Image>();
        toggle.colors = Tint(BoxTint, Color.white, new Color(0.6f, 0.6f, 0.6f, 1f));
        toggle.isOn = on;
        check.GetComponent<Image>().canvasRenderer.SetAlpha(on ? 1f : 0f);
        RowLabel(row, label);
        return toggle;
    }

    // A full-width, invisible hit area at the padding, one block tall.
    static RectTransform Row(RectTransform panel, string name, float y)
    {
        var row = Rect(panel, name, Color.clear, new Vector2(0f, 1f), new Vector2(Pad + (ColumnWidth - 2f * Pad) / 2f, y), new Vector2(ColumnWidth - 2f * Pad, Block));
        row.GetComponent<Image>().raycastTarget = true;
        return row;
    }

    static void RowLabel(RectTransform row, string label)
    {
        const float x = Block + 22f;
        MakeText(row, "Label", label, RowSize, Ink, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f), new Vector2(x + 200f, 0f), new Vector2(400f, Block));
    }

    // -------------------------------------------------------------- helpers

    static RectTransform Panel(RectTransform parent, string name)
    {
        var rt = Rect(parent, name, PanelColor, new Vector2(0f, 1f), Vector2.zero, Vector2.zero);
        rt.GetComponent<Image>().raycastTarget = false;
        return rt;
    }

    static void Title(RectTransform panel, string text, float y) =>
        MakeText(panel, "Title " + text, text, TitleSize, InkMuted, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(Pad + 150f, y), new Vector2(300f, 34f));

    static Text Value(RectTransform panel, string name, int size, Color color, float y) =>
        MakeText(panel, name, "0", size, color, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(Pad + 150f, y), new Vector2(300f, size * 1.3f));

    static void SetText(GameObject panel, string child, string text, int size)
    {
        var t = panel.GetComponentsInChildren<Text>(true).First(x => x.name == child);
        t.text = text;
        t.fontSize = size;
        EditorUtility.SetDirty(t);
    }

    static Sprite Load(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);

    static RectTransform MakeImage(RectTransform parent, string name, Sprite sprite, Color color, Vector2 anchor, Vector2 position, Vector2 size)
    {
        var rt = Rect(parent, name, color, anchor, position, size);
        rt.GetComponent<Image>().sprite = sprite;
        return rt;
    }

    static RectTransform Rect(RectTransform parent, string name, Color color, Vector2 anchor, Vector2 position, Vector2 size)
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

    static Text MakeText(RectTransform parent, string name, string text, int size, Color color, TextAnchor align, Vector2 anchor, Vector2 position, Vector2 box)
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
