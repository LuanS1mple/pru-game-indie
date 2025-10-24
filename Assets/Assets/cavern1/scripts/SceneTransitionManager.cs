using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;
    private string lastSceneName;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ lại khi chuyển scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GoToWorldMap()
    {
        // Lưu lại scene hiện tại trước khi chuyển
        lastSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene("WorldMap");
    }

    public void ReturnToLastScene()
    {
        if (!string.IsNullOrEmpty(lastSceneName))
        {
            SceneManager.LoadScene(lastSceneName);
        }
        else
        {
            Debug.LogWarning("Không có scene trước đó để quay lại!");
        }
    }
}
