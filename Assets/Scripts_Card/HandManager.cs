using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;

    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform handContainer;

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
        // Esperar a que GameManager esté completamente inicializado
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
        // Limpiar mano actual
        foreach (var card in spawnedCards)
        {
            if (card != null)
            {
                Destroy(card);
            }
        }
        spawnedCards.Clear();

        // Validar GameManager y currentHand
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

        // Crear nuevas cartas
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
    }
}