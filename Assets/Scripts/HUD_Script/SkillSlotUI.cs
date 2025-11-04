using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillSlotUI : MonoBehaviour
{
    [Header("UI Refs")]
    public Image icon;                   // icon kỹ năng
    public TextMeshProUGUI keyText;      // phím bấm (E, Q,...)
    public TextMeshProUGUI countText;    // số giây còn lại / "Ready"

    [Tooltip("Nếu để trống, script sẽ tự tạo 1 Image phủ để fill cooldown")]
    public Image cooldownFill;

    [Header("Options")]
    [Tooltip("Chuỗi hiển thị khi sẵn sàng (VD: \"\" hoặc \"Ready\")")]
    public string readyText = "";
    [Tooltip("Hiển thị thập phân (0.0) hay tròn lên (2,1,0)")]
    public bool showDecimals = false;
    [Tooltip("Màu phủ khi đang hồi chiêu")]
    public Color cooldownOverlayColor = new Color(0, 0, 0, 0.45f);

    void Awake()
    {
        // Đảm bảo có icon
        if (!icon)
        {
            Debug.LogWarning("[SkillSlotUI] Chưa gán 'icon'!");
        }

        // Tự tạo overlay cooldown nếu thiếu
        if (!cooldownFill && icon)
        {
            var go = new GameObject("CooldownFill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(icon.transform, false);
            cooldownFill = go.GetComponent<Image>();

            cooldownFill.raycastTarget = false;
            cooldownFill.color = cooldownOverlayColor;
            cooldownFill.type = Image.Type.Filled;
            cooldownFill.fillMethod = Image.FillMethod.Radial360;
            cooldownFill.fillOrigin = 2;                  // Top
            cooldownFill.fillClockwise = false;
            cooldownFill.fillAmount = 0f;

            var rt = cooldownFill.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Đặt overlay trên icon (nhưng text thường ở ngoài icon nên vẫn nằm trên cùng)
            cooldownFill.transform.SetAsLastSibling();
        }

        // Khởi tạo hiển thị sẵn sàng
        SetCooldown(0, 1);
    }

    /// <summary>
    /// Cập nhật HUD cooldown. current = thời gian còn lại, max = tổng thời gian hồi.
    /// fill = current/max (0 = sẵn sàng, 1 = còn đầy).
    /// </summary>
    public void SetCooldown(float current, float max)
    {
        current = Mathf.Max(0f, current);
        max = Mathf.Max(0.0001f, max);

        float remain = current;
        float fill = Mathf.Clamp01(remain / max);

        if (cooldownFill)
            cooldownFill.fillAmount = fill;

        if (countText)
        {
            if (remain <= 0.0001f)
            {
                countText.text = readyText;
            }
            else
            {
                countText.text = showDecimals ? $"{remain:0.0}" : $"{Mathf.CeilToInt(remain)}";
            }
        }
    }

    public void SetIcon(Sprite spr)
    {
        if (icon) icon.sprite = spr;
    }

    public void SetKey(string key)
    {
        if (keyText) keyText.text = key;
    }

    /// <summary>
    /// Overload tiện: truyền thẳng KeyCode.
    /// </summary>
    public void SetKey(KeyCode key)
    {
        if (keyText) keyText.text = key.ToString();
    }

    /// <summary>
    /// Cho phép đổi màu overlay động (nếu muốn hiệu ứng sáng mờ khi sẵn sàng).
    /// </summary>
    public void SetOverlayColor(Color c)
    {
        cooldownOverlayColor = c;
        if (cooldownFill) cooldownFill.color = cooldownOverlayColor;
    }
}
