using UnityEngine;

public class BossHitBox : MonoBehaviour
{
    //public int damage = 1; // Damage gây ra cho player
    private bool canDamage = false;

    public void EnableDamage() => canDamage = true;
    public void DisableDamage() => canDamage = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canDamage) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("Boss hit player!");

            // Chỉ được đánh 1 lần mỗi animation
            canDamage = false;
        }
    }
}
