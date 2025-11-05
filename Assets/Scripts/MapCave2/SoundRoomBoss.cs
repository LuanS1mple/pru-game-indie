using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SoundRoomBoss : MonoBehaviour
{
    [Header("🎵 Cài đặt âm thanh")]
    public AudioClip bossMusic;           // Nhạc boss mới
    public AudioManager audioManager;     // Tham chiếu đến AudioManager

    [Header("⚙️ Cài đặt Trigger")]
    [SerializeField] private LayerMask playerLayer; // Layer của Player

    private bool hasPlayed = false;       // Đảm bảo chỉ phát 1 lần

    private void Awake()
    {
        // Đảm bảo collider là trigger
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Chỉ phản ứng nếu va chạm với player
        if (((1 << other.gameObject.layer) & playerLayer) == 0) return;
        if (audioManager == null || bossMusic == null) return;
        if (hasPlayed) return; // chỉ cho phép phát 1 lần duy nhất

        hasPlayed = true;

        // Đổi nhạc
        audioManager.musicSource.Stop();
        audioManager.musicSource.clip = bossMusic;
        audioManager.musicSource.Play();

        Debug.Log($"🎵 [SoundRoomBoss] Phát nhạc boss: {bossMusic.name}");
    }
}
