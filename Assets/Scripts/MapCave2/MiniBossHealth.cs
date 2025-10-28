using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MiniBossHealth : MonoBehaviour
{
    [Header("References")]
    public MiniBossStats stats; // ⚡ Gắn script stats vào đây
    public Animator animatorObject;
    private AudioManager audioManager;

    [Header("Events")]
    public UnityEvent OnMiniBossDied;

    [Header("Animation Triggers")]
    public string hurtTrigger = "Hurt";
    public string deadTrigger = "Dead";
    public string idleState = "IdleMiniBoss";

    [Header("Delays & VFX")]
    public float deathDelay = 1f;
    public float hurtRecoverDelay = 0.6f;
    public GameObject explosionVFX;

    private Animator anim;
    private bool isDead = false;
    private bool isInvulnerable = false;

    void Start()
    {
        if (stats == null)
        {
            stats = GetComponent<MiniBossStats>();
            if (stats == null)
            {
                Debug.LogError("[MiniBossHealth] ❌ Không tìm thấy MiniBossStats!");
                enabled = false;
                return;
            }
        }

        anim = animatorObject != null ? animatorObject : GetComponentInChildren<Animator>();
        if (anim == null)
            Debug.LogError($"[MiniBossHealth] ❌ Không tìm thấy Animator trong {name}!");

        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj)
            audioManager = audioObj.GetComponent<AudioManager>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvulnerable) return;

        stats.TakeDamage(damage);
        Debug.Log($"[MiniBossHealth] 💥 HP: {stats.currentHP}/{stats.maxHP}");

        if (!stats.IsDead)
            StartCoroutine(HurtAndRecover());
        else
            Die();
    }

    private IEnumerator HurtAndRecover()
    {
        isInvulnerable = true;
        PlayHurtAnimation();
        yield return new WaitForSeconds(hurtRecoverDelay);

        if (!isDead && anim)
        {
            anim.Play(idleState);
            Debug.Log("[MiniBossHealth] ↩ Quay lại trạng thái IdleMiniBoss.");
        }

        isInvulnerable = false;
    }

    private void PlayHurtAnimation()
    {
        if (anim && anim.HasParameterOfType(hurtTrigger, AnimatorControllerParameterType.Trigger))
            anim.SetTrigger(hurtTrigger);
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        OnMiniBossDied?.Invoke();

        if (anim && anim.HasParameterOfType(deadTrigger, AnimatorControllerParameterType.Trigger))
            anim.SetTrigger(deadTrigger);

        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        if (explosionVFX)
            Instantiate(explosionVFX, transform.position, Quaternion.identity);

        if (audioManager)
            audioManager.PlaySFX(audioManager.monsterDeath);

        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(deathDelay);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead || isInvulnerable) return;
        if (!collision.CompareTag("TestAttack")) return;

        PlayerCombat playerCombat = collision.GetComponentInParent<PlayerCombat>();
        if (playerCombat != null && playerCombat.playerStats != null)
        {
            int damage = (int)playerCombat.playerStats.attack;
            TakeDamage(damage);
        }
    }
}
