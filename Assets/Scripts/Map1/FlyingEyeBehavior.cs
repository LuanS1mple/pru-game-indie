using System.Collections;
using UnityEngine;
using Pathfinding;

public class FlyingEyeBehavior : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AIPath aiPath;
    [SerializeField] private GameObject target;
    private Animator animator;
    [Header("Stats")]
    [SerializeField] private float hp = 100f;
    [SerializeField] private float detectRange = 6f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackDuration = 1f;

    private bool isAttacking = false;
    private bool canAttack = true;
    private bool isTakingHit = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (aiPath == null)
            aiPath = GetComponent<AIPath>();

        aiPath.canMove = true;
        animator.SetFloat("Hp", hp);
    }

    void Update()
    {
        if (hp <= 0.01f)
        {
            Destroy(gameObject); // Chết khi hết máu
            return;
        }

        if (target == null) return;

        float distance = Vector2.Distance(transform.position, target.transform.position);

        // --- Flip hướng theo vận tốc ---
        if (aiPath.desiredVelocity.x > 0.01f)
            transform.localScale = new Vector3(1f, 1f, 1f);
        else if (aiPath.desiredVelocity.x < -0.01f)
            transform.localScale = new Vector3(-1f, 1f, 1f);

        // --- Logic hành vi ---
        if (distance <= detectRange)
        {
            aiPath.destination = target.transform.position;

            if (distance <= attackRange)
            {
                if (canAttack && !isAttacking && !isTakingHit)
                    StartCoroutine(AttackRoutine());
            }
            else
            {
                aiPath.canMove = true;
            }
        }
        else
        {
            aiPath.canMove = false;
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        canAttack = false;
        aiPath.canMove = false;

        animator.SetTrigger("attack");
        Debug.Log("⚔️ FlyingEye Attack!");

        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;

        yield return new WaitForSeconds(attackCooldown);

        aiPath.canMove = true;
        canAttack = true;
        Debug.Log("FlyingEye sẵn sàng tấn công lại!");
    }

    public void TakeDamage(float damage)
    {
        if (isTakingHit || hp <= 0.01f) return;

        hp -= damage;
        animator.SetFloat("Hp", hp);
        StartCoroutine(TakeHitRoutine());
    }

    IEnumerator TakeHitRoutine()
    {
        isTakingHit = true;
        animator.SetTrigger("takehit");

        yield return new WaitForSeconds(0.5f);

        isTakingHit = false;
    }
}
