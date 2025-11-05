using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorControllerMapCave2 : MonoBehaviour
{
    public int keyCount = 0; // Biến đếm số key
    public GameObject door;  // Cửa cần mở

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Khi va chạm với vật có tag "Chapter6-Key"
        if (collision.CompareTag("Chapter6-Key"))
        {
            keyCount++;
            Debug.Log("Đã nhặt chìa khóa: " + keyCount);

            // Xóa chìa khóa sau khi nhặt
            Destroy(collision.gameObject);

            // Nếu đã đủ 3 chìa khóa
            if (keyCount >= 2 && door != null)
            {
                Collider2D doorCollider = door.GetComponent<Collider2D>();
                if (doorCollider != null)
                {
                    doorCollider.isTrigger = true; // Mở cửa
                    Debug.Log("Cửa đã mở!");
                }
            }
        }
    }
}
