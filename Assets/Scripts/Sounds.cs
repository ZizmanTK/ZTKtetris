using UnityEngine;

// Music and sound effects. Each effect has its own volume, set from the
// clip's measured loudness so nothing is piercing; tweak in the Inspector.
public class Sounds : MonoBehaviour
{
    [System.Serializable]
    public class Effect
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.3f;
        [Range(0.5f, 2f)] public float pitch = 1f;
    }

    // Levels live in each Effect, not in the mixer: the WebGL player ignores
    // AudioMixer group volumes, so the clips themselves are levelled.
    public AudioSource move;
    public AudioSource music;
    public AudioSource sounds;
    public AudioClip main;

    [Header("Piece")]
    public Effect shift;
    public Effect rotate;

    public Effect lockPiece;
    public Effect hardDrop;
    public Effect hold;

    [Header("Board")]
    public Effect clear;
    public Effect bigClear;
    public Effect hole;
    public Effect levelUp;
    public Effect gameOver;

    [Header("Menu")]
    public Effect menuMove;
    public Effect menuSelect;
    public Effect menuToggle;

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

    public void PlayShift() => Play(move, shift);
    public void PlayRotate() => Play(move, rotate);
    public void PlayLock() => Play(sounds, lockPiece);
    public void PlayHardDrop() => Play(sounds, hardDrop);
    public void PlayHold() => Play(sounds, hold);
    public void PlayClear(int lines) => Play(sounds, lines >= 4 ? bigClear : clear);
    public void PlayHole() => Play(sounds, hole);
    public void PlayLevelUp() => Play(sounds, levelUp);
    public void PlayGameOver() => Play(sounds, gameOver);
    public void PlayMenuMove() => Play(sounds, menuMove);
    public void PlayMenuSelect() => Play(sounds, menuSelect);
    public void PlayMenuToggle() => Play(sounds, menuToggle);

    void Play(AudioSource source, Effect effect)
    {
        if (!soundsON || effect == null || effect.clip == null) return;
        source.pitch = effect.pitch;
        source.PlayOneShot(effect.clip, effect.volume);
    }
}
