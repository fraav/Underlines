using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;

    [Header("Card Display Settings")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform handContainer;
    [SerializeField] private float cardSpacing = 120f;
    [SerializeField] private float horizontalOffset = 200f; // Offset from right edge
    [SerializeField] private float verticalOffset = 150f; // Offset from bottom edge
    [SerializeField] private float maxArcHeight = 50f;
    [SerializeField] private float fanAngle = 15f;
    [SerializeField] private float moveDuration = 0.3f;
    [SerializeField] private float startXPosition = 0f;
    [SerializeField] private float startYPosition = 0f;
    [SerializeField] private Button confirmButton;
    public List<GameObject> spawnedCards = new List<GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Verificación mejorada para todas las escenas de batalla
        string sceneName = SceneManager.GetActiveScene().name;
        if (!sceneName.Contains("Battle"))
        {
            Debug.LogWarning($"Destruyendo HandManager en escena incorrecta: {sceneName}");
            Destroy(gameObject);
            return;
        }
        
        // Configurar botón de confirmación
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        }
        
        StartCoroutine(InitializeHand());
    }

    private IEnumerator InitializeHand()
    {
        while (GameManager.Instance == null || GameManager.Instance.currentHand == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        if (GameManager.Instance.isBattleScene) RefreshHand();
    }

    public void RefreshHand()
    {
        ClearExistingCards();
        if (GameManager.Instance == null || GameManager.Instance.currentHand == null) return;

        CreateNewCards();
        StartCoroutine(ArrangeCardsInFan(true));
        UpdateInteractableState();
    }

    private void ClearExistingCards()
    {
        foreach (var card in spawnedCards)
        {
            if (card != null) Destroy(card);
        }
        spawnedCards.Clear();
    }

    private void CreateNewCards()
    {
        foreach (CardData card in GameManager.Instance.currentHand)
        {
            GameObject newCard = Instantiate(cardPrefab, handContainer);
            CardDisplay display = newCard.GetComponent<CardDisplay>();

            if (display != null)
            {
                display.Initialize(card);
                spawnedCards.Add(newCard);
            }
            else Destroy(newCard);
        }
    }

    private IEnumerator ArrangeCardsInFan(bool repeat = false)
    {
        yield return new WaitForEndOfFrame();

        int cardCount = spawnedCards.Count;
        if (cardCount == 0) yield break;

        // Get canvas size for positioning
        Canvas canvas = handContainer.GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        // Calculate positions for bottom-right corner
        float totalWidth = cardSpacing * (cardCount - 1);
        float startX = startXPosition; // Start from right edge
        float baseY =  startYPosition;// Bottom edge

        for (int i = 0; i < cardCount; i++)
        {
            GameObject card = spawnedCards[i];
            if (card == null) continue;

            float t = cardCount > 1 ? i / (float)(cardCount - 1) : 0.5f;
            float x = startX + i * cardSpacing;
            float y = baseY + maxArcHeight * (1f - Mathf.Pow(2f * t - 1f, 2));
            float rotation = Mathf.Lerp(-fanAngle, fanAngle, t);

            CardDisplay display = card.GetComponent<CardDisplay>();
            if (display != null)
            {
                // Set position directly first
                RectTransform cardRect = card.GetComponent<RectTransform>();
                if (cardRect != null)
                {
                    cardRect.anchoredPosition = new Vector3(x, y, 0);
                    cardRect.rotation = Quaternion.Euler(0, 0, rotation);
                }
                
                display.MoveToFanPosition(
                    new Vector3(x, y, 0),
                    Quaternion.Euler(0, 0, rotation),
                    moveDuration
                );
            }
        }
        yield return null;;
        if(repeat)
        {
            ArrangeCardsInFan(false);
        }
    }

    public void SetInteractable(bool interactable)
    {
        foreach (GameObject cardObj in spawnedCards)
        {
            if (cardObj != null)
            {
                CardDisplay display = cardObj.GetComponent<CardDisplay>();
                if (display != null)
                {
                    display.SetInteractableState(interactable);

                    if (!interactable && display == GameManager.Instance?.SelectedCardDisplay)
                    {
                        display.SetSelected(false);
                    }
                }
            }
        }
    }

    private void UpdateInteractableState()
    {
        bool interactable = GameManager.Instance != null &&
                          GameManager.Instance.currentTurn == GameManager.TurnState.PlayerTurn &&
                          GameManager.Instance.isBattleScene;

        SetInteractable(interactable);
        UpdateConfirmButton();
    }
    
    private void UpdateConfirmButton()
    {
        if (confirmButton != null)
        {
            bool showButton = GameManager.Instance != null && 
                            GameManager.Instance.currentTurn == GameManager.TurnState.SelectingAssistance;
            confirmButton.gameObject.SetActive(showButton);
        }
    }
    
    private void OnConfirmButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ConfirmAssistanceSelection();
        }
    }

}    