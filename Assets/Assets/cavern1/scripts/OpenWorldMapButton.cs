using UnityEngine;

public class OpenWorldMapButton : MonoBehaviour
{
    private void OnMouseDown() 
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.GoToWorldMap();
        }
    }
}
