using System.Collections;
using UnityEngine;

public class attack : MonoBehaviour
{
    private AudioManager audioManager;
    public Animator ami;
    private movement moveScript;
    private BaseStats stats;

   
    private float lastAttackTime;

    private int currentComboStep = 0;     // 0 = idle, 1 = Attack1, 2 = Attack2, 3 = Attack3
    private float comboTimer;
    public float comboResetTime = 1f;     // reset nếu combo bị ngắt

    public bool isAttacking;
    private Transform attackpoint;  

    void Start()
    {
        ami = GetComponent<Animator>();
        moveScript = GetComponent<movement>();
        stats = GetComponent<BaseStats>();

        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    void Update()
    {
        if (stats != null && stats.isDead)
        {
            // Cần đảm bảo animation tấn công (nếu đang chạy) bị dừng
            isAttacking = false;
            if (moveScript != null)
                moveScript.SetCanFlip(true); // Mở khóa xoay để nhân vật không bị kẹt hướng

            return; // Thoát khỏi hàm Update, không xử lý HandleAttack() nữa
        }
        HandleAttack();
       
        // Nếu combo hết hạn → reset
        if (currentComboStep > 0 && Time.time > comboTimer)
        {
            ResetCombo();
        }

        // 🔓 Nếu không còn đang tấn công → đảm bảo cho phép xoay lại
        if (!isAttacking && moveScript != null)
            moveScript.SetCanFlip(true);
    }


    void HandleAttack()
    {
        if (moveScript != null && moveScript.isRolling) return; // không tấn công khi đang lăn
        if (Time.time < lastAttackTime + stats.attackCooldown) return;

        if (Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0))
        {
            // Nếu đang ở idle (chưa combo) → bắt đầu combo
            if (currentComboStep == 0)
            {
                PlayAttack("attack1");
                currentComboStep = 1;
            }
            // Nếu đang ở attack1 và người chơi bấm kịp trong thời gian combo → sang attack2
            else if (currentComboStep == 1 && Time.time <= comboTimer)
            {
                PlayAttack("attack2");
                currentComboStep = 2;
            }
            // Nếu đang ở attack2 → sang attack3
            else if (currentComboStep == 2 && Time.time <= comboTimer)
            {
                PlayAttack("attack3");
                currentComboStep = 3;
            }
            // Nếu đã combo xong (attack3) hoặc bấm quá chậm → reset combo và bắt đầu lại
            else
            {
                ResetCombo();
                PlayAttack("attack1");
                currentComboStep = 1;
            }

            // Thiết lập thời gian cho phép combo tiếp
            lastAttackTime = Time.time;
            comboTimer = Time.time + comboResetTime;
        }
    }

    void PlayAttack(string triggerName)
    {
        ami.ResetTrigger("attack1");
        ami.ResetTrigger("attack2");
        ami.ResetTrigger("attack3");
        ami.SetTrigger(triggerName);

        if (moveScript != null)
            moveScript.SetCanFlip(false); // ✅ đúng cách

        isAttacking = true;

        //Âm thanh kiếm chém vào không khí
        if (audioManager != null)
        {
            audioManager.PlaySFX(audioManager.swordMovement);
        }
    }


    public void EndAttack()
    {
        isAttacking = false;
        if (moveScript != null)
            moveScript.SetCanFlip(true); // ✅ mở xoay lại
    }






    void ResetCombo()
    {
        currentComboStep = 0;
        isAttacking = false;
    }


}
