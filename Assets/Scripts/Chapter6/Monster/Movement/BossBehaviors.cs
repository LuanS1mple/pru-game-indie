using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBehaviors : MonoBehaviour
{
    [Header("References")]
    public AIPath aiPath;
    private Animator animator;
    public GameObject target;  // người chơi
    public GameObject castPrefab; // object tạo ra khi Cast

    [Header("Settings")]
    public float attackRange = 2f;
    public float castCooldown = 5f;
    private bool canCast = true;

    [Header("States")]
    public bool IsDeath;
    public bool IsHurt;
    public bool IsAttack;
    public bool IsCast;
    public bool IsWalk;

    void Start()
    {
        if (aiPath == null) aiPath = GetComponent<AIPath>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (IsDeath || IsHurt) return; // nếu đang chết hoặc bị thương thì dừng logic

        float distance = Vector2.Distance(transform.position, target.transform.position);

        // Quái luôn hướng về người chơi
        aiPath.destination = target.transform.position;

        // Nếu còn sống và không cast thì đang đi
        IsWalk = !IsAttack && !IsCast;

        // Gán animation
        UpdateAnimator();

        // Kiểm tra hành vi theo khoảng cách
        if (distance <= attackRange)
        {
            TryAttack();
        }
        else
        {
            TryCast();
        }
    }

    void TryAttack()
    {
        if (!IsAttack)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        IsAttack = true;
        IsWalk = false;
        animator.SetBool("IsAttack", true);
        aiPath.canMove = false;

        // Giả lập thời gian tấn công
        yield return new WaitForSeconds(1f);

        IsAttack = false;
        aiPath.canMove = true;
    }

    void TryCast()
    {
        if (canCast && !IsCast)
        {
            float random = Random.value; // 0 -> 1
            if (random <= 0.2f) // 20% tỉ lệ cast
            {
                StartCoroutine(CastRoutine());
            }
        }
    }

    IEnumerator CastRoutine()
    {
        IsCast = true;
        IsWalk = false;
        canCast = false;
        aiPath.canMove = false;
        animator.SetBool("IsCast", true);

        yield return new WaitForSeconds(0.8f); // thời gian cast animation

        // Tạo object tại vị trí người chơi
        if (target != null && castPrefab != null)
        {
            GameObject obj = Instantiate(castPrefab, target.transform.position, Quaternion.identity);
            Destroy(obj, 1f); // 🟢 Hủy object sau 1 giây
        }

        // Kết thúc cast
        IsCast = false;
        aiPath.canMove = true;

        // Hồi chiêu cast
        yield return new WaitForSeconds(castCooldown);
        canCast = true;
    }


    void UpdateAnimator()
    {
        animator.SetBool("IsDeath", IsDeath);
        animator.SetBool("IsHurt", IsHurt);
        animator.SetBool("IsAttack", IsAttack);
        animator.SetBool("IsCast", IsCast);
        animator.SetBool("IsWalk", IsWalk);
    }
}
