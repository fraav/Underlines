// Script: CompanyIntroController.cs
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CompanyIntroController : MonoBehaviour 
{
    public VideoPlayer videoPlayer;
    
    void Start() {
        videoPlayer.loopPointReached += EndReached;
    }

    void EndReached(VideoPlayer vp) {
        SceneManager.LoadScene("1_GameIntro");
    }
}