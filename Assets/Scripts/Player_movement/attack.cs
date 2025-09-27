using System.Collections;
using UnityEngine;

public class attack : MonoBehaviour
{
    public Animator ami;
    private movement moveScript;

    public float attackCooldown = 0.3f;   // delay nhỏ giữa các hit
    private float lastAttackTime;

    private int currentComboStep = 0;     // 0 = idle, 1 = Attack1, 2 = Attack2, 3 = Attack3
    private float comboTimer;
    public float comboResetTime = 1f;     // reset nếu combo bị ngắt

    private bool isAttacking;

    void Start()
    {
        ami = GetComponent<Animator>();
        moveScript = GetComponent<movement>();
    }

    void Update()
    {
        HandleAttack();

        // Nếu đang combo mà hết thời gian bấm tiếp → reset
        if (currentComboStep > 0 && Time.time > comboTimer)
        {
            ResetCombo();
        }
    }

    void HandleAttack()
    {
        if (moveScript != null && moveScript.isRolling) return;

        if ((Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0))
            && Time.time >= lastAttackTime + attackCooldown)
        {
            // Combo logic: click tiếp thì tăng step
            if (currentComboStep == 0)
            {
                PlayAttack("attack1");
                currentComboStep = 1;
            }
            else if (currentComboStep == 1)
            {
                PlayAttack("attack2");
                currentComboStep = 2;
            }
            else if (currentComboStep == 2)
            {
                PlayAttack("attack3");
                currentComboStep = 3;
            }
            else
            {
                // Nếu đã hết combo → reset về đòn 1
                ResetCombo();
                PlayAttack("attack1");
                currentComboStep = 1;
            }

            lastAttackTime = Time.time;
            comboTimer = Time.time + comboResetTime; // reset combo nếu không bấm tiếp
        }
    }

    void PlayAttack(string triggerName)
    {
        ami.ResetTrigger("attack1");
        ami.ResetTrigger("attack2");
        ami.ResetTrigger("attack3");

        ami.SetTrigger(triggerName);
        isAttacking = true;
    }

    public void EndAttack() // gọi trong Animation Event
    {
        isAttacking = false;
    }

    void ResetCombo()
    {
        currentComboStep = 0;
        isAttacking = false;
    }
}
