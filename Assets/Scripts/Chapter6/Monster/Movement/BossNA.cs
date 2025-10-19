using UnityEngine;

public class BossNA : StateMachineBehaviour
{
    private float attackRadius = 4f;      // Bán kính vùng tấn công
    public int damage = 20;              // Sát thương
    public LayerMask playerLayer;        // Layer của player
    private bool hasDealtDamage = false; // Đảm bảo chỉ đánh 1 lần / animation

    override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Khi animation đạt khoảng giữa nhát chém (ví dụ 0.4 - 0.6)
        if (stateInfo.normalizedTime > 0.4f &&  !hasDealtDamage)
        {
            // Vị trí trung tâm vùng tấn công (trước mặt boss)
            Vector2 attackCenter = animator.transform.position + animator.transform.right * 1.5f;

            // Quét các collider trong vùng tấn công
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackCenter, attackRadius, playerLayer);

            foreach (Collider2D hit in hits)
            {                if (hit.CompareTag("Player"))
                {
                        Debug.LogWarning("Boss chém trúng player!");
                }
            }

            hasDealtDamage = true; // chỉ đánh 1 lần trong animation
        }
    }

    // Reset lại khi vào animation mới
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hasDealtDamage = false;
    }

    // Debug vùng tấn công trong Scene view
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("Boss kết thúc nhát chém");
    }
}
