using UnityEngine;

public class DemonHitBox : MonoBehaviour
{
    //[Header("Damage Settings")]
    //public int damage = 1;                // Lượng damage gây ra
    private bool canDamage = false;       // Chỉ gây damage khi đang tấn công

    // Bật collider (gọi trong animation event)
    public void EnableDamage()
    {
        canDamage = true;
    }

    // Tắt collider (gọi trong animation event)
    public void DisableDamage()
    {
        canDamage = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canDamage) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("DemonHitBox: Hit Player!");

            // Ngăn việc trúng nhiều lần trong cùng 1 đòn
            canDamage = false;
        }
    }
}
