using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chapter6_HealthUI_Boss : MonoBehaviour
{
    [Header("Follow")]
    public Transform followTarget;
    public Vector2 worldOffset = new Vector2(0f, 1.0f);

    [Header("UI")]
    public Image fill;
    public Canvas canvas;

    [Header("Visibility")]
    public bool hideWhenFull = true;
    public float showSecondsAfterHit = 2f;

    [Header("Refs")]
    public BossStat bossStat;  

    // internal
    int _lastHP = -1;
    float _showTimer = 0f;
    Vector3 _initialLocalScale;

    void Awake()
    {
        if (!canvas) canvas = GetComponentInParent<Canvas>();
        if (!bossStat) bossStat = GetComponentInParent<BossStat>();
        if (!followTarget && bossStat) followTarget = bossStat.transform;
        _initialLocalScale = transform.localScale;

        if (bossStat)
        {
            _lastHP = bossStat.GetCurrentHealth();
            UpdateFillImmediate();
        }
    }

    void LateUpdate()
    {
        if (!bossStat || !followTarget || !fill || !canvas) return;

        // 1) Theo dõi vị trí
        Vector3 pos = followTarget.position + (Vector3)worldOffset;
        transform.position = pos;

        // 2) Cập nhật fill
        UpdateFillImmediate();

        // 3) Hiện/ẩn khi bị đánh
        int currentHP = bossStat.GetCurrentHealth();
        if (_lastHP != currentHP)
        {
            _lastHP = currentHP;
            _showTimer = showSecondsAfterHit;
            canvas.enabled = true;
        }

        if (hideWhenFull)
        {
            if (currentHP >= bossStat.maxHP && _showTimer <= 0f)
                canvas.enabled = false;
        }

        if (_showTimer > 0f)
        {
            _showTimer -= Time.deltaTime;
            if (_showTimer <= 0f && hideWhenFull && currentHP >= bossStat.maxHP)
                canvas.enabled = false;
        }

        // 4) Giữ hướng thanh máu
        Vector3 s = _initialLocalScale;
        s.x = Mathf.Abs(s.x);
        transform.localScale = s;
    }

    void UpdateFillImmediate()
    {
        float max = Mathf.Max(1, bossStat.maxHP);
        float t = Mathf.Clamp01((float)bossStat.GetCurrentHealth() / max);
        fill.fillAmount = t;
    }

    // Cho phép gọi thủ công khi spawn
    public void ForceShow(float seconds = 1.5f)
    {
        _showTimer = seconds;
        if (canvas) canvas.enabled = true;
    }
}
