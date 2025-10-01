using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryboardMng : MonoBehaviour
{
    public void LoadStoryBoard(string senceGame)
    {
        SceneManager.LoadScene(senceGame);
    }
}
