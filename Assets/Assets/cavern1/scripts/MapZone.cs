using UnityEngine;
using UnityEngine.SceneManagement;

public class MapZone : MonoBehaviour
{
    [Header("Tên Scene muốn chuyển đến")]
    public string sceneToLoad;

    private void OnMouseDown()
    {
        // Khi click chuột trái vào object
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log("Loading scene: " + sceneToLoad);
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("Scene name is empty for " + gameObject.name);
        }
    }
}
