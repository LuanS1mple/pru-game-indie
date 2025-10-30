using Pathfinding;
using System.Collections;
using UnityEngine;

public class BossBehaviors : MonoBehaviour
{
    [Header("References")]
    public AIPath aiPath;
    private Animator animator;
    public GameObject target;
    public GameObject castPrefab;
    public GameObject castFrame;
    public BossStat bossStat;

    [Header("Settings")]
    public float detectionRange = 10f; // 👈 thêm khoảng phát hiện
    public float attackRange = 5f;
    public float attackCooldown = 1f;
    public float castCooldown = 5f;

    private bool canAttack = true;
    private bool canCast = true;

    [Header("States")]
    public bool IsHurt;
    public bool IsAttack;
    public bool IsCast;
    public bool IsWalk;

    void Start()
    {
        if (aiPath == null) aiPath = GetComponent<AIPath>();
        if (animator == null) animator = GetComponent<Animator>();
        if (bossStat == null) bossStat = GetComponent<BossStat>();
    }

    void Update()
    {
        if (bossStat != null && bossStat.isDead) return;
        if (IsAttack || IsCast || IsHurt) return;

        float distance = Vector2.Distance(transform.position, target.transform.position);

        // 👇 Chỉ di chuyển khi target nằm trong khoảng phát hiện
        if (distance <= detectionRange)
        {
            aiPath.destination = target.transform.position;
            aiPath.canMove = true;
            IsWalk = true;
            UpdateAnimator();

            if (distance <= attackRange)
                TryAttack();
            else
                TryCast();
        }
        else
        {
            // 👇 target quá xa, dừng lại và không đi nữa
            aiPath.canMove = false;
            IsWalk = false;
            UpdateAnimator();
        }
    }

    void TryAttack()
    {
        if (canAttack)
            StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        canAttack = false;
        IsAttack = true;
        IsWalk = false;
        aiPath.canMove = false;
        animator.SetBool("IsAttack", true);

        yield return new WaitForSeconds(1f);

        IsAttack = false;
        aiPath.canMove = true;
        animator.SetBool("IsAttack", false);

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    void TryCast()
    {
        if (canCast && Random.value <= 0.2f)
            StartCoroutine(CastRoutine());
    }

    IEnumerator CastRoutine()
    {
        canCast = false;
        IsCast = true;
        IsWalk = false;
        aiPath.canMove = false;
        animator.SetBool("IsCast", true);

        yield return new WaitForSeconds(0.8f);

        if (target != null && castPrefab != null && castFrame!=null)
        {
            if (Random.value < 0.5f && castPrefab != null)
            {
                Vector3 spawnPos = new Vector3(target.transform.position.x, -27.49f, target.transform.position.z);
                GameObject obj = Instantiate(castPrefab, spawnPos, Quaternion.identity);
                Destroy(obj, 1f);
            }
            else if (castFrame != null)
            {
                Vector3 spawnPos = new Vector3(target.transform.position.x, -16.36f, target.transform.position.z);
                GameObject obj = Instantiate(castFrame, spawnPos, Quaternion.identity);
                Destroy(obj, 4f);
            }
        }

        IsCast = false;
        aiPath.canMove = true;
        animator.SetBool("IsCast", false);

        yield return new WaitForSeconds(castCooldown);
        canCast = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (bossStat != null && bossStat.isDead) return;

        if (other.CompareTag("TestAttack"))
        {
            if (!IsAttack && !IsCast)
                StartCoroutine(HurtRoutine());
            else
                StartCoroutine(FlinchEffect());
        }
    }

    IEnumerator FlinchEffect()
    {
        if (IsHurt) yield break;

        IsHurt = true;
        animator.SetTrigger("Flinch");

        Vector3 startPos = transform.position;
        transform.position += new Vector3(0.05f, 0, 0);
        yield return new WaitForSeconds(0.1f);
        transform.position = startPos;

        IsHurt = false;
    }

    public IEnumerator HurtRoutine()
    {
        if (bossStat != null && bossStat.isDead) yield break;

        IsHurt = true;
        IsWalk = false;
        aiPath.canMove = false;
        animator.SetBool("IsHurt", true);

        yield return new WaitForSeconds(0.7f);

        IsHurt = false;
        aiPath.canMove = true;
        animator.SetBool("IsHurt", false);
    }

    void UpdateAnimator()
    {
        animator.SetBool("IsHurt", IsHurt);
        animator.SetBool("IsAttack", IsAttack);
        animator.SetBool("IsCast", IsCast);
        animator.SetBool("IsWalk", IsWalk);
    }
}
