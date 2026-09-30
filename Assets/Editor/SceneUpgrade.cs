using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

// One-off migration of Level0 to the new game scripts: removes the dead
// components, wires the new references and builds the HUD, hold preview,
// ghost piece and particles. Safe to run again.
//   Unity.exe -batchmode -quit -projectPath . -executeMethod SceneUpgrade.Run
public static class SceneUpgrade
{
    const string ScenePath = "Assets/Scenes/Level0.unity";

    static readonly Color PanelColor = new Color(0.047f, 0.047f, 0.047f, 1f);
    static readonly Color DarkText = new Color(0.196f, 0.196f, 0.196f, 1f);
    static readonly Color Dim = new Color(1f, 1f, 1f, 0.6f);

    static Font font;

    [MenuItem("ZKTris/Upgrade Level0 Scene")]
    public static void Run()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath);
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).Select(t => t.gameObject).ToList();
        GameObject Find(string name) => all.First(g => g.name == name);
        T Get<T>() where T : Object => Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);

        // Old MoveTetrimono components point at a deleted script.
        foreach (var go in all)
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);

        var grid = Get<GameGrid>();
        grid.tilemap = Find("GameGrid").GetComponent<Tilemap>();
        grid.emptyTile = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Imports/Tiles/TetrisGrid_0.asset");
        EditorUtility.SetDirty(grid);

        var sounds = Get<Sounds>();
        sounds.hardDrop = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Imports/mixkit-game-whip-shot-1512.wav");
        EditorUtility.SetDirty(sounds);

        // Falling piece + ghost.
        var game = Get<TetrimonoBehaviour>();
        var monos = Enumerable.Range(1, 4).Select(i => Find("Monos" + i).GetComponent<PositionOnGrid>()).ToArray();
        var ghosts = new PositionOnGrid[4];
        for (int i = 0; i < 4; i++)
        {
            var monoRenderer = monos[i].GetComponent<SpriteRenderer>();
            monoRenderer.sortingOrder = 3;
            var ghostGo = all.FirstOrDefault(g => g.name == "Ghost" + (i + 1));
            if (ghostGo == null)
            {
                ghostGo = Object.Instantiate(monos[i].gameObject, monos[i].transform.parent);
                ghostGo.name = "Ghost" + (i + 1);
            }
            var ghostRenderer = ghostGo.GetComponent<SpriteRenderer>();
            ghostRenderer.sortingOrder = 2;
            ghostRenderer.color = new Color(1f, 1f, 1f, 0.28f);
            ghosts[i] = ghostGo.GetComponent<PositionOnGrid>();
        }
        game.monos = monos;
        game.ghosts = ghosts;

        var moves = Get<MoveTetrimonos>();
        moves.behaviour = game;
        EditorUtility.SetDirty(moves);

        // Camera juice.
        var camera = Find("Main Camera");
        var feel = camera.GetComponent<GameFeel>();
        if (feel == null) feel = camera.AddComponent<GameFeel>();
        feel.rowFlashes = Enumerable.Range(1, 4).Select(i => Find($"Effects ({i})").GetComponent<Effects>()).ToArray();
        feel.lineBurst = MakeBurst("LineBurst", all, new Color(1f, 0.93f, 0.7f), new Color(1f, 0.6f, 0.2f),
            ParticleSystemShapeType.Box, new Vector3(GameGrid.Width, 0.6f, 0f), 3f, 10f, 0.5f, 0.9f, 0.2f, 0.45f);
        feel.holeBurst = MakeBurst("HoleBurst", all, new Color(1f, 0.35f, 0.35f), new Color(0.6f, 0.05f, 0.1f),
            ParticleSystemShapeType.Sphere, new Vector3(0.4f, 0.4f, 0.4f), 1f, 4f, 0.35f, 0.7f, 0.12f, 0.28f);
        EditorUtility.SetDirty(feel);
        game.feel = feel;

        // UI.
        var canvas = Find("Canvas").transform;
        var form = Get<Form>();
        form.holdPreview = MakeHoldPreview(all, canvas);
        EditorUtility.SetDirty(form);

        var hud = MakeHud(all, canvas);
        game.hud = hud;
        EditorUtility.SetDirty(game);

        var controls = Get<Controls>();
        controls.hud = hud;
        controls.quitButton = Find("Quit");
        EditorUtility.SetDirty(controls);
        var restart = hud.gameOverPanel.GetComponentInChildren<Button>(true);
        restart.onClick = new Button.ButtonClickedEvent();
        UnityEventTools.AddPersistentListener(restart.onClick, new UnityAction(controls.Restart));

        UpdateInstructions(all, canvas);

        // The game owns the keyboard. With UI navigation on, a toggle that
        // was clicked stays selected: arrow keys then move the selection and
        // Space/Enter "submit" it, flipping Music/Pause/Full Screen mid-game.
        var eventSystem = Get<EventSystem>();
        eventSystem.sendNavigationEvents = false;
        EditorUtility.SetDirty(eventSystem);
        foreach (var selectable in Object.FindObjectsByType<Selectable>(FindObjectsInactive.Include))
        {
            selectable.navigation = new Navigation { mode = Navigation.Mode.None };
            EditorUtility.SetDirty(selectable);
        }

        PlayerSettings.productName = "ZKTris Unforgivable";

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[SceneUpgrade] Level0 upgraded");
    }

    static ParticleSystem MakeBurst(string name, System.Collections.Generic.List<GameObject> all, Color a, Color b,
        ParticleSystemShapeType shapeType, Vector3 shapeScale, float minSpeed, float maxSpeed,
        float minLife, float maxLife, float minSize, float maxSize)
    {
        var go = all.FirstOrDefault(g => g.name == name) ?? new GameObject(name);
        var ps = go.GetComponent<ParticleSystem>();
        if (ps == null) ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 1f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(minLife, maxLife);
        main.startSpeed = new ParticleSystem.MinMaxCurve(minSpeed, maxSpeed);
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startColor = new ParticleSystem.MinMaxGradient(a, b);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI);
        main.gravityModifier = 1.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 1000;

        var emission = ps.emission;
        emission.rateOverTime = 0f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = shapeType;
        shape.scale = shapeScale;
        if (shapeType == ParticleSystemShapeType.Sphere) shape.radius = shapeScale.x;

        var fade = new Gradient();
        fade.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
        var color = ps.colorOverLifetime;
        color.enabled = true;
        color.color = fade;

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));

        // Square particles: they match the blocks.
        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        renderer.sortingOrder = 6;
        return ps;
    }

    static Image MakeHoldPreview(System.Collections.Generic.List<GameObject> all, Transform canvas)
    {
        var existing = all.FirstOrDefault(g => g.name == "HoldBackground");
        if (existing != null) { Object.DestroyImmediate(existing); all.RemoveAll(g => g == null); }

        var nextBackground = all.First(g => g.name == "PreviewBackground");
        var hold = Object.Instantiate(nextBackground, canvas);
        hold.name = "HoldBackground";
        var rt = (RectTransform)hold.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        // Left of the board, level with the Next box.
        rt.anchoredPosition = new Vector2(430f, -199f);
        var preview = hold.transform.Find("NextFormPreview");
        preview.name = "HoldFormPreview";
        hold.GetComponentsInChildren<Text>(true).First().text = "Hold";
        var image = preview.GetComponent<Image>();
        image.enabled = false;
        return image;
    }

    static Hud MakeHud(System.Collections.Generic.List<GameObject> all, Transform canvas)
    {
        foreach (var old in all.Where(g => g.name == "HUD" || g.name == "Overlay").ToList())
            Object.DestroyImmediate(old);
        all.RemoveAll(g => g == null);

        // Stats panel, top left.
        var hudGo = new GameObject("HUD", typeof(RectTransform));
        hudGo.transform.SetParent(canvas, false);
        var hudRt = (RectTransform)hudGo.transform;
        Stretch(hudRt);
        var hud = hudGo.AddComponent<Hud>();

        var panel = MakeImage(hudRt, "StatsPanel", PanelColor, new Vector2(0f, 1f), new Vector2(162f, -205f), new Vector2(280f, 370f));
        MakeText(panel, "ScoreLabel", "SCORE", 26, TextAnchor.UpperLeft, Dim, new Vector2(0f, 1f), new Vector2(140f, -34f), new Vector2(240f, 34f));
        hud.score = MakeText(panel, "Score", "0", 62, TextAnchor.UpperLeft, Color.white, new Vector2(0f, 1f), new Vector2(140f, -94f), new Vector2(240f, 72f), FontStyle.Bold);
        hud.best = StatRow(panel, "Best", "BEST", -170f, Color.white);
        hud.level = StatRow(panel, "Level", "LEVEL", -225f, Hud.AccentColor);
        hud.lines = StatRow(panel, "Lines", "LINES", -280f, Color.white);
        hud.holes = StatRow(panel, "Holes", "HOLES", -335f, Hud.HoleColor);

        // Overlays sit above the board, which the main canvas renders under.
        var overlayGo = new GameObject("Overlay", typeof(RectTransform));
        overlayGo.transform.SetParent(canvas, false);
        Stretch((RectTransform)overlayGo.transform);
        var overlayCanvas = overlayGo.AddComponent<Canvas>();
        overlayCanvas.overrideSorting = true;
        overlayCanvas.sortingOrder = 10;
        overlayGo.AddComponent<GraphicRaycaster>();
        var overlay = (RectTransform)overlayGo.transform;

        // Board center on the 1920x1080 reference canvas.
        var boardCenter = new Vector2(-32f, 0f);
        var boardSize = new Vector2(602f, 260f);

        hud.banner = MakeText(overlay, "Banner", "LEVEL 2", 84, TextAnchor.MiddleCenter, Hud.AccentColor, new Vector2(0.5f, 0.5f), boardCenter + new Vector2(0f, 180f), new Vector2(900f, 120f), FontStyle.Bold);
        hud.banner.gameObject.AddComponent<Outline>().effectDistance = new Vector2(3f, -3f);

        var paused = MakeImage(overlay, "PausedPanel", new Color(0f, 0f, 0f, 0.72f), new Vector2(0.5f, 0.5f), boardCenter, boardSize);
        MakeText(paused, "Title", "PAUSED", 76, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(600f, 100f), FontStyle.Bold);
        MakeText(paused, "Hint", "ENTER / P  to play", 30, TextAnchor.MiddleCenter, Dim, new Vector2(0.5f, 0.5f), new Vector2(0f, -45f), new Vector2(600f, 50f));
        hud.pausedPanel = paused.gameObject;

        var over = MakeImage(overlay, "GameOverPanel", new Color(0f, 0f, 0f, 0.82f), new Vector2(0.5f, 0.5f), boardCenter, boardSize + new Vector2(0f, 120f));
        MakeText(over, "Title", "GAME OVER", 76, TextAnchor.MiddleCenter, Hud.HoleColor, new Vector2(0.5f, 0.5f), new Vector2(0f, 110f), new Vector2(600f, 100f), FontStyle.Bold);
        hud.gameOverScore = MakeText(over, "Score", "SCORE  0", 34, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(600f, 90f));
        var button = MakeImage(over, "RestartButton", Hud.AccentColor, new Vector2(0.5f, 0.5f), new Vector2(0f, -85f), new Vector2(260f, 64f));
        button.gameObject.AddComponent<Button>().targetGraphic = button.GetComponent<Image>();
        MakeText(button, "Label", "RESTART", 32, TextAnchor.MiddleCenter, PanelColor, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(260f, 64f), FontStyle.Bold);
        MakeText(over, "Hint", "or press ENTER", 24, TextAnchor.MiddleCenter, Dim, new Vector2(0.5f, 0.5f), new Vector2(0f, -150f), new Vector2(600f, 40f));
        hud.gameOverPanel = over.gameObject;
        over.gameObject.SetActive(false);

        return hud;
    }

    static void UpdateInstructions(System.Collections.Generic.List<GameObject> all, Transform canvas)
    {
        void Label(string arrow, string text)
        {
            var label = all.First(g => g.name == arrow).GetComponentInChildren<Text>(true);
            label.text = text;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
        }
        Label("Rotate", "ROTATE");
        Label("Place", "SOFT DROP");

        var existing = all.FirstOrDefault(g => g.name == "MoreControls");
        if (existing != null) { Object.DestroyImmediate(existing); all.RemoveAll(g => g == null); }
        var more = MakeText((RectTransform)canvas, "MoreControls",
            "SPACE  hard drop     C  hold\nZ / X  rotate     P  pause", 30, TextAnchor.MiddleCenter, DarkText,
            new Vector2(0f, 0f), new Vector2(318f, 80f), new Vector2(600f, 80f));
        more.lineSpacing = 1.3f;
    }

    static Text StatRow(RectTransform panel, string name, string label, float y, Color valueColor)
    {
        MakeText(panel, name + "Label", label, 26, TextAnchor.MiddleLeft, Dim, new Vector2(0f, 1f), new Vector2(140f, y), new Vector2(240f, 48f));
        return MakeText(panel, name, "0", 36, TextAnchor.MiddleRight, valueColor, new Vector2(0f, 1f), new Vector2(140f, y), new Vector2(240f, 48f), FontStyle.Bold);
    }

    static RectTransform MakeImage(RectTransform parent, string name, Color color, Vector2 anchor, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        go.GetComponent<Image>().color = color;
        return rt;
    }

    static Text MakeText(RectTransform parent, string name, string text, int size, TextAnchor align, Color color,
        Vector2 anchor, Vector2 position, Vector2 box, FontStyle style = FontStyle.Normal)
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
        t.fontStyle = style;
        t.alignment = align;
        t.color = color;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
