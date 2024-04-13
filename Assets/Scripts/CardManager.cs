//Handles generating cards

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public enum CardSuit {hearts, diamonds, clubs, spades, all};
public enum CardRank {A, two, three, four, five, six, seven, eight, nine, ten, J, Q, K};
public enum CardShader {Glow};
public enum CardZone {deck, discard, selectable, nextup, hand1, hand2, hand3};
public enum HandType {high_card, pair, two_pair, three_of_a_kind, straight, flush, full_house, four_of_a_kind, straight_flush, five_of_a_kind, full_flush, five_flush};

public class CardManager : MonoBehaviour {

    public float TINY_CARD_SIZE=.3f;
    public float SMALL_CARD_SIZE=.4f;
//    public int NUM_RANKS=13;

    public List<CardSO> allCardSO = new List<CardSO>();
    public List<HandSO> allHandSO = new List<HandSO>();
    public GameObject cardPrefab;

    public Sprite cardBackSprite;

    public GameObject gameCanvas;


    public GameObject deckContainer;
    public GameObject dummyDeckContainer;
    public GameObject discardContainer;
    
    
    

    public List<GameObject> card1NextUpContainers;
    public List<GameObject> card2NextUpContainers;
    public List<GameObject> card3NextUpContainers;
    public List<GameObject> nextUpBonusContainers;

    public List<GameObject> handContainers = new List<GameObject>();

    public List<GameObject> currentDraftContainers = new List<GameObject>();
    public GameObject currentBonusContainer;

    public List<GameObject> hand1Containers = new List<GameObject>();
    public List<GameObject> hand2Containers = new List<GameObject>();
    public List<GameObject> hand3Containers = new List<GameObject>();



    List<Card> currentDraftCards = new List<Card>();
    List<Card> nextUp1DraftCards = new List<Card>();
    List<Card> nextUp2DraftCards = new List<Card>();
    List<Card> nextUp3DraftCards = new List<Card>();

    List<Card> allCards = new List<Card>();
    List<Card> deck = new List<Card>();
    List<Card> discard = new List<Card>();

    List<Card> hand1 = new List<Card>();
    List<Card> hand2 = new List<Card>();
    List<Card> hand3 = new List<Card>();

    GameManager gameManager;


    public GameObject testCard;
    public GameObject testTargetLocation;

    public void init(GameManager newGameManager) {
        gameManager=newGameManager;
        generateInitialDeck();

        //TESTING
        gameManager.animationManager.animateObjectToNewParent(testCard, testTargetLocation, 30f, .4f);
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
            deck.Add(newCard);

            //TESTING FIRE ANIMATION
            //newCard.disableAllShaders();
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

    private HandType getHandType(List<Card> hand) {
      
        //Check for flush
        bool flush=true;
        CardSuit suit=hand[0].cardSuit;
        foreach (Card card in hand) {
            if (card.cardSuit!=suit) {
                flush=false;
                break;
            }
        }

        

        //Check for straight
        List<Card> sortedHand=new List<Card>(hand);

        sortedHand.Sort(sortCardsByValue);
        bool straight=true;
        int cardUpto=0;
        int previousRank=(int)sortedHand[0].cardRank;
        foreach (Card card in sortedHand) {
            cardUpto++;
            if (cardUpto==4 && previousRank==12 && card.cardRank==0) {
                //Special handling for 10-J-Q-K-A straight
            }
            else if ((int)card.cardRank!=previousRank+1) {
                straight=false;
                break;
            }
            previousRank=(int)card.cardRank;
        }

        //Check for sets (pair, 3-5 of a kind, 2 pair, full house, )
        HandType setHandType=HandType.high_card;
        int pairs=0; int threeOfAKind=0; int fourOfAKind=0; int fiveOfAKind=0;
        for(int i=0; i<Enum.GetNames(typeof(CardRank)).Length; i++) {
            int cardsOfThisType=0;
            foreach (Card card in sortedHand) {
                if ((int)card.cardRank==i) {
                    cardsOfThisType++;
                }
            }
            if (cardsOfThisType==2) {
                pairs++;
            }
            else if (cardsOfThisType==3) {https://discussions.unity.com/t/enum-count/78841
                threeOfAKind++;
            }
            else if (cardsOfThisType==4) {
                fourOfAKind++;
            }
            else if (cardsOfThisType==5) {
                fiveOfAKind++;
            }
        }

        if (flush && fiveOfAKind>0) {
            return HandType.five_flush;
        }
        else if (flush && threeOfAKind>0 && pairs>0) {
            return HandType.full_flush;
        }
        else if (fiveOfAKind>0) {
            return HandType.five_of_a_kind;
        }
        else if (flush && straight) {
            return HandType.straight_flush;
        }
        else if (fourOfAKind>0) {
            return HandType.four_of_a_kind;
        }
        else if (threeOfAKind>0 && pairs>0) {
            return HandType.full_house;
        }
        else if (straight) {
            return HandType.straight;
        }
        else if (flush) {
            return HandType.flush;
        }
        else if (threeOfAKind>0) {
            return HandType.three_of_a_kind;
        }
        else if (pairs>1) {
            return HandType.two_pair;
        }
        else if (pairs>0) {
            return HandType.pair;
        }

        return HandType.high_card;
    }

    public int sortCardsByValue(Card card1, Card card2) {
        return card1.cardRank.CompareTo(card2.cardRank);
    }

    public void dealAllCards() {
        Invoke("dealAllCardsExecute", 1f);
    }
    
    //Animates dealing out all cards when the game starts
    void dealAllCardsExecute() {
        foreach(GameObject draftContainer in currentDraftContainers) {
            Card thisCard=deck[0];
            thisCard.setZone(CardZone.selectable);
            thisCard.CardUI.transform.SetParent(dummyDeckContainer.transform);
            //thisCard.CardUI.transform.localPosition=new Vector3(0f+dummyDeckContainer.GetComponent<RectTransform>().rect.width/2, 0f+dummyDeckContainer.GetComponent<RectTransform>().rect.height/2, 0f);
            deck.Remove(thisCard);
            currentDraftCards.Add(thisCard);
            //gameManager.animationManager.animateObjectToNewParent(thisCard.CardUI, draftContainer, 30f, .4f);
            thisCard.CardUI.transform.SetParent(draftContainer.transform);
            thisCard.CardUI.transform.localPosition=new Vector3(0f, 0f, 0f);
        }

        dealNextUpCards();
    }

    public void discardHand(int hand) {
        List<Card> handToDiscard=(hand==1 ? hand1 : (hand==2 ? hand2 : hand3));
        foreach(Card thisCard in handToDiscard) {
            thisCard.setZone(CardZone.discard);
            thisCard.CardUI.transform.SetParent(discardContainer.transform);
            thisCard.CardUI.transform.localPosition=new Vector3(0f, 0f, 0f);
            discard.Add(thisCard);
        }
        handToDiscard.Clear();
    }

    public void dealNextUpCards() {
        foreach(GameObject draftContainer in card1NextUpContainers) {
            Card thisCard=drawCard();
            thisCard.setZone(CardZone.nextup);
            thisCard.CardUI.transform.SetParent(dummyDeckContainer.transform);
            //thisCard.CardUI.transform.localPosition=new Vector3(0f+dummyDeckContainer.GetComponent<RectTransform>().rect.width/2, 0f+dummyDeckContainer.GetComponent<RectTransform>().rect.height/2, 0f);
            nextUp1DraftCards.Add(thisCard);
            //gameManager.animationManager.animateObjectToNewParent(thisCard.CardUI, draftContainer, 30f, .4f);
            thisCard.CardUI.transform.SetParent(draftContainer.transform);
            thisCard.CardUI.transform.localPosition=new Vector3(0f, 0f, 0f);
        }
        foreach(GameObject draftContainer in card2NextUpContainers) {
            Card thisCard=drawCard();
            thisCard.setZone(CardZone.nextup);
            thisCard.CardUI.transform.SetParent(dummyDeckContainer.transform);
            //thisCard.CardUI.transform.localPosition=new Vector3(0f+dummyDeckContainer.GetComponent<RectTransform>().rect.width/2, 0f+dummyDeckContainer.GetComponent<RectTransform>().rect.height/2, 0f);
            nextUp2DraftCards.Add(thisCard);
            //gameManager.animationManager.animateObjectToNewParent(thisCard.CardUI, draftContainer, 30f, .4f);
            thisCard.CardUI.transform.SetParent(draftContainer.transform);
            thisCard.CardUI.transform.localPosition=new Vector3(0f, 0f, 0f);
        }
        foreach(GameObject draftContainer in card3NextUpContainers) {
            Card thisCard=drawCard();
            thisCard.setZone(CardZone.nextup);
            thisCard.CardUI.transform.SetParent(dummyDeckContainer.transform);
            //thisCard.CardUI.transform.localPosition=new Vector3(0f+dummyDeckContainer.GetComponent<RectTransform>().rect.width/2, 0f+dummyDeckContainer.GetComponent<RectTransform>().rect.height/2, 0f);
            nextUp3DraftCards.Add(thisCard);
            //gameManager.animationManager.animateObjectToNewParent(thisCard.CardUI, draftContainer, 30f, .4f);
            thisCard.CardUI.transform.SetParent(draftContainer.transform);
            thisCard.CardUI.transform.localPosition=new Vector3(0f, 0f, 0f);
        }
    }

    public Card drawCard() {
        if (deck.Count==0) {
            shuffleDiscardIntoDeck();
        }
        Card thisCard=deck[0];
        deck.Remove(thisCard);
        return thisCard;
    }

    void shuffleDiscardIntoDeck() {
        foreach (Card card in discard) {
            card.setZone(CardZone.deck);
            card.setParent(deckContainer.transform);
            deck.Add(card);
        }
        discard.Clear();
        ShuffleDeck();
    }

    //Hides all containers used for laying stuff out. Called at the start
    public void hideAllContainerImages() {
        deckContainer.GetComponent<Image>().enabled=false;
        dummyDeckContainer.GetComponent<Image>().enabled=false;
        discardContainer.GetComponent<Image>().enabled=false;
        currentBonusContainer.GetComponent<Image>().enabled=false;

        foreach (GameObject container in currentDraftContainers) {
            container.GetComponent<Image>().enabled=false;
            Debug.Log("Hiding container "+container.name);
        }

        foreach (GameObject container in currentDraftContainers) {
            container.GetComponent<Image>().enabled=false;
        }

        foreach (GameObject container in card1NextUpContainers) {
            container.GetComponent<Image>().enabled=false;
        }

        foreach (GameObject container in card2NextUpContainers) {
            container.GetComponent<Image>().enabled=false;
        }

        foreach (GameObject container in card3NextUpContainers) {
            container.GetComponent<Image>().enabled=false;
        }

        foreach (GameObject container in nextUpBonusContainers) {
            container.GetComponent<Image>().enabled=false;
        }

        foreach (GameObject container in hand1Containers) {
            container.GetComponent<Image>().enabled=false;
        }

        foreach (GameObject container in hand2Containers) {
            container.GetComponent<Image>().enabled=false;
        }

        foreach (GameObject container in hand3Containers) {
            container.GetComponent<Image>().enabled=false;
        }

    }

    public void draftCardToHand(int hand, int cardPicked) {
             Card cardDrafted = currentDraftCards[cardPicked-1];
             CardZone zoneToMoveTo=(hand==1 ? CardZone.hand1 : (hand==2 ? CardZone.hand2 : CardZone.hand3));
             List<Card> handToMoveTo=(hand==1 ? hand1 : (hand==2 ? hand2 : hand3));
             List<GameObject> handContainers=(hand==1 ? hand1Containers : (hand==2 ? hand2Containers : hand3Containers));

             bool completedHand=false;

             foreach(Card thisCard in currentDraftCards) {
                if (thisCard==cardDrafted) {
                //Move Drafted card to hand
                    thisCard.setZone(zoneToMoveTo);
                    handToMoveTo.Add(thisCard);
                    if (handToMoveTo.Count==5) {
                        completedHand=true;
                    }
                    thisCard.setParent(handContainers[handToMoveTo.Count-1].transform);
                    thisCard.CardUI.transform.localPosition=new Vector3(0f, 0f, 0f);
                } 
                else {
                //Move non-Drafted cards to discard
                    thisCard.setZone(CardZone.discard);
                    thisCard.CardUI.transform.localPosition=new Vector3(0f, 0f, 0f);
                    thisCard.setParent(discardContainer.transform);
                    discard.Add(thisCard);
                }
             }
             currentDraftCards.Clear();
            
             
             for(int i=1; i<=3; i++) {
                List<Card> nextUpDraftCards=(i==1 ? nextUp1DraftCards : (i==2 ? nextUp2DraftCards : nextUp3DraftCards));
                List<GameObject> nextUpContainers=(i==1 ? card1NextUpContainers : (i==2 ? card2NextUpContainers : card3NextUpContainers));

                if (i==cardPicked) {
                    //Move next up cards in group to current cards
                    int nextCardUpto=0;
                    foreach(Card thisCard in nextUpDraftCards) {
                        thisCard.setZone(CardZone.selectable);
                        thisCard.setParent(currentDraftContainers[nextCardUpto].transform);
                        thisCard.CardUI.transform.localPosition=new Vector3(0f, 0f, 0f);
                        currentDraftCards.Add(thisCard);
                        nextCardUpto++;
                    }
                }
                else {
                    foreach(Card thisCard in nextUpDraftCards) {
                        //Move next up cards in other groups to discard
                        thisCard.setZone(CardZone.discard);
                        thisCard.CardUI.transform.localPosition=new Vector3(0f, 0f, 0f);
                        thisCard.setParent(discardContainer.transform);
                        discard.Add(thisCard);
                    }
                }
             }
             nextUp1DraftCards.Clear(); nextUp2DraftCards.Clear(); nextUp3DraftCards.Clear();

            if (completedHand) {
                scoreHand(hand);
            }
            else {
                dealNextUpCards();
            }
        }

    void scoreHand(int hand) {
        List<Card> handToScore=(hand==1 ? hand1 : (hand==2 ? hand2 : hand3));
        HandType handType=getHandType(handToScore);
        Debug.Log("Hand type is "+handType);

        HandSO ourHandSO=null;
        foreach(HandSO handSO in allHandSO) {
            if (handSO.handType==handType) {
                ourHandSO=handSO;
            }
        }

        Debug.Log("About to score hand "+hand+": "+handToScore[0].cardName+" "+handToScore[1].cardName+" "+handToScore[2].cardName+" "+handToScore[3].cardName+" "+handToScore[4].cardName);

        gameManager.scoreHand(hand, ourHandSO, handToScore);
    }



    void checkHand(int hand) {

    }

}
