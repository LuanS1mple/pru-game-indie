using UnityEngine;

public class Monster2Controller : MonoBehaviour
{
    public Transform gunTip;
    public GameObject bullet;
    public float fireRate = 0.5f;
    private float nextFire = 0f;

    void Update()
    {
        AutoFire();
    }

    private void AutoFire()
    {
        if (Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;

            // ✅ Spawn đạn theo rotation hiện tại của GunTip
            GameObject newBullet = Instantiate(bullet, gunTip.position, gunTip.rotation);

            // ✅ Nếu viên đạn có Rigidbody2D, bắn theo hướng GunTip đang nhìn
            Rigidbody2D rb = newBullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                float bulletSpeed = 10f; // tùy bạn
                rb.velocity = gunTip.right * bulletSpeed;
            }
        }
    }
}
