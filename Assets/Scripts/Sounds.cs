using UnityEngine;

// Music and sound effects. Effects are Kenney CC0 sounds (Assets/Audio/Sfx).
public class Sounds : MonoBehaviour
{
    // "move" plays the frequent, quiet effects; "sounds" the others.
    public AudioSource move;
    public AudioSource music;
    public AudioSource sounds;
    public AudioClip main;

    [Header("Piece")]
    public AudioClip shift;
    public AudioClip rotate;
    public AudioClip lockPiece;
    public AudioClip hardDrop;
    public AudioClip hold;

    [Header("Board")]
    public AudioClip clear;
    public AudioClip bigClear;
    public AudioClip hole;
    public AudioClip levelUp;
    public AudioClip gameOver;

    [Header("Menu")]
    public AudioClip menuMove;
    public AudioClip menuSelect;
    public AudioClip menuToggle;

    public bool soundsON = true;
    public bool musicPlaying = true;

    void Start()
    {
        music.clip = main;
        music.loop = true;
        PlayMusic();
    }

    public void PauseMusic() => music.Pause();
    public void PlayMusic() => music.Play();

    // Small pitch jitter keeps rapid repeats (sliding, locking) from grating.
    public void PlayShift() => Play(move, shift, 0.5f, 0.06f);
    public void PlayRotate() => Play(move, rotate, 0.6f, 0.04f);
    public void PlayLock() => Play(sounds, lockPiece, 0.7f, 0.05f);
    public void PlayHardDrop() => Play(sounds, hardDrop, 0.9f, 0.03f);
    public void PlayHold() => Play(move, hold, 0.7f);
    public void PlayClear(int lines) => Play(sounds, lines >= 4 ? bigClear : clear, 0.9f);
    public void PlayHole() => Play(sounds, hole, 0.8f);
    public void PlayLevelUp() => Play(sounds, levelUp, 0.8f);
    public void PlayGameOver() => Play(sounds, gameOver, 1f);
    public void PlayMenuMove() => Play(move, menuMove, 0.5f);
    public void PlayMenuSelect() => Play(sounds, menuSelect, 0.8f);
    public void PlayMenuToggle() => Play(sounds, menuToggle, 0.8f);

    void Play(AudioSource source, AudioClip clip, float volume, float pitchJitter = 0f)
    {
        if (!soundsON || clip == null) return;
        source.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
        source.PlayOneShot(clip, volume);
    }
}
