using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyLifeCycle : MonoBehaviour
{
    // Start is called before the first frame update
    private Collider2D keyCollider;

    void Start()
    {
        keyCollider = GetComponent<Collider2D>();
        if (keyCollider != null)
        {
            keyCollider.enabled = false; // Tắt collider khi mới spawn
            StartCoroutine(EnableColliderAfterDelay(2f)); // Bật lại sau 2 giây
        }
    }

    IEnumerator EnableColliderAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        keyCollider.enabled = true;
    }
}
