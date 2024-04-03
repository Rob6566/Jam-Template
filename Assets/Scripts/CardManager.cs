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

    List<Card> allCards = new List<Card>();

    GameManager gameManager;

    public void init(GameManager newGameManager) {
        gameManager=newGameManager ;
    }

    public void generateInitialDeck() {
        allCards.Clear();
        foreach (CardSO cardSO in allCardSO) {
            Card newCard = createCardFromCardSO(cardSO);
            allCards.Add(newCard);
        }
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

}
