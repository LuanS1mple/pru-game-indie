using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        Debug.Log("Đang chuyển sang màn 1...");
        SceneManager.LoadScene("Story_Board_1");
    }

    public void QuitGame()
    {
        Debug.Log("Thoát game...");

        Application.Quit(); // Thoát khi build

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Thoát trong Unity Editor
#endif
    }

    public void RetryGame()
    {
        Debug.Log("Chuyển lại màn hình intro");
        SceneManager.LoadScene("IntroScene");
    }
}
