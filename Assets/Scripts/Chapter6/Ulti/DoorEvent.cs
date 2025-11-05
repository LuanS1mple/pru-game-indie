using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorEvent : MonoBehaviour
{
    public AudioClip BossSound;
    public AudioManager AudioManager;
    [SerializeField] private LayerMask playerLayer;

    private void OnTriggerStay2D(Collider2D collision)
    {
        // chỉ xét nếu collider thuộc playerLayer
        if (((1 << collision.gameObject.layer) & playerLayer) == 0) return;
        AudioManager.musicSource.clip = BossSound;
        AudioManager.musicSource.Play();
    }
}
