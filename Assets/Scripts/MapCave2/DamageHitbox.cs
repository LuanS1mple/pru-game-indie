using UnityEngine;

public class DamageHitbox : MonoBehaviour
{
    public int damage = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Gọi hàm nhận damage của Player
            Debug.Log($"{name} đánh trúng Player gây {damage} damage!");
        }
    }
}
