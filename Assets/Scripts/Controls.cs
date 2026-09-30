using UnityEngine;
using UnityEngine.UI;

// Menu buttons (pause, full screen, music, sounds, quit) and the keyboard
// shortcuts for pausing and restarting.
public class Controls : MonoBehaviour
{
    public Animator camAnim;
    public Text pauseText;
    public TetrimonoBehaviour behaviour;
    public MoveTetrimonos moves;
    public Sounds sounds;
    public Hud hud;
    public GameObject quitButton;

    Toggle pauseToggle;

    void Start()
    {
        pauseToggle = pauseText.GetComponentInParent<Toggle>();
        Screen.fullScreen = false;
#if UNITY_WEBGL
        // Application.Quit does nothing in a browser.
        if (quitButton != null) quitButton.SetActive(false);
#endif
        SetPaused(true);
    }

    void Update()
    {
        if (behaviour.IsGameOver)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.R))
                Restart();
            return;
        }
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            TogglePause();
    }

    public void ToggleFullScreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    public void ToggleMusic()
    {
        if (sounds.musicPlaying)
        {
            sounds.PauseMusic();
            sounds.musicPlaying = false;
        }
        else
        {
            sounds.PlayMusic();
            sounds.musicPlaying = true;
        }
    }

    public void ToggleSounds()
    {
        sounds.soundsON = !sounds.soundsON;
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void TogglePause()
    {
        if (behaviour.IsGameOver)
        {
            if (pauseToggle != null) pauseToggle.SetIsOnWithoutNotify(!behaviour.gamePaused);
            return;
        }
        SetPaused(!behaviour.gamePaused);
    }

    public void Restart()
    {
        behaviour.NewGame();
        SetPaused(false);
    }

    void SetPaused(bool paused)
    {
        camAnim.SetBool("Paused", paused);
        pauseText.text = paused ? "Play" : "Pause";
        // Keep the toggle in sync when pausing from the keyboard, without
        // firing its OnValueChanged (which would toggle again).
        if (pauseToggle != null) pauseToggle.SetIsOnWithoutNotify(!paused);
        moves.enabled = !paused;
        behaviour.gamePaused = paused;
        hud.ShowPaused(paused);
    }
}
