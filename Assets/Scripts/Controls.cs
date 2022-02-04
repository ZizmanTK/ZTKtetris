using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controls : MonoBehaviour
{
    public Animator camAnim;
    public Text pauseText;
    public TetrimonoBehaviour behaviour;
    public MoveTetrimonos moves;
    public Sounds sounds;
    // Start is called before the first frame update
    void Start()
    {
        Screen.fullScreen = false;
        TogglePause();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Pause_Play()
    {

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
            sounds.musicPlaying=true;
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
        if (behaviour.gamePaused)
        {
            camAnim.SetBool("Paused", false);
            pauseText.text = "Pause";
            behaviour.StartFall();
            moves.enabled = true;
            behaviour.gamePaused = false;
        }
        else
        {
            camAnim.SetBool("Paused", true);
            pauseText.text = "Play";
            behaviour.StopFall();
            moves.enabled = false;
            behaviour.gamePaused = true;
        }
    }
}
