using UnityEngine;

public class SoundManager : MonoBehaviour
{
    
    public static SoundManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [Header("Audio Clips")]
    public AudioClip flipClip;
    public AudioClip matchClip;
    public AudioClip mismatchClip;
    public AudioClip gameOverClip;

    private AudioSource _source;

    private void Start()
    {
        _source = gameObject.AddComponent<AudioSource>();
        _source.playOnAwake = false;
    }

    // === Public API ===
    public void PlayFlip() => Play(flipClip);
    public void PlayMatch() => Play(matchClip);
    public void PlayMismatch() => Play(mismatchClip);
    public void PlayGameOver() => Play(gameOverClip);

    private void Play(AudioClip clip)
    {
        if (clip == null) return;
        _source.PlayOneShot(clip);
    }
}
