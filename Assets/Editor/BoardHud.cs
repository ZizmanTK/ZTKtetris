using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// One-off: replaces the boxed panels with a board-centric HUD (Hold / Next
// as thin salmon frames hanging off the board, floating stats, side
// banner) and a pause screen (text menu + outlined key list).
//   Unity.exe -batchmode -quit -projectPath . -executeMethod BoardHud.Run
public static class BoardHud
{
    const string ScenePath = "Assets/Scenes/Level0.unity";
    const string WhiteArrowPath = "Assets/Imports/ArrowWhite.png";

    static readonly Color Cream = new Color(0.96f, 0.9f, 0.8f, 1f);
    static readonly Color CreamMuted = new Color(0.96f, 0.9f, 0.8f, 0.75f);
    static readonly Color Salmon = new Color(0.808f, 0.42f, 0.45f, 1f);

    const float Gap = 24f, FrameWidth = 220f, Line = 3f;

    static Font regular, bold;
    static Sprite arrowWhite;

    [MenuItem("ZKTris/Build Board HUD")]
    public static void Run()
    {
        arrowWhite = MakeWhiteArrow();
        var scene = EditorSceneManager.OpenScene(ScenePath);
        regular = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/BarlowCondensed-Medium.ttf");
        bold = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/BarlowCondensed-SemiBold.ttf");

        var all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).Select(t => t.gameObject).ToList();
        GameObject Find(string name) => all.First(g => g != null && g.name == name);

        var hudRoot = (RectTransform)Find("HUD").transform;
        var overlay = (RectTransform)Find("Overlay").transform;
        var hud = hudRoot.GetComponent<Hud>();
        var form = Object.FindAnyObjectByType<Form>(FindObjectsInactive.Include);
        var controls = Object.FindAnyObjectByType<Controls>(FindObjectsInactive.Include);
        var game = Object.FindAnyObjectByType<TetrimonoBehaviour>(FindObjectsInactive.Include);
        var camera = Find("Main Camera").GetComponent<Camera>();

        // Keep the preview images Form already references.
        var holdImage = (RectTransform)Find("HoldFormPreview").transform;
        var nextImage = (RectTransform)Find("NextFormPreview").transform;
        holdImage.SetParent(hudRoot, false);
        nextImage.SetParent(hudRoot, false);

        foreach (var name in new[] { "ScorePanel", "HoldPanel", "NextPanel", "StatsPanel", "ControlsLegend",
                                     "PausedPanel", "GameOverPanel", "Banner", "BoardFrame", "KeyHint", "PauseScreen" })
            foreach (var old in all.Where(g => g != null && g.name == name).ToList())
                Object.DestroyImmediate(old);
        all.RemoveAll(g => g == null);

        // The old Controls panel only keeps its script.
        var controlsRt = (RectTransform)controls.transform;
        for (int i = controlsRt.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(controlsRt.GetChild(i).gameObject);
        var oldImage = controlsRt.GetComponent<Image>();
        if (oldImage != null) Object.DestroyImmediate(oldImage);

        // ---- board frame: everything below hangs off the board's edges
        var frameGo = new GameObject("BoardFrame", typeof(RectTransform));
        var frame = (RectTransform)frameGo.transform;
        frame.SetParent(hudRoot, false);
        var boardFrame = frameGo.AddComponent<BoardFrame>();
        boardFrame.board = Find("Borders").GetComponent<Renderer>();
        boardFrame.worldCamera = camera;
        boardFrame.Refresh();

        // Hold, left of the board.
        Label(frame, "HoldLabel", "HOLD", regular, 28, CreamMuted, Side.Left, -18f);
        Bracket(frame, "HoldFrame", Side.Left, -44f, -214f);
        Place(holdImage, frame, Side.Left, -129f, new Vector2(180f, 101f), 1f);
        form.holdPreview = holdImage.GetComponent<Image>();

        // Stats, right-aligned toward the board.
        Label(frame, "ScoreLabel", "SCORE", regular, 26, CreamMuted, Side.Left, -262f);
        hud.score = Label(frame, "Score", "0", bold, 76, Color.white, Side.Left, -312f);
        hud.best = Label(frame, "Best", "BEST 0", regular, 26, CreamMuted, Side.Left, -370f);
        hud.level = Label(frame, "Level", "LEVEL 1", regular, 26, CreamMuted, Side.Left, -440f);
        hud.levelProgress = ProgressLine(frame, -472f);
        Label(frame, "LinesLabel", "LINES", regular, 26, CreamMuted, Side.Left, -520f);
        hud.lines = Label(frame, "Lines", "0", bold, 48, Color.white, Side.Left, -560f);
        Label(frame, "HolesLabel", "HOLES", regular, 26, CreamMuted, Side.Left, -620f);
        hud.holes = Label(frame, "Holes", "0", bold, 48, Hud.HoleColor, Side.Left, -660f);
        hud.banner = Label(frame, "Banner", "DOUBLE", bold, 56, Color.white, Side.Left, -760f);
        EditorUtility.SetDirty(hud);

        // Next queue, right of the board: three slots shrinking with distance
        // (alpha would muddy the crimson against the orange).
        Label(frame, "NextLabel", "NEXT", regular, 28, CreamMuted, Side.Right, -18f);
        Bracket(frame, "NextFrame", Side.Right, -44f, -444f);
        Place(nextImage, frame, Side.Right, -129f, new Vector2(180f, 101f), 1f);
        var next2 = Object.Instantiate(nextImage.gameObject, frame).GetComponent<RectTransform>();
        next2.name = "NextFormPreview2";
        Place(next2, frame, Side.Right, -262f, new Vector2(130f, 73f), 1f);
        var next3 = Object.Instantiate(nextImage.gameObject, frame).GetComponent<RectTransform>();
        next3.name = "NextFormPreview3";
        Place(next3, frame, Side.Right, -372f, new Vector2(104f, 59f), 1f);
        form.nextPreviews = new[] { nextImage.GetComponent<Image>(), next2.GetComponent<Image>(), next3.GetComponent<Image>() };
        EditorUtility.SetDirty(form);

        // One quiet hint, bottom right.
        var hint = MakeText(hudRoot, "KeyHint", "ESC  MENU      C  HOLD      SPACE  DROP", regular, 26, new Color(Cream.r, Cream.g, Cream.b, 0.6f), TextAnchor.LowerRight);
        var hintRt = hint.rectTransform;
        hintRt.anchorMin = hintRt.anchorMax = hintRt.pivot = new Vector2(1f, 0f);
        hintRt.anchoredPosition = new Vector2(-32f, 22f);
        hintRt.sizeDelta = new Vector2(700f, 40f);

        // ---- pause / start / game over screen
        var menu = BuildPauseScreen(overlay, controls);
        controls.menu = menu;
        EditorUtility.SetDirty(controls);
        game.menu = menu;
        game.hud = hud;
        EditorUtility.SetDirty(game);

        foreach (var selectable in Object.FindObjectsByType<Selectable>(FindObjectsInactive.Include))
        {
            selectable.navigation = new Navigation { mode = Navigation.Mode.None };
            EditorUtility.SetDirty(selectable);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[BoardHud] done");
    }

    // ------------------------------------------------------------- HUD bits

    enum Side { Left, Right }

    // Text hanging off the left/right edge of the board, aligned toward it.
    static Text Label(RectTransform frame, string name, string text, Font font, int size, Color color, Side side, float y)
    {
        bool left = side == Side.Left;
        var t = MakeText(frame, name, text, font, size, color, left ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft);
        var rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(left ? 0f : 1f, 1f);
        rt.pivot = new Vector2(left ? 1f : 0f, 0.5f);
        rt.anchoredPosition = new Vector2(left ? -Gap : Gap, y);
        rt.sizeDelta = new Vector2(420f, size * 1.25f);
        return t;
    }

    // Open frame growing out of the board border: top, outer side, bottom.
    static void Bracket(RectTransform frame, string name, Side side, float top, float bottom)
    {
        bool left = side == Side.Left;
        float sign = left ? -1f : 1f;
        float x = left ? 0f : 1f;
        var root = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        root.SetParent(frame, false);
        root.anchorMin = root.anchorMax = new Vector2(x, 1f);
        root.pivot = new Vector2(x, 1f);
        root.anchoredPosition = Vector2.zero;
        root.sizeDelta = Vector2.zero;
        Bar(root, "Top", new Vector2(sign * FrameWidth / 2f, top), new Vector2(FrameWidth, Line));
        Bar(root, "Bottom", new Vector2(sign * FrameWidth / 2f, bottom), new Vector2(FrameWidth, Line));
        Bar(root, "Side", new Vector2(sign * FrameWidth, (top + bottom) / 2f), new Vector2(Line, top - bottom + Line));
    }

    static void Bar(RectTransform parent, string name, Vector2 center, Vector2 size)
    {
        var image = MakeImage(parent, name, Salmon, null);
        var rt = image.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = center;
        rt.sizeDelta = size;
    }

    static void Place(RectTransform image, RectTransform frame, Side side, float y, Vector2 size, float alpha)
    {
        bool left = side == Side.Left;
        image.SetParent(frame, false);
        image.anchorMin = image.anchorMax = new Vector2(left ? 0f : 1f, 1f);
        image.pivot = new Vector2(0.5f, 0.5f);
        image.anchoredPosition = new Vector2((left ? -1f : 1f) * FrameWidth / 2f, y);
        image.sizeDelta = size;
        image.localScale = Vector3.one;
        var img = image.GetComponent<Image>();
        img.preserveAspect = true;
        img.color = new Color(1f, 1f, 1f, alpha);
        EditorUtility.SetDirty(img);
    }

    static RectTransform ProgressLine(RectTransform frame, float y)
    {
        var track = MakeImage(frame, "LevelTrack", new Color(Cream.r, Cream.g, Cream.b, 0.3f), null).rectTransform;
        track.anchorMin = track.anchorMax = new Vector2(0f, 1f);
        track.pivot = new Vector2(1f, 0.5f);
        track.anchoredPosition = new Vector2(-Gap, y);
        track.sizeDelta = new Vector2(180f, 4f);
        var fill = MakeImage(track, "Fill", Color.white, null).rectTransform;
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = new Vector2(0f, 1f);
        fill.offsetMin = fill.offsetMax = Vector2.zero;
        return fill;
    }

    // ---------------------------------------------------------- pause screen

    static PauseMenu BuildPauseScreen(RectTransform overlay, Controls controls)
    {
        var root = new GameObject("PauseScreen", typeof(RectTransform)).GetComponent<RectTransform>();
        root.SetParent(overlay, false);
        Stretch(root);
        var dim = MakeImage(root, "Dim", new Color(0.08f, 0.08f, 0.08f, 0.97f), null);
        Stretch(dim.rectTransform);
        dim.raycastTarget = true;

        var menu = root.gameObject.AddComponent<PauseMenu>();
        menu.root = root.gameObject;
        menu.controls = controls;
        menu.idleColor = new Color(Cream.r, Cream.g, Cream.b, 0.6f);

        menu.title = Centered(root, "Title", "ZKTRIS", bold, 130, Color.white, new Vector2(-500f, 290f), new Vector2(700f, 150f), TextAnchor.MiddleLeft);
        menu.subtitle = Centered(root, "Subtitle", "UNFORGIVABLE", regular, 36, Salmon, new Vector2(-496f, 200f), new Vector2(700f, 50f), TextAnchor.MiddleLeft);

        float y = 100f;
        PauseMenuItem Item(string name, string label, bool setting)
        {
            var item = MakeItem(root, name, label, setting, y);
            y -= 72f;
            return item;
        }
        menu.resume = Item("Resume", "RESUME", false);
        menu.restart = Item("Restart", "RESTART", false);
        menu.music = Item("Music", "MUSIC", true);
        menu.sounds = Item("Sounds", "SOUNDS", true);
        menu.fullScreen = Item("FullScreen", "FULL SCREEN", true);
        menu.quit = Item("Quit", "QUIT", false);

        var marker = MakeImage(root, "Marker", Salmon, null).rectTransform;
        marker.anchorMin = marker.anchorMax = marker.pivot = new Vector2(0.5f, 0.5f);
        marker.anchoredPosition = new Vector2(-526f, 100f);
        marker.sizeDelta = new Vector2(6f, 46f);
        menu.marker = marker;

        var divider = MakeImage(root, "Divider", new Color(1f, 1f, 1f, 0.2f), null).rectTransform;
        divider.anchorMin = divider.anchorMax = divider.pivot = new Vector2(0.5f, 0.5f);
        divider.anchoredPosition = new Vector2(0f, -20f);
        divider.sizeDelta = new Vector2(2f, 620f);

        Centered(root, "ControlsTitle", "CONTROLS", regular, 30, CreamMuted, new Vector2(80f, 230f), new Vector2(400f, 40f), TextAnchor.MiddleLeft);
        float ky = 160f;
        void Keys(string action, params string[] keys)
        {
            float x = 80f;
            foreach (var key in keys)
            {
                float width = key.Length > 2 ? 120f : 52f;
                KeyOutline(root, key, new Vector2(x, ky), width);
                x += width + 10f;
            }
            Centered(root, "Action " + action, action, regular, 32, Cream, new Vector2(256f, ky), new Vector2(400f, 52f), TextAnchor.MiddleLeft);
            ky -= 62f;
        }
        Keys("MOVE", "←", "→");
        Keys("ROTATE", "↑", "X");
        Keys("ROTATE BACK", "Z");
        Keys("SOFT DROP", "↓");
        Keys("HARD DROP", "SPACE");
        Keys("HOLD", "C");
        Keys("PAUSE", "ESC");

        root.gameObject.SetActive(false);
        return menu;
    }

    static PauseMenuItem MakeItem(RectTransform root, string name, string label, bool setting, float y)
    {
        var hit = MakeImage(root, name, Color.clear, null);
        hit.raycastTarget = true;
        var rt = hit.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = new Vector2(-506f, y);
        rt.sizeDelta = new Vector2(400f, 64f);
        var item = hit.gameObject.AddComponent<PauseMenuItem>();
        item.isSetting = setting;
        item.label = MakeText(rt, "Label", label, bold, 48, Color.white, TextAnchor.MiddleLeft);
        Fill(item.label.rectTransform, 0f, 0f);
        if (setting)
        {
            item.value = MakeText(rt, "Value", "ON", regular, 34, Color.white, TextAnchor.MiddleRight);
            Fill(item.value.rectTransform, 0f, 0f);
        }
        return item;
    }

    // Outlined key cap: four thin lines, white key legend or arrow.
    static void KeyOutline(RectTransform root, string key, Vector2 leftCenter, float width)
    {
        var cap = new GameObject("Key " + key, typeof(RectTransform)).GetComponent<RectTransform>();
        cap.SetParent(root, false);
        cap.anchorMin = cap.anchorMax = new Vector2(0.5f, 0.5f);
        cap.pivot = new Vector2(0f, 0.5f);
        cap.anchoredPosition = leftCenter;
        cap.sizeDelta = new Vector2(width, 52f);
        var edge = new Color(1f, 1f, 1f, 0.7f);
        void Edge(string n, Vector2 min, Vector2 max)
        {
            var e = MakeImage(cap, n, edge, null).rectTransform;
            e.anchorMin = min;
            e.anchorMax = max;
            e.offsetMin = e.offsetMax = Vector2.zero;
        }
        const float t = 0.04f;
        Edge("T", new Vector2(0f, 1f - t), Vector2.one);
        Edge("B", Vector2.zero, new Vector2(1f, t));
        Edge("L", Vector2.zero, new Vector2(2f / width, 1f));
        Edge("R", new Vector2(1f - 2f / width, 0f), Vector2.one);

        float angle;
        switch (key)
        {
            case "←": angle = 180f; break;
            case "→": angle = 0f; break;
            case "↑": angle = 90f; break;
            case "↓": angle = -90f; break;
            default:
                var legend = MakeText(cap, "Legend", key, bold, key.Length > 2 ? 24 : 28, Color.white, TextAnchor.MiddleCenter);
                Fill(legend.rectTransform, 0f, 0f);
                return;
        }
        var icon = MakeImage(cap, "Arrow", Color.white, arrowWhite).rectTransform;
        icon.anchorMin = icon.anchorMax = icon.pivot = new Vector2(0.5f, 0.5f);
        icon.anchoredPosition = Vector2.zero;
        icon.sizeDelta = new Vector2(27f, 18f);
        icon.localEulerAngles = new Vector3(0f, 0f, angle);
    }

    // The game's arrow is black; outlined caps need it white.
    static Sprite MakeWhiteArrow()
    {
        var source = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Imports/Arrow.png");
        var rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(source, rt);
        var previous = RenderTexture.active;
        RenderTexture.active = rt;
        var tex = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);
        var pixels = tex.GetPixels32();
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Color32(255, 255, 255, pixels[i].a);
        tex.SetPixels32(pixels);
        File.WriteAllBytes(WhiteArrowPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(WhiteArrowPath);
        var importer = (TextureImporter)AssetImporter.GetAtPath(WhiteArrowPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(WhiteArrowPath);
    }

    // -------------------------------------------------------------- helpers

    static Text Centered(RectTransform root, string name, string text, Font font, int size, Color color, Vector2 leftCenter, Vector2 box, TextAnchor align)
    {
        var t = MakeText(root, name, text, font, size, color, align);
        var rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = leftCenter;
        rt.sizeDelta = box;
        return t;
    }

    static Text MakeText(RectTransform parent, string name, string text, Font font, int size, Color color, TextAnchor align)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<Text>();
        t.font = font;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    static Image MakeImage(RectTransform parent, string name, Color color, Sprite sprite)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        image.sprite = sprite;
        image.raycastTarget = false;
        return image;
    }

    static void Fill(RectTransform rt, float left, float right)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(left, 0f);
        rt.offsetMax = new Vector2(-right, 0f);
    }

    static void Stretch(RectTransform rt) => Fill(rt, 0f, 0f);
}
