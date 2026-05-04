using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;

    [Header("Card Display Settings")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform handContainer;
    [SerializeField] private float cardSpacing = 120f;
    [SerializeField] private float startXPosition = 0f;
    [SerializeField] private float startYPosition = 0f;
    private List<GameObject> spawnedCards = new List<GameObject>();

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
        ArrangeCards();
        
        // CRÍTICO: Actualizar estado de interacción inmediatamente y con delay para asegurar sincronización
        UpdateInteractableState();
        StartCoroutine(DelayedUpdateInteractableState());
    }

    private IEnumerator DelayedUpdateInteractableState()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.1f); // Pequeño delay adicional para asegurar sincronización
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

    /// <summary>
    /// Organiza las cartas en una línea horizontal simple sin animaciones que interfieran
    /// </summary>
    private void ArrangeCards()
    {
        int cardCount = spawnedCards.Count;
        if (cardCount == 0) return;

        // Calcular posiciones simples en línea horizontal
        float startX = startXPosition;
        float baseY = startYPosition;

        for (int i = 0; i < cardCount; i++)
        {
            GameObject card = spawnedCards[i];
            if (card == null) continue;

            // CRÍTICO: Verificar si la carta está siendo arrastrada antes de moverla
            Card cardComponent = card.GetComponent<Card>();
            if (cardComponent != null && cardComponent.IsBeingDragged())
            {
                // NO mover ni actualizar la posición de cartas que están siendo arrastradas
                continue;
            }

            float x = startX + i * cardSpacing;
            float y = baseY;
            Vector2 targetPos = new Vector2(x, y);

            RectTransform cardRect = card.GetComponent<RectTransform>();
            if (cardRect != null)
            {
                // Posicionar directamente sin animaciones
                cardRect.anchoredPosition = targetPos;
                cardRect.rotation = Quaternion.identity;
            }

            // Actualizar la posición original en el componente Card para que el arrastre funcione correctamente
            if (cardComponent != null)
            {
                cardComponent.UpdateOriginalPosition(targetPos);
            }
        }
    }

    public void SetInteractable(bool interactable)
    {
        int cardsUpdated = 0;
        foreach (GameObject cardObj in spawnedCards)
        {
            if (cardObj != null)
            {
                CardDisplay display = cardObj.GetComponent<CardDisplay>();
                if (display != null)
                {
                    bool perCardInteractable = interactable;
                    if (perCardInteractable && GameManager.Instance != null && display.currentCard != null)
                    {
                        perCardInteractable = GameManager.Instance.CanAffordEnergy(display.currentCard);
                    }

                    display.SetInteractableState(perCardInteractable);
                    cardsUpdated++;

                    if (!interactable && display == GameManager.Instance?.SelectedCardDisplay)
                    {
                        display.SetSelected(false);
                    }
                }
            }
        }
        Debug.Log($"[HandManager] SetInteractable({interactable}) - {cardsUpdated} cartas actualizadas");
    }

    /// <summary>
    /// Actualiza el estado de interacción de todas las cartas basado en el estado actual del juego
    /// </summary>
    public void UpdateInteractableState()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[HandManager] GameManager.Instance es null en UpdateInteractableState");
            SetInteractable(false);
            return;
        }

        bool interactable = GameManager.Instance.currentTurn == GameManager.TurnState.PlayerTurn &&
                          GameManager.Instance.isBattleScene &&
                          !GameManager.Instance.AreInteractionsBlocked();

        Debug.Log($"[HandManager] UpdateInteractableState - Interactable: {interactable}, Turn: {GameManager.Instance.currentTurn}, Blocked: {GameManager.Instance.AreInteractionsBlocked()}, BattleScene: {GameManager.Instance.isBattleScene}");

        SetInteractable(interactable);
    }

}    