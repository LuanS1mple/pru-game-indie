using UnityEngine;
using UnityEngine.UI;

public class SimpleHUD : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform hpFill;
    public RectTransform mpFill;

    float hpMaxWidth;
    float mpMaxWidth;

    void Start()
    {
        if (hpFill != null) hpMaxWidth = hpFill.sizeDelta.x;
        if (mpFill != null) mpMaxWidth = mpFill.sizeDelta.x;

        SetHP(50, 100);  // nửa thanh máu //test
        SetMP(20, 50);   // nửa thanh mana
    }

    public void SetHP(float current, float max)
    {
        if (max <= 0 || hpFill == null) return;
        float t = Mathf.Clamp01(current / max);
        var size = hpFill.sizeDelta;
        size.x = hpMaxWidth * t;
        hpFill.sizeDelta = size;
    }

    public void SetMP(float current, float max)
    {
        if (max <= 0 || mpFill == null) return;
        float t = Mathf.Clamp01(current / max);
        var size = mpFill.sizeDelta;
        size.x = mpMaxWidth * t;
        mpFill.sizeDelta = size;
    }

}

