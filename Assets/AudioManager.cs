using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("---------- Audio Source ----------")]
    public AudioSource musicSource;
    public AudioSource SFXSource;

    [Header("---------- Audio Clip ----------")]
    public AudioClip background;
    public AudioClip monsterDeath;
    public AudioClip playerRoll;
    public AudioClip swordMovement;
    public AudioClip monsterAttack;
    public AudioClip jump;
    public AudioClip walk;
    public AudioClip skill1;
    public AudioClip skill2;
    public AudioClip hurt;
    public AudioClip swordHit;

    [Header("---------- Volume Control ----------")]
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private void Start()
    {
        musicSource.clip = background;
        musicSource.volume = musicVolume;
        SFXSource.volume = sfxVolume;
        musicSource.Play();
    }

    private void Update()
    {
        
        musicSource.volume = musicVolume;
        SFXSource.volume = sfxVolume;
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip, sfxVolume);
    }
}
