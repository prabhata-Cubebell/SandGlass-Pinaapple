using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private AudioSource bgSource;
    private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();

        bgSource.loop = true;
        bgSource.playOnAwake = false;

        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }

   
    
    // Plays a sound effect. If `loop` is true, it will keep playing until stopped.
    public void PlaySFXMusic(AudioClip clip, bool loop = false)
    {
        if (clip == null) return;

        sfxSource.loop = loop;
        sfxSource.clip = clip;
        sfxSource.Play();
    }

    public void PlayBgMusic(AudioClip clip)
    {
        bgSource.clip = clip;
        bgSource.Play();
    }

    /// Stops any currently playing SFX (not background music).
    public void StopSFX()
    {
        sfxSource.Stop();
        sfxSource.clip = null;
        sfxSource.loop = false;
    }
}
