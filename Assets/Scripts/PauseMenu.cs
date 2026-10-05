using UnityEngine;
using UnityEngine.UI;

// Start / pause / game-over screen: a text menu driven by ↑↓ + Enter (←→
// flip settings) or the mouse, next to the controls list.
public class PauseMenu : MonoBehaviour
{
    public enum Mode { Start, Paused, GameOver }

    public Controls controls;
    public GameObject root;
    public Text title;
    public Text subtitle;
    public RectTransform marker;
    public PauseMenuItem resume, restart, music, sounds, fullScreen, quit;

    public Color selectedColor = Color.white;
    public Color idleColor = new Color(0.96f, 0.9f, 0.8f, 0.6f);

    PauseMenuItem[] itemCache;
    Mode mode;
    int selected;

    // Built on first use: the screen starts inactive, so Awake would only
    // run after Controls has already asked it to open.
    PauseMenuItem[] items
    {
        get
        {
            if (itemCache == null)
            {
                itemCache = new[] { resume, restart, music, sounds, fullScreen, quit };
                foreach (var item in itemCache) item.menu = this;
            }
            return itemCache;
        }
    }

    public void ShowPaused(Mode startOrPaused)
    {
        mode = startOrPaused;
        title.text = "ZKTRIS";
        subtitle.text = "UNFORGIVABLE";
        resume.label.text = mode == Mode.Start ? "PLAY" : "RESUME";
        Open();
    }

    public void ShowGameOver(int score, int best, bool newBest)
    {
        mode = Mode.GameOver;
        title.text = "GAME OVER";
        subtitle.text = newBest ? $"NEW BEST  {score:N0}" : $"SCORE {score:N0}   BEST {best:N0}";
        Open();
    }

    public void Hide() => root.SetActive(false);

    void Open()
    {
        bool over = mode == Mode.GameOver;
        resume.gameObject.SetActive(!over);
        music.gameObject.SetActive(!over);
        sounds.gameObject.SetActive(!over);
        fullScreen.gameObject.SetActive(!over);
        quit.gameObject.SetActive(controls.CanQuit);
        root.SetActive(true);
        Select(over ? restart : resume);
    }

    void Update()
    {
        if (!root.activeSelf) return;
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) Step(-1);
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) Step(1);
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) Activate(items[selected]);
        bool flip = Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow);
        if (flip && items[selected].isSetting) Activate(items[selected]);
        Refresh();
    }

    public void Select(PauseMenuItem item)
    {
        selected = System.Array.IndexOf(items, item);
        Refresh();
    }

    public void Activate(PauseMenuItem item)
    {
        if (item == resume) controls.TogglePause();
        else if (item == restart) controls.Restart();
        else if (item == music) controls.ToggleMusic();
        else if (item == sounds) controls.ToggleSounds();
        else if (item == fullScreen) controls.ToggleFullScreen();
        else if (item == quit) controls.Quit();
    }

    void Step(int direction)
    {
        for (int i = 1; i <= items.Length; i++)
        {
            int next = (selected + direction * i + items.Length * 2) % items.Length;
            if (items[next].gameObject.activeSelf)
            {
                selected = next;
                return;
            }
        }
    }

    void Refresh()
    {
        music.SetValue(controls.MusicOn);
        sounds.SetValue(controls.SoundsOn);
        fullScreen.SetValue(controls.FullScreen);
        for (int i = 0; i < items.Length; i++)
            items[i].label.color = i == selected ? selectedColor : idleColor;
        var target = (RectTransform)items[selected].transform;
        marker.position = new Vector3(marker.position.x, target.position.y, marker.position.z);
    }
}
