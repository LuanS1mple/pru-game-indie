using UnityEngine;

public class BossWindEffect : MonoBehaviour
{
    public GameObject windLeft;
    public GameObject windRight;
    public float windDuration = 0.5f; // Hiệu ứng tồn tại 0.5s

    // Gọi khi Boss tiếp đất
    public void PlayWindEffect()
    {
        if (windLeft != null)
        {
            windLeft.SetActive(true);
            StartCoroutine(DisableAfterTime(windLeft));
        }

        if (windRight != null)
        {
            windRight.SetActive(true);
            StartCoroutine(DisableAfterTime(windRight));
        }
    }

    private System.Collections.IEnumerator DisableAfterTime(GameObject obj)
    {
        yield return new WaitForSeconds(windDuration);
        obj.SetActive(false);
    }
}
