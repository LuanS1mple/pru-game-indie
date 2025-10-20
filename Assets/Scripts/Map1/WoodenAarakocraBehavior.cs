using System.Collections;
using UnityEngine;
using Pathfinding;

public class WoodenAarakocraBehavior : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AIPath aiPath;
    [SerializeField] private GameObject target;
    private Animator animator;
    private BaseStats targetStats; // ✅ Stats của Player

    [Header("Ranges")]
    [SerializeField] private float detectRange = 6f;
    [SerializeField] private float attackRange = 2f;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackDuration = 1.0f;
    [SerializeField] private float attackSpeed = 1.0f;
    [SerializeField] private float damage = 15f; // ✅ Damage của quái

    private bool isAttacking = false;
    private bool canAttack = true;
    private int attackCount = 0;

    [Header("HP")]
    public float hp = 100f;
    private bool isTakingHit = false;
    private bool isDead = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (aiPath == null)
            aiPath = GetComponent<AIPath>();

        aiPath.canMove = true;
        animator.SetFloat("AttackSpeed", attackSpeed);

        if (target != null)
            targetStats = target.GetComponentInParent<BaseStats>(); // ✅ lấy BaseStats của Player
    }

    void Update()
    {
        if (isDead) return;
        animator.SetFloat("Hp", hp);

        // ☠️ Kiểm tra chết
        if (hp <= 0.01f)
        {
            StartCoroutine(DieRoutine());
            return;
        }

        if (target == null) return;

        float distance = Vector2.Distance(transform.position, target.transform.position);

        // 👁️ Xoay hướng
        if (aiPath.desiredVelocity.x >= 0.01f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (aiPath.desiredVelocity.x <= -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);

        // 🧭 Hành vi
        if (distance <= detectRange)
        {
            aiPath.destination = target.transform.position;

            if (distance <= attackRange)
            {
                aiPath.canMove = false;
                if (!isAttacking && canAttack && !isTakingHit)
                    StartCoroutine(AttackRoutine());
            }
            else aiPath.canMove = true;
        }
        else aiPath.canMove = false;
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        canAttack = false;
        aiPath.canMove = false;

        float distance = Vector2.Distance(transform.position, target.transform.position);
        if (distance > attackRange)
        {
            isAttacking = false;
            canAttack = true;
            aiPath.canMove = true;
            yield break;
        }

        // ⚔️ Combo attack
        if (attackCount < 3)
        {
            animator.SetTrigger("trig_attack");
            attackCount++;
        }
        else
        {
            animator.SetTrigger("trig_special");
            attackCount = 0;
        }

        // 🕒 Chờ đúng thời điểm chạm (ví dụ: frame 0.3s animation)
        yield return new WaitForSeconds(0.35f);
        DealDamageToPlayer();

        yield return new WaitForSeconds(attackDuration / attackSpeed);
        yield return new WaitForSeconds(attackCooldown);

        aiPath.canMove = true;
        canAttack = true;
        isAttacking = false;
    }

    // ✅ Hàm gây sát thương cho Player thật sự
    void DealDamageToPlayer()
    {
        if (isDead || targetStats == null) return;

        float distance = Vector2.Distance(transform.position, target.transform.position);
        if (distance <= attackRange)
        {
            targetStats.TakeDamage(damage);
            Debug.Log($"💥 Wooden Aarakocra gây {damage} damage cho Player!");
        }
    }

    // 💥 Bị đánh
    public void TakeDamage(float damage)
    {
        if (isTakingHit || isDead) return;

        hp -= damage;
        animator.SetFloat("Hp", hp);
        StartCoroutine(TakeHitRoutine());
    }

    IEnumerator TakeHitRoutine()
    {
        isTakingHit = true;
        animator.SetTrigger("trig_hit");
        yield return new WaitForSeconds(0.4f);
        isTakingHit = false;
    }

    IEnumerator DieRoutine()
    {
        isDead = true;
        aiPath.canMove = false;
        animator.SetTrigger("trig_die");
        Debug.Log("☠️ Wooden Aarakocra đã chết!");
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}
