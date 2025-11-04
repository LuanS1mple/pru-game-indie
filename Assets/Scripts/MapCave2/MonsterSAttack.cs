using System.Collections;
using UnityEngine;

public class MonsterSAttack : MonoBehaviour
{
    private AudioManager audioManager;

    [Header("Attack Settings")]
    public float attackCooldown = 2f;
    private float nextAttackTime;
    private bool isAttacking;

    [Header("Attack Range Settings")]
    public float stopDistance = 1.2f;

    [Header("Animator Settings")]
    public GameObject animatorObject;
    private Animator animator;
    public string attackTrigger = "Attack";
    public string speedParam = "Speed";

    [Header("Player Target")]
    public Transform player;

    [Header("Attack Collider")]
    public PolygonCollider2D attackCollider;

    void Start()
    {
        if (animatorObject != null)
        {
            animator = animatorObject.GetComponent<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError("❌ [MonsterSAttack] Animator null! Hãy gán animatorObject đúng trong Inspector.");
        }
        else
        {
            Debug.Log("✅ [MonsterSAttack] Animator đã được gán: " + animatorObject.name);
            CheckTriggerExists(attackTrigger);
        }

        audioManager = GameObject.FindGameObjectWithTag("Audio")?.GetComponent<AudioManager>();
        DisableCollider();
    }

    void Update()
    {
        if (player == null)
        {
            SetSpeedParam(0);
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);

        // 🔹 Nếu đủ gần thì tấn công
        if (dist <= stopDistance && !isAttacking && Time.time >= nextAttackTime)
        {
            StartCoroutine(AttackRoutine());
            nextAttackTime = Time.time + attackCooldown;
        }

        // 🔹 Cập nhật tốc độ animation (để AIPath có thể điều khiển riêng)
        if (animator != null)
        {
            animator.SetFloat(speedParam, isAttacking ? 0 : 1);
        }
    }

    public void StartAttack(Transform target)
    {
        player = target;
        nextAttackTime = Time.time;
        Debug.Log("👀 [MonsterSAttack] Player detected.");
    }

    public void StopAttack()
    {
        player = null;
        SetSpeedParam(0);
        Debug.Log("👋 [MonsterSAttack] Player left.");
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        SetSpeedParam(0);

        yield return new WaitForSeconds(0.05f);
        TriggerAttack();

        yield return new WaitForSeconds(0.9f);
        DisableCollider();
        isAttacking = false;

        if (animator != null)
        {
            var state = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"🏁 [MonsterSAttack] State sau attack: {state.shortNameHash}");
        }
    }

    private void TriggerAttack()
    {
        if (animator == null)
        {
            Debug.LogError("❌ [MonsterSAttack] Animator null khi set trigger!");
            return;
        }

        Debug.Log($"🕹 [MonsterSAttack] SetTrigger({attackTrigger})");
        animator.SetTrigger(attackTrigger);

        if (audioManager != null)
            audioManager.PlaySFX(audioManager.monsterAttack);

        EnableColliderDelayed(0.15f);
        StartCoroutine(CheckAnimationStateNextFrame());
    }

    IEnumerator CheckAnimationStateNextFrame()
    {
        yield return null;
        if (animator != null)
        {
            var state = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"ℹ️ [Animator Debug] State sau trigger Attack → {state.shortNameHash}");
        }
    }

    void EnableColliderDelayed(float delay)
    {
        StartCoroutine(EnableColliderCO(delay));
    }

    IEnumerator EnableColliderCO(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (attackCollider != null)
        {
            attackCollider.enabled = true;
            Debug.Log($"✅ [MonsterSAttack] Bật collider: {attackCollider.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("⚠️ [MonsterSAttack] attackCollider null!");
        }
    }

    void DisableCollider()
    {
        if (attackCollider != null) attackCollider.enabled = false;
    }

    private void SetSpeedParam(float speed)
    {
        if (animator != null)
            animator.SetFloat(speedParam, speed);
    }

    private void CheckTriggerExists(string triggerName)
    {
        if (animator == null) return;
        bool exists = false;
        foreach (var param in animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger && param.name == triggerName)
            {
                exists = true;
                break;
            }
        }
        if (!exists)
            Debug.LogError($"❌ [MonsterSAttack] Trigger '{triggerName}' không tồn tại trong Animator!");
        else
            Debug.Log($"✅ [MonsterSAttack] Trigger '{triggerName}' OK.");
    }
}
