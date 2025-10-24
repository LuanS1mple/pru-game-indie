using UnityEngine;

public class BackToPreviousScene : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.ReturnToLastScene();
        }
        else
        {
            Debug.LogWarning("Không tìm thấy SceneTransitionManager trong scene!");
        }
    }
}
