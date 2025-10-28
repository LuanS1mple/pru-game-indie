using UnityEngine;

public class HintTriggerOnBossDeath : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MinibossBehaviour boss; // boss map1
    [SerializeField] private EnemyStats_IDamageable enemy; // boss map2
    [SerializeField] private BossStatus boss3; // boss map3
    [SerializeField] private MiniBossHealth boss4; // boss map3
    [SerializeField] private BossStat boss5;
    [SerializeField] private SuggestionHint hint;

    private bool hasShown = false;

    void Update()
    {
        if (hasShown || hint == null)
            return;

        bool isDead = false;

        if (boss != null)
            isDead = boss.GetCurrentHealth() <= 0;
        else if (enemy != null)
            isDead = enemy.IsDead;
        else if (boss3 != null)
            isDead = boss3.GetCurrentHealth() <= 0;
        else if( boss4 != null)
                isDead = boss4.GetCurrentHealth() <= 0;
        else if(boss5 != null)
            isDead = boss5.GetCurrentHealth() <= 0;
        if (isDead)
        {
            ShowHint();
            hasShown = true;
        }
    }

    private void ShowHint()
    {
        hint.gameObject.SetActive(true);
        Debug.Log("💡 Hint đã xuất hiện sau khi boss hoặc enemy chết!");
    }
}
