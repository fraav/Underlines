using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;

    [Header("Card Display Settings")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform handContainer;
    [SerializeField] private float cardSpacing = 150f;
    [SerializeField] private float verticalOffset = -100f;
    [SerializeField] private float maxArcHeight = 100f;
    [SerializeField] private float moveDuration = 0.3f;
    [SerializeField] private float fanAngle = 30f;

    private List<GameObject> spawnedCards = new List<GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        StartCoroutine(InitializeAfterGameManager());
    }

    private IEnumerator InitializeAfterGameManager()
    {
        while (GameManager.Instance == null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        RefreshHand();
    }

    public void RefreshHand()
    {
        ClearExistingCards();
        if (GameManager.Instance == null || GameManager.Instance.currentHand == null) return;
        CreateNewCards();
        StartCoroutine(ArrangeCardsInFan());
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
            else
            {
                Destroy(newCard);
            }
        }
    }

    private IEnumerator ArrangeCardsInFan()
    {
        yield return new WaitForEndOfFrame();

        int cardCount = spawnedCards.Count;
        if (cardCount == 0) yield break;

        float totalWidth = cardSpacing * (cardCount - 1);
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cardCount; i++)
        {
            GameObject card = spawnedCards[i];
            if (card == null) continue;

            float t = cardCount > 1 ? i / (float)(cardCount - 1) : 0.5f;
            float x = startX + i * cardSpacing;
            float y = verticalOffset + maxArcHeight * (1f - Mathf.Pow(2f * t - 1f, 2));
            float rotation = Mathf.Lerp(-fanAngle, fanAngle, t);

            CardDisplay display = card.GetComponent<CardDisplay>();
            if (display != null)
            {
                display.MoveToFanPosition(
                    new Vector3(x, y, 0),
                    Quaternion.Euler(0, 0, rotation),
                    moveDuration
                );
            }
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

                    if (!interactable && display == GameManager.Instance?.selectedCardDisplay)
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
                          GameManager.Instance.currentTurn == GameManager.TurnState.PlayerTurn;
        SetInteractable(interactable);
    }
}