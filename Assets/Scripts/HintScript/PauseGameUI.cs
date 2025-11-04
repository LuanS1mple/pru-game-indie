using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseGameUI : MonoBehaviour
{
    [Header("UI Panel")]
    [SerializeField] private GameObject pausePanel;  // Kéo Panel chính (gồm Resume, Quit, Main Menu)

    private bool isPaused = false;

    private void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    private void Update()
    {
        // Nhấn ESC để bật/tắt
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;                // Dừng mọi chuyển động
        isPaused = true;
        pausePanel.SetActive(true);
        AudioListener.pause = true;         // Tắt tiếng (nếu có nhạc nền)
        Debug.Log("⏸️ Game Paused");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;                // Tiếp tục
        isPaused = false;
        pausePanel.SetActive(false);
        AudioListener.pause = false;
        Debug.Log("▶️ Game Resumed");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("IntroScene");
        Application.Quit(); // Thoát khi build

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Thoát trong Unity Editor
#endif// Chỉ hoạt động khi build game
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("WorldMap"); // Đặt đúng tên scene của bạn
    }
}
