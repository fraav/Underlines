using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VideoPlayerManager : MonoBehaviour
{
    public static VideoPlayerManager Instance { get; private set; }

    [Header("Configuración")]
    [SerializeField] private VideoClip videoClip;
    [SerializeField] private RawImage videoDisplay;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private bool preloadVideo = true;

    private VideoPlayer videoPlayer;
    private RenderTexture renderTexture;
    private bool isPrepared = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupVideoSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void SetupVideoSystem()
    {
        // Crear RenderTexture dinámicamente
        renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        videoDisplay.texture = renderTexture;
        videoDisplay.raycastTarget = false;

        // Configurar VideoPlayer
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.clip = videoClip;
        videoPlayer.isLooping = true;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);

        // Precarga del video
        if (preloadVideo)
        {
            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.Prepare();
            Debug.Log("Iniciando precarga del video...");
        }
        else
        {
            isPrepared = true;
            PlayVideo();
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        isPrepared = true;
        Debug.Log("Video precargado correctamente");
        PlayVideo();
    }

    void PlayVideo()
    {
        if (!isPrepared) return;
        
        videoPlayer.time = 0;
        videoPlayer.Play();
        Debug.Log("Reproduciendo video desde el inicio");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isPrepared) 
        {
            PlayVideo();
        }
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.prepareCompleted -= OnVideoPrepared;
        
        if (renderTexture != null)
            renderTexture.Release();

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}