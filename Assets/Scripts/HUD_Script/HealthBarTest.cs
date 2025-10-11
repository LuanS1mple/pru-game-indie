using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarTest : MonoBehaviour
{
    [Header("UI References")]
    public Image fillImage;                 // phần Fill (đỏ)
    public TextMeshProUGUI hpText;          // text hiển thị "100/100"

    [Header("HP Settings")]
    public float maxHP = 100f;
    private float currentHP;

    void Start()
    {
        currentHP = maxHP;
        UpdateHealthUI();
    }

    void Update()
    {
        // Giảm máu khi nhấn phím Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(10);
        }

        // Hồi máu khi nhấn phím H
        if (Input.GetKeyDown(KeyCode.H))
        {
            Heal(10);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHP = Mathf.Clamp(currentHP - amount, 0, maxHP);
        UpdateHealthUI();
    }

    public void Heal(float amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (fillImage != null)
            fillImage.fillAmount = currentHP / maxHP;

        if (hpText != null)
            hpText.text = $"{currentHP:0}/{maxHP:0}";
    }
}

