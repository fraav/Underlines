using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;

    [Header("Cartas (mazo principal)")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform handContainer;
    [SerializeField] private float cardSpacing = 120f;
    [SerializeField] private float startXPosition = 0f;
    [SerializeField] private float startYPosition = 0f;
    private List<GameObject> spawnedCards = new List<GameObject>();

    [Header("Carta/objeto (mismo sistema de posición manual que cartas, en vertical)")]
    [SerializeField] private GameObject objectCardPrefab;
    [SerializeField] private Transform objectHandContainer;
    [SerializeField] private float objectCardSpacing = 120f;
    [SerializeField] private float objectStartXPosition = 0f;
    [SerializeField] private float objectStartYPosition = 0f;
    private List<GameObject> spawnedObjectCards = new List<GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (!sceneName.Contains("Battle"))
        {
            Debug.LogWarning($"Destruyendo HandManager en escena incorrecta: {sceneName}");
            Destroy(gameObject);
            return;
        }

        if (objectCardPrefab == null)
            objectCardPrefab = cardPrefab;

        DisableLayoutOnObjectHandContainer();
        StartCoroutine(InitializeHand());
    }

    /// <summary>
    /// El layout group interfiere con el posicionamiento manual (igual que en la mano de cartas).
    /// </summary>
    private void DisableLayoutOnObjectHandContainer()
    {
        if (objectHandContainer == null) return;

        VerticalLayoutGroup vlg = objectHandContainer.GetComponent<VerticalLayoutGroup>();
        if (vlg != null)
            vlg.enabled = false;

        HorizontalLayoutGroup hlg = objectHandContainer.GetComponent<HorizontalLayoutGroup>();
        if (hlg != null)
            hlg.enabled = false;

        ContentSizeFitter csf = objectHandContainer.GetComponent<ContentSizeFitter>();
        if (csf != null)
            csf.enabled = false;
    }

    private IEnumerator InitializeHand()
    {
        while (GameManager.Instance == null ||
               GameManager.Instance.currentHand == null ||
               GameManager.Instance.currentObjectHand == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        if (GameManager.Instance.isBattleScene)
            RefreshAllHands();
    }

    public void RefreshHand()
    {
        ClearSpawnedList(spawnedCards);
        if (GameManager.Instance == null || GameManager.Instance.currentHand == null) return;

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

        ArrangeCardsHorizontal(spawnedCards, cardSpacing, startXPosition, startYPosition);
        UpdateInteractableState();
        StartCoroutine(DelayedUpdateInteractableState());
    }

    public void RefreshObjectHand()
    {
        if (objectHandContainer == null)
        {
            Debug.LogWarning("[HandManager] objectHandContainer no asignado.");
            return;
        }

        ClearSpawnedList(spawnedObjectCards);
        if (GameManager.Instance == null || GameManager.Instance.currentObjectHand == null) return;

        GameObject prefab = objectCardPrefab != null ? objectCardPrefab : cardPrefab;
        if (prefab == null)
        {
            Debug.LogError("[HandManager] No hay prefab para carta/objeto.");
            return;
        }

        foreach (ObjectCardData card in GameManager.Instance.currentObjectHand)
        {
            GameObject newCard = Instantiate(prefab, objectHandContainer);

            // Evitar conflicto si el prefab duplicado aún trae Card/CardDisplay
            Card legacyCard = newCard.GetComponent<Card>();
            if (legacyCard != null)
                Destroy(legacyCard);
            CardDisplay legacyDisplay = newCard.GetComponent<CardDisplay>();
            if (legacyDisplay != null)
                Destroy(legacyDisplay);

            ObjectCardDisplay display = newCard.GetComponent<ObjectCardDisplay>();
            if (display == null)
                display = newCard.AddComponent<ObjectCardDisplay>();

            if (newCard.GetComponent<ObjectCard>() == null)
                newCard.AddComponent<ObjectCard>();

            display.Initialize(card);
            spawnedObjectCards.Add(newCard);
        }

        ArrangeCardsVertical(spawnedObjectCards, objectCardSpacing, objectStartXPosition, objectStartYPosition);
        UpdateInteractableState();
    }

    public void SyncObjectCardPositions()
    {
        ArrangeCardsVertical(spawnedObjectCards, objectCardSpacing, objectStartXPosition, objectStartYPosition);
    }

    public void RefreshAllHands()
    {
        RefreshHand();
        RefreshObjectHand();
    }

    private IEnumerator DelayedUpdateInteractableState()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.1f);
        UpdateInteractableState();
    }

    private void ClearSpawnedList(List<GameObject> list)
    {
        foreach (var card in list)
        {
            if (card != null) Destroy(card);
        }
        list.Clear();
    }

    private void ArrangeCardsHorizontal(List<GameObject> cards, float spacing, float startX, float startY)
    {
        int cardCount = cards.Count;
        if (cardCount == 0) return;

        for (int i = 0; i < cardCount; i++)
        {
            GameObject card = cards[i];
            if (card == null) continue;

            Card cardComponent = card.GetComponent<Card>();
            if (cardComponent != null && cardComponent.IsBeingDragged())
                continue;

            float x = startX + i * spacing;
            float y = startY;
            ApplyCardPosition(card, new Vector2(x, y), cardComponent, null);
        }
    }

    private void ArrangeCardsVertical(List<GameObject> cards, float spacing, float startX, float startY)
    {
        int cardCount = cards.Count;
        if (cardCount == 0) return;

        for (int i = 0; i < cardCount; i++)
        {
            GameObject card = cards[i];
            if (card == null) continue;

            ObjectCard objectCard = card.GetComponent<ObjectCard>();
            if (objectCard != null && objectCard.IsBeingDragged())
                continue;

            float x = startX;
            float y = startY - i * spacing;
            ApplyCardPosition(card, new Vector2(x, y), null, objectCard);
        }
    }

    private static void ApplyCardPosition(
        GameObject card,
        Vector2 targetPos,
        Card cardComponent,
        ObjectCard objectCard)
    {
        RectTransform cardRect = card.GetComponent<RectTransform>();
        if (cardRect != null)
        {
            cardRect.anchoredPosition = targetPos;
            cardRect.localRotation = Quaternion.identity;
        }

        cardComponent?.UpdateOriginalPosition(targetPos);
        objectCard?.UpdateOriginalPosition(targetPos);
    }

    public void SetInteractable(bool interactable)
    {
        SetPanelInteractable(spawnedCards, interactable, false);
        SetPanelInteractable(spawnedObjectCards, interactable, true);
    }

    private void SetPanelInteractable(List<GameObject> cards, bool interactable, bool objectPanel)
    {
        foreach (GameObject cardObj in cards)
        {
            if (cardObj == null) continue;

            if (objectPanel)
            {
                ObjectCard objectCard = cardObj.GetComponent<ObjectCard>();
                if (objectCard != null && objectCard.IsBeingDragged())
                    continue;

                ObjectCardDisplay display = cardObj.GetComponent<ObjectCardDisplay>();
                if (display == null || display.currentCard == null) continue;

                bool perCard = interactable;
                if (perCard && GameManager.Instance != null)
                    perCard = GameManager.Instance.CanAffordObjectEnergy(display.currentCard);

                display.SetInteractableState(perCard);
            }
            else
            {
                Card cardComponent = cardObj.GetComponent<Card>();
                if (cardComponent != null && cardComponent.IsBeingDragged())
                    continue;

                CardDisplay display = cardObj.GetComponent<CardDisplay>();
                if (display == null) continue;

                bool perCard = interactable;
                if (perCard && GameManager.Instance != null && display.currentCard != null)
                    perCard = GameManager.Instance.CanAffordEnergy(display.currentCard);

                display.SetInteractableState(perCard);

                if (!interactable && display == GameManager.Instance?.SelectedCardDisplay)
                    display.SetSelected(false);
            }
        }
    }

    public void UpdateInteractableState()
    {
        if (GameManager.Instance == null)
        {
            SetInteractable(false);
            return;
        }

        bool interactable = GameManager.Instance.currentTurn == GameManager.TurnState.PlayerTurn &&
                            GameManager.Instance.isBattleScene &&
                            !GameManager.Instance.AreInteractionsBlocked();

        SetInteractable(interactable);
    }
}
