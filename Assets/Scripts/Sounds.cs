using UnityEngine;

public class Sounds : MonoBehaviour
{
    public AudioClip error;
    public AudioSource move;
    public AudioSource music;
    public AudioSource sounds;
    public AudioClip main;
    public AudioClip rowFill;
    public AudioClip place;
    public AudioClip hardDrop;
    public bool soundsON = true;
    public bool musicPlaying = true;

    void Start()
    {
        music.clip = main;
        music.loop = true;
        PlayMusic();
    }

    public void PauseMusic()
    {
        music.Pause();
    }

    public void PlayMusic()
    {
        music.Play();
    }

    public void PlayPlaced()
    {
        if (soundsON) sounds.PlayOneShot(place);
    }

    public void PlayRowFill()
    {
        if (soundsON) sounds.PlayOneShot(rowFill);
    }

    public void PlayError()
    {
        if (soundsON) move.PlayOneShot(error);
    }

    public void PlayMove()
    {
        if (soundsON) move.PlayOneShot(place);
    }

    public void PlayHardDrop()
    {
        if (soundsON && hardDrop != null) sounds.PlayOneShot(hardDrop, 0.6f);
    }
}
