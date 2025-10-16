using UnityEngine;

public class EnemyBehavior : BaseStats
{
    private Animator anim;
    private bool isHurt = false;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public override void TakeDamage(float damage)
    {
        if (isDead) return;

        base.TakeDamage(damage);
        anim.SetTrigger("Hit");

        if (isDead)
        {
            Die();
        }
    }

    protected override void Die()
    {
        base.Die();
        anim.SetTrigger("Dead");
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
    }
}
