using UnityEngine;
using UnityEngine.Video;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class CreditsController : MonoBehaviour
{
    [Header("Video Settings")]
    public VideoPlayer videoPlayer;
    
    [Header("Skip Settings")]
    public GameObject skipPrompt;
    public TMP_Text skipText;
    public float skipHoldDuration = 2f;
    public float promptTimeout = 5f;
    public string skipMessage = "Mantenga ESPACIO para volver al menú";
    
    private float spaceHoldTime = 0f;
    private float promptTimer = 0f;
    private bool promptVisible = false;
    private bool isSkipping = false;

    void Start()
    {
        // Configurar audio de fondo
        SetupAudio();
        
        // Configuración inicial del prompt
        InitializeSkipPrompt();
        
        // Configurar video
        SetupVideoPlayer();
    }

    void SetupAudio()
    {
        // Detener música de fondo si existe
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.backgroundMusic.Stop();
        }
    }

    void InitializeSkipPrompt()
    {
        if (skipPrompt != null)
        {
            skipPrompt.SetActive(false);
        }
        
        if (skipText != null)
        {
            skipText.text = skipMessage;
        }
    }

    void SetupVideoPlayer()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += EndReached;
            videoPlayer.Play();
        }
        else
        {
            Debug.LogError("VideoPlayer no asignado en CreditsController");
        }
    }

    void Update()
    {
        HandleSkipInput();
    }

    void HandleSkipInput()
    {
        // Mostrar prompt al presionar espacio
        if (Input.GetKeyDown(KeyCode.Space) && !promptVisible && !isSkipping)
        {
            ShowPrompt();
        }

        // Temporizador para ocultar prompt
        if (promptVisible && !isSkipping)
        {
            promptTimer += Time.deltaTime;
            if (promptTimer >= promptTimeout)
            {
                HidePrompt();
            }
        }

        // Detectar si se mantiene espacio presionado
        if (Input.GetKey(KeyCode.Space) && !isSkipping)
        {
            spaceHoldTime += Time.deltaTime;
            
            // Actualizar texto para mostrar progreso
            if (promptVisible && skipText != null)
            {
                float progress = Mathf.Clamp01(spaceHoldTime / skipHoldDuration);
                skipText.text = $"{skipMessage} ({Mathf.RoundToInt(progress * 100)}%)";
            }

            // Saltar cuando se completa el tiempo
            if (spaceHoldTime >= skipHoldDuration)
            {
                SkipToMainMenu();
            }
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            spaceHoldTime = 0f;
            // Restaurar texto original
            if (promptVisible && skipText != null)
            {
                skipText.text = skipMessage;
            }
        }
    }

    void ShowPrompt()
    {
        if (skipPrompt != null)
        {
            skipPrompt.SetActive(true);
            promptVisible = true;
            promptTimer = 0f;
            
            if (skipText != null)
            {
                skipText.text = skipMessage;
            }
        }
    }

    void HidePrompt()
    {
        if (skipPrompt != null)
        {
            skipPrompt.SetActive(false);
            promptVisible = false;
            spaceHoldTime = 0f;
        }
    }

    void SkipToMainMenu()
    {
        if (isSkipping) return;
        
        isSkipping = true;
        StartCoroutine(ReturnToMainMenu());
    }

    IEnumerator ReturnToMainMenu()
    {
        // Reanudar música de fondo si existe
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuMusic();
        }
        
        // Cargar escena principal
        yield return new WaitForEndOfFrame();
        SceneManager.LoadScene("2_MainMenu");
    }

    void EndReached(VideoPlayer vp)
    {
        if (!isSkipping)
        {
            StartCoroutine(ReturnToMainMenu());
        }
    }
}