using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("---------- Audio Source ----------")]
    public  AudioSource musicSource;
    public  AudioSource SFXSource;

    [Header("---------- Audio Clip ----------")]
    public AudioClip background;
    public AudioClip monsterDeath;
    public AudioClip playerRoll;
    public AudioClip swordMovement;
    public AudioClip monsterAttack;
    public AudioClip jump;
    public AudioClip walk;
    public AudioClip portalIn;
    public AudioClip portalOut;

    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }
}
