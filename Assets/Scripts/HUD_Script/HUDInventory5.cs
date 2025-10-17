using UnityEngine;
using UnityEngine.UI;

public class HUDInventory5 : MonoBehaviour
{
    public static HUDInventory5 Instance { get; private set; }

    [SerializeField] private Image[] slots = new Image[5];
    [SerializeField] private Sprite emptySprite;

    int count = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        ClearAll();
    }

    public void Add(Sprite icon)
    {
        if (icon == null) return;
        if (count < slots.Length) { slots[count++].sprite = icon; }
        else
        {
            for (int i = 1; i < slots.Length; i++) slots[i - 1].sprite = slots[i].sprite;
            slots[^1].sprite = icon;
        }
    }

    public void ClearAll()
    {
        foreach (var img in slots) if (img) img.sprite = emptySprite;
        count = 0;
    }
}
