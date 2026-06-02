// Script: GameIntroController.cs
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameIntroController : MonoBehaviour {
    public VideoPlayer videoPlayer;
    public GameObject skipPrompt;
    public float skipHoldDuration = 2f;
    public float promptTimeout = 5f;
    
    private float spaceHoldTime = 0f;
    private float promptTimer = 0f;
    private bool promptVisible = false;

    void Start() {
        skipPrompt.SetActive(false);
        videoPlayer.loopPointReached += EndReached;
    }

    void Update() {
        // Lógica para mostrar/ocultar prompt
        if (Input.GetKeyDown(KeyCode.Space)) {
            skipPrompt.SetActive(true);
            promptVisible = true;
            promptTimer = 0f;
        }

        // Temporizador para ocultar prompt
        if (promptVisible) {
            promptTimer += Time.deltaTime;
            if (promptTimer >= promptTimeout) {
                skipPrompt.SetActive(false);
                promptVisible = false;
            }
        }

        // Lógica para saltar
        if (Input.GetKey(KeyCode.Space)) {
            spaceHoldTime += Time.deltaTime;
            if (spaceHoldTime >= skipHoldDuration) {
                SkipVideo();
            }
        } else {
            spaceHoldTime = 0f;
        }
    }

    void SkipVideo() {
        SceneManager.LoadScene("2_MainMenu");
    }

    void EndReached(VideoPlayer vp) {
        SceneManager.LoadScene("2_MainMenu");
    }
}