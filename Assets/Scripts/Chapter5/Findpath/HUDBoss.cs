using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class HUDBoss : MonoBehaviour
{
    [Header("References")]
    public BaseStats bossStats;      // Tham chiếu tới BaseStats của boss
    public Image fill;               // Thanh fill của máu boss
    public TMP_Text bossNameText;    // Dòng chữ hiển thị "TRÙM CUỐI"
    public Canvas canvas;            // Canvas chứa UI

    [Header("Display Settings")]
    public string bossTitle = "TRÙM CUỐI";
    public bool hideWhenDead = true; // Ẩn khi boss chết
    public bool autoHideWhenFull = false; // Ẩn khi full máu (nếu muốn)

    private float _cachedHP = -1f;

    void Awake()
    {
        if (!canvas) canvas = GetComponentInParent<Canvas>();
        if (bossNameText) bossNameText.text = bossTitle;

        // Nếu có sẵn stats → cập nhật ngay
        if (bossStats != null)
        {
            _cachedHP = bossStats.currentHP;
            UpdateHealthBar();
        }
    }

    void Update()
    {
        if (bossStats == null || fill == null) return;

        // Cập nhật thanh máu
        UpdateHealthBar();

        // Ẩn khi full máu (nếu được chọn)
        if (autoHideWhenFull && Mathf.Approximately(bossStats.currentHP, bossStats.maxHP))
        {
            if (canvas) canvas.enabled = false;
        }
        else
        {
            if (canvas) canvas.enabled = true;
        }

        // Ẩn khi chết
        if (hideWhenDead && bossStats.currentHP <= 0)
        {
            if (canvas) canvas.enabled = false;
        }
    }

    void UpdateHealthBar()
    {
        float max = Mathf.Max(1f, bossStats.maxHP);
        float t = Mathf.Clamp01(bossStats.currentHP / max);
        fill.fillAmount = t;
    }

    // Gọi từ BaseStats khi Boss chết
    public void Hide()
    {
        if (canvas) canvas.enabled = false;
    }

    // Gọi khi Boss xuất hiện
    public void Show(string customName = "")
    {
        if (!string.IsNullOrEmpty(customName))
        {
            bossTitle = customName;
            if (bossNameText) bossNameText.text = bossTitle;
        }
        if (canvas) canvas.enabled = true;
        UpdateHealthBar();
    }
}
