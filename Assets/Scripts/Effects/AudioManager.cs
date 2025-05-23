using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("----------- Audio Source -----------")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("----------- Audio Clip -----------")]
    public AudioClip background;
    public AudioClip checkpoint;
    public AudioClip collect;
    public AudioClip dead;
    public AudioClip hurt;
    public AudioClip jump;
    public AudioClip spikeFalling;
    public AudioClip bullet;
    public AudioClip dash;
    public AudioClip glideTurnOn;
    public AudioClip button;
    public AudioClip landingOnGround;

    float musicVolume;
    
    void Awake()
    {
        musicVolume = musicSource.volume;
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        musicSource.clip = background;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    
    }
}
