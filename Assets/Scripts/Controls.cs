using UnityEngine;

// Game state switches (pause, restart) and settings (music, sounds, full
// screen). The pause screen (PauseMenu) is the UI for all of them.
public class Controls : MonoBehaviour
{
    public Animator camAnim;
    public TetrimonoBehaviour behaviour;
    public MoveTetrimonos moves;
    public Sounds sounds;
    public PauseMenu menu;

    bool started;

    public bool MusicOn => sounds.musicPlaying;
    public bool SoundsOn => sounds.soundsON;
    public bool FullScreen => Screen.fullScreen;
    public bool CanQuit => Application.platform != RuntimePlatform.WebGLPlayer;

    void Start()
    {
        Screen.fullScreen = false;
        SetPaused(true);
    }

    void Update()
    {
        if (behaviour.IsGameOver)
        {
            if (Input.GetKeyDown(KeyCode.R)) Restart();
            return;
        }
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void TogglePause()
    {
        if (behaviour.IsGameOver) return;
        SetPaused(!behaviour.gamePaused);
    }

    public void Restart()
    {
        behaviour.NewGame();
        SetPaused(false);
    }

    public void ToggleFullScreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    public void ToggleMusic()
    {
        if (sounds.musicPlaying) sounds.PauseMusic();
        else sounds.PlayMusic();
        sounds.musicPlaying = !sounds.musicPlaying;
    }

    public void ToggleSounds()
    {
        sounds.soundsON = !sounds.soundsON;
    }

    public void Quit()
    {
        Application.Quit();
    }

    void SetPaused(bool paused)
    {
        camAnim.SetBool("Paused", paused);
        moves.enabled = !paused;
        behaviour.gamePaused = paused;
        if (paused) menu.ShowPaused(started ? PauseMenu.Mode.Paused : PauseMenu.Mode.Start);
        else menu.Hide();
        started |= !paused;
    }
}
