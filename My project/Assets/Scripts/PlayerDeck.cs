using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerDeck : MonoBehaviour
{
    [Header("Cartas disponibles")]
    public List<Card> unlockedCards = new List<Card>();

    [Header("Prefabs y UI")]
    public GameObject cardUIPrefab;
    public Transform handParent;
    public TMP_Text nextCardText;

    private List<Card> hand = new List<Card>();
    private Card nextCard;
    private int currentTurn = 0;

    void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        currentTurn = 1;
        FillInitialHand();
        SelectNextCard();
        RefreshUI();
    }

    public void EndTurn()
    {
        currentTurn++;

        if (!hand.Contains(nextCard))
            hand.Add(nextCard);

        SelectNextCard();
        RefreshUI();
    }

    private void FillInitialHand()
    {
        hand.Clear();
        List<Card> pool = new List<Card>(unlockedCards);
        Shuffle(pool);

        int drawAmount = unlockedCards.Count - 1;
        for (int i = 0; i < drawAmount; i++)
            hand.Add(pool[i]);
    }

    private void SelectNextCard()
    {
        var options = unlockedCards.Where(c => !hand.Contains(c)).ToList();
        nextCard = options.Count > 0 ? options[Random.Range(0, options.Count)] : null;
    }

    private void RefreshUI()
    {
        // Limpiar mano
        foreach (Transform child in handParent)
            Destroy(child.gameObject);

        foreach (Card card in hand)
        {
            GameObject cardGO = Instantiate(cardUIPrefab, handParent);
            CardUI cardUI = cardGO.GetComponent<CardUI>();
            cardUI.Setup(card);

            Button btn = cardGO.GetComponent<Button>();
            btn.onClick.AddListener(() => PlayCard(cardUI));
        }

        if (nextCard != null)
            nextCardText.text = $"Siguiente carta: {nextCard.cardID}";
        else
            nextCardText.text = "Sin próxima carta";
    }

    public void PlayCard(CardUI cardUI)
    {
        Card card = cardUI.GetCard();
        if (hand.Contains(card))
        {
            hand.Remove(card);
            Debug.Log($"Jugaste carta: {card.cardID}");
            RefreshUI();
        }
    }

    private void Shuffle(List<Card> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            var temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }
}
