using UnityEngine;
using TMPro;

public class SuggestionHint : MonoBehaviour
{
    [Header("Message Settings")]
    [TextArea]
    [SerializeField] private string message = "You found a secret message from the boss...";

    [Header("UI References")]
    [SerializeField] private GameObject letterPanel;  // Kéo trực tiếp vào Inspector
    [SerializeField] private TMP_Text letterText;     // Kéo trực tiếp vào Inspector

    [Header("Pickup Filter")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string thisLayer = "PickupHint";

    private bool playerInZone = false;

    private void Start()
    {
        // Kiểm tra layer/tag
        Debug.Log($"[{gameObject.name}] Start(): Tag={tag}, Layer={LayerMask.LayerToName(gameObject.layer)}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[{gameObject.name}] OnTriggerEnter2D by {other.name} | Tag={other.tag} | Layer={LayerMask.LayerToName(other.gameObject.layer)}");

        if (other.CompareTag(playerTag) && gameObject.layer == LayerMask.NameToLayer(thisLayer))
        {
            playerInZone = true;
            Debug.Log("✅ Player entered hint zone!");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInZone = false;
            Debug.Log("❌ Player left hint zone!");
            HideLetter();
        }
    }

    private void Update()
    {
        if (playerInZone && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("📜 F pressed - showing letter");
            ShowLetter();
        }

        if (letterPanel != null && letterPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("❌ ESC pressed - hiding letter");
            HideLetter();
        }
    }

    private void ShowLetter()
    {
        if (letterPanel == null || letterText == null)
        {
            Debug.LogError("❌ LetterPanel or LetterText is not assigned!");
            return;
        }

        letterPanel.SetActive(true);
        letterText.text = message;
    }

    private void HideLetter()
    {
        if (letterPanel == null) return;
        letterPanel.SetActive(false);
    }
}
