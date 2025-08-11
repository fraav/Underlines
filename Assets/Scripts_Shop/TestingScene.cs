
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TestingScene : MonoBehaviour
{
    public void LoadNextScene(string nextScene)
    {
        SceneManager.LoadScene(nextScene);
    }
}