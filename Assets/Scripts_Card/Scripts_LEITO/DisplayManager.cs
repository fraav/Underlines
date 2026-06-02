using UnityEngine;
using System.Collections;

public class DisplayManager : MonoBehaviour
{
    public static DisplayManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadDisplaySettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetFullscreen(bool isFullscreen)
    {
        StartCoroutine(ApplyFullscreen(isFullscreen));
    }

    private IEnumerator ApplyFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = false;
        yield return new WaitForEndOfFrame();
        Screen.fullScreen = isFullscreen;
        
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadDisplaySettings()
    {
        bool fullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        Screen.fullScreen = fullscreen;
    }
}