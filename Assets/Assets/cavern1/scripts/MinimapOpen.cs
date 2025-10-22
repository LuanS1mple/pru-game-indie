using UnityEngine;

public class MinimapOpen : MonoBehaviour
{
    public RectTransform minimapUI;
    private bool isExpanded = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            isExpanded = !isExpanded;
            minimapUI.localScale = isExpanded ? Vector3.one * 2f : Vector3.one;
            Debug.Log("press m");
        }
    }
}
