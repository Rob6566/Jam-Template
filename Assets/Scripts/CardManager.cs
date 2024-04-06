//Handles generating cards

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum CardSuit {hearts, diamonds, clubs, spades, all};
public enum CardRank {A, two, three, four, five, six, seven, eight, nine, ten, J, Q, K, Joker};

public class CardManager : MonoBehaviour {

    public List<CardSO> allCardSO = new List<CardSO>();
    public GameObject cardPrefab;

    public GameObject deckContainer;

    List<Card> allCards = new List<Card>();
    List<Card> deck = new List<Card>();
    List<Card> discard = new List<Card>();

    GameManager gameManager;

    public void init(GameManager newGameManager) {
        gameManager=newGameManager;
        generateInitialDeck();
    }

    public void generateInitialDeck() {
        allCards.Clear();
        foreach (CardSO cardSO in allCardSO) {
            Card newCard = createCardFromCardSO(cardSO);
            allCards.Add(newCard);
            newCard.setParent(deckContainer.transform);
        }
        ShuffleDeck();
    }

    //Creates a card from a cardsO
    public Card createCardFromCardSO(CardSO cardSO) {
        CardSO clonedCardSO=Instantiate(cardSO);
        
        Card newCard = (Card)System.Activator.CreateInstance(System.Type.GetType(cardSO.cardClass));        
        GameObject cardGameObject=Instantiate(cardPrefab);

        newCard.init(clonedCardSO, cardGameObject);

        Debug.Log("Init card "+newCard.cardName);

        return newCard;        
    }

    public void ShuffleDeck() {
        for (int i = 0; i < deck.Count; i++) {
            _ShuffleDeckSwapCards(i, UnityEngine.Random.Range(0, deck.Count-1));
        }
    }
    
    //Helper function for ShuffleLibrary
    private void _ShuffleDeckSwapCards(int i, int j) {
        var temp = deck[i];
        deck[i] = deck[j];
        deck[j] = temp;
    }
}
