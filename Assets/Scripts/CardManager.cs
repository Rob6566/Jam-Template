//Handles generating cards

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum CardSuit {hearts, diamonds, clubs, spades, all};
public enum CardRank {A, two, three, four, five, six, seven, eight, nine, ten, J, Q, K, Joker};
public enum CardShader {Glow};
public enum CardZone {deck, discard, selectable, nextup, hand1, hand2, hand3};

public class CardManager : MonoBehaviour {

    float SMALL_CARD_SIZE=.4f;

    public List<CardSO> allCardSO = new List<CardSO>();
    public GameObject cardPrefab;

    public Sprite cardBackSprite;


    public GameObject deckContainer;
    public GameObject discardContainer;

    List<Card> currentDraftCards = new List<Card>();


    List<Card> allCards = new List<Card>();
    List<Card> deck = new List<Card>();
    List<Card> discard = new List<Card>();

    GameManager gameManager;


    public GameObject testCard;
    public GameObject testTargetLocation;

    public void init(GameManager newGameManager) {
        gameManager=newGameManager;
        generateInitialDeck();

        //TESTING
        gameManager.animationManager.animateObjectToNewParent(testCard, testTargetLocation, 30f, 1.3f);
    }

    public void generateInitialDeck() {
        allCards.Clear();
        foreach (CardSO cardSO in allCardSO) {
            Card newCard = createCardFromCardSO(cardSO);
            allCards.Add(newCard);
            //gameManager.animationManager.animateObjectToNewParent(newCard.CardUI, deckContainer, .1f);
            newCard.shrinkToRatio(SMALL_CARD_SIZE);
            newCard.setParent(deckContainer.transform);
            newCard.setZone(CardZone.deck);
            

            //TESTING FIRE ANIMATION
            newCard.disableAllShaders();
        }
        ShuffleDeck();
    }

    //Creates a card from a cardsO
    public Card createCardFromCardSO(CardSO cardSO) {
        CardSO clonedCardSO=Instantiate(cardSO);
        
        Card newCard = (Card)System.Activator.CreateInstance(System.Type.GetType(cardSO.cardClass));        
        GameObject cardGameObject=Instantiate(cardPrefab);

        newCard.init(gameManager, clonedCardSO, cardGameObject);

        //Debug.Log("Init card "+newCard.cardName);

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
