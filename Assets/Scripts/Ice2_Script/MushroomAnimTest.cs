using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MushroomAnimTest : MonoBehaviour
{
    Animator a; int dir = 1;
    void Awake() { a = GetComponent<Animator>(); }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        { // Idle <-> Run
            float s = a.GetFloat("Speed"); a.SetFloat("Speed", s > 0.1f ? 0f : 1f);
            dir *= -1; var sc = transform.localScale; sc.x = Mathf.Abs(sc.x) * dir; transform.localScale = sc;
        }
        if (Input.GetKeyDown(KeyCode.J)) a.SetTrigger("Attack1");
        if (Input.GetKeyDown(KeyCode.K)) a.SetTrigger("Attack2");
        if (Input.GetKeyDown(KeyCode.U)) a.SetTrigger("Hit");
        if (Input.GetKeyDown(KeyCode.L)) a.SetBool("Death", true);
    }
}
