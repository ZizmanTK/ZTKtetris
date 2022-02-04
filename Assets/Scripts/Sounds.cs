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
    public bool soundsON = true;
    public bool musicPlaying = true;

    // Start is called before the first frame update
    void Start()
    {
        music.clip = main;
        PlayMusic();
    }

    // Update is called once per frame
    void Update()
    {
        
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
        if (soundsON)  sounds.PlayOneShot(rowFill);
    }

    public void PlayError()
    {
        if (soundsON) move.PlayOneShot(error);
    }

    public void PlayMove()
    {
        if (soundsON) move.PlayOneShot(place);
    }

}
