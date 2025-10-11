using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image fill;
    [SerializeField] private TextMeshProUGUI hpText;

    private int maxHp = 100;
    private int curHp = 100;

    void Start()
    {
        UpdateBar();
    }

    public void SetHP(int hp)
    {
        curHp = Mathf.Clamp(hp, 0, maxHp);
        UpdateBar();
    }

    private void UpdateBar()
    {
        // cập nhật thanh máu
        fill.fillAmount = (float)curHp / maxHp;
        // cập nhật text số
        if (hpText != null)
        {
            hpText.text = curHp + " / " + maxHp;
        }
    }
}
