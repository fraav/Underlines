using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;

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
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(InitializeAfterGM());
    }

    IEnumerator InitializeAfterGM()
    {
        int maxAttempts = 10;
        int attempts = 0;

        while (GameManager.Instance == null && attempts < maxAttempts)
        {
            yield return new WaitForSeconds(0.1f);
            attempts++;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager no se inicializó después de 1 segundo!");
        }
        else
        {
            RefreshHand();
        }
    }

    public void RefreshHand()
    {
        foreach (var card in spawnedCards)
        {
            if (card != null)
            {
                Destroy(card);
            }
        }
        spawnedCards.Clear();

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance es nulo en RefreshHand!");
            return;
        }

        if (GameManager.Instance.currentHand == null)
        {
            Debug.LogError("currentHand es nulo en RefreshHand!");
            return;
        }

        foreach (CardData card in GameManager.Instance.currentHand)
        {
            if (card == null)
            {
                Debug.LogWarning("Se encontró carta nula en la mano. Saltando...");
                continue;
            }

            if (cardPrefab == null)
            {
                Debug.LogError("CardPrefab no asignado en HandManager!");
                return;
            }

            if (handContainer == null)
            {
                Debug.LogError("HandContainer no asignado en HandManager!");
                return;
            }

            GameObject newCard = Instantiate(cardPrefab, handContainer);
            newCard.transform.localPosition = Vector3.zero;
            newCard.transform.localRotation = Quaternion.identity;

            CardDisplay display = newCard.GetComponent<CardDisplay>();
            if (display != null)
            {
                display.Initialize(card);
                spawnedCards.Add(newCard);
            }
            else
            {
                Debug.LogError("CardDisplay no encontrado en el prefab de carta");
                Destroy(newCard);
            }
        }

        StartCoroutine(ArrangeCardsInFan());
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
            
            // Calcular posición en arco parabólico
            float t = i / (float)(cardCount - 1);
            float x = startX + i * cardSpacing;
            
            // Altura basada en función parabólica
            float y = verticalOffset + maxArcHeight * (1f - Mathf.Pow(2f * t - 1f, 2));
            
            // Rotación basada en posición
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
}