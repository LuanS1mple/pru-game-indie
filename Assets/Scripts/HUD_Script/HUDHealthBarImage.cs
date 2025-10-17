// HUDHealthBarImage.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDHealthBarImage : MonoBehaviour
{
    [SerializeField] private PlayerStats player;      // kéo Player vào đây
    [SerializeField] private Image fill;              // kéo node Fill (Image)
    [SerializeField] private TextMeshProUGUI hpText;  // kéo node HPText (optional)

    void OnEnable() { player.OnHealthChanged += Sync; }
    void OnDisable() { player.OnHealthChanged -= Sync; }
    void Start() { Sync(player.CurrentHealth, player.MaxHealth); }

    void Sync(int current, int max)
    {
        if (fill) fill.fillAmount = max > 0 ? (float)current / max : 0f;
        if (hpText) hpText.text = $"{current}/{max}";
    }
}
