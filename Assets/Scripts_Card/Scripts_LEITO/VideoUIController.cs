// VideoUIController.cs
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

[RequireComponent(typeof(VideoPlayer), typeof(RawImage))]
public class VideoUIController : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    private RawImage rawImage;

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        rawImage = GetComponent<RawImage>();
        
        // Configuración inicial
        rawImage.raycastTarget = false;
        videoPlayer.playOnAwake = true;
        videoPlayer.isLooping = true;
        
        // Crear RenderTexture dinámica
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        videoPlayer.targetTexture = rt;
        rawImage.texture = rt;
        
        // Forzar reproducción desde inicio
        videoPlayer.time = 0;
        videoPlayer.Play();
    }

    void Update()
    {
        // Opcional: Pausar cuando el juego está pausado
        if (Time.timeScale < 0.1f && videoPlayer.isPlaying)
            videoPlayer.Pause();
        else if (Time.timeScale >= 0.1f && !videoPlayer.isPlaying)
            videoPlayer.Play();
    }
}