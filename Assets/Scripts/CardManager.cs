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
public enum CardZone {deck, discard, selectable, nextup, hand1, hand2, hand3, shop, all};
public enum HandType {high_card, pair, two_pair, three_of_a_kind, straight, flush, full_house, four_of_a_kind, straight_flush, five_of_a_kind, full_flush, five_flush};
public enum CardEnhancement {increase_score, increase_mult, all_suits, remove_card, increase_rank, decrease_rank, copy_card, increase_timer};

public class CardManager : MonoBehaviour {

    public float TINY_CARD_SIZE=.32f;
    public float SMALL_CARD_SIZE=.4f;

    //Bonuses conferred on cards by enhancements
    public int CARD_SCORE_BUFF_VALUE=20; 
    public int CARD_MULT_BUFF_VALUE=2;

//    public int NUM_RANKS=13;

    public List<CardSO> allCardSO = new List<CardSO>();
    public List<HandSO> allHandSO = new List<HandSO>();
    public List<CardBuffSO> allBuffs = new List<CardBuffSO>();
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

    public List<GameObject> nextUpHolders;
    public List<GameObject> nextUpHolderBackgrounds;
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

    int mostRecentHandDrafted;


    public GameObject testCard;
    public GameObject testTargetLocation;


    /************ Objects for deck overlay ***************/
    //A label and button for each type of display (all deck, draw, discard)
    public List<GameObject> lstButtonAllDeck;
    public List<GameObject> lstButtonDraw;
    public List<GameObject> lstButtonDiscard;

    public List<TextMeshProUGUI> suitTotals;
    public List<TextMeshProUGUI> rankTotals;
    public List<TextMeshProUGUI> tableTotals;
    public TextMeshProUGUI TXTtotalCards;

    public void init(GameManager newGameManager) {
        gameManager=newGameManager;
        generateInitialDeck();

        //TESTING
        //gameManager.animationManager.animateObjectToNewParent(testCard, testTargetLocation, 30f, .4f);
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
        CardSuit suit=hand[0].CardSuit;
        foreach (Card card in hand) {
            if (suit==CardSuit.all) {
                suit=card.CardSuit;
            }
            if (card.CardSuit!=suit && card.CardSuit!=CardSuit.all) {
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
            if (cardUpto==1) {continue;}
            else if (cardUpto==2 && previousRank==(int)CardRank.A && card.cardRank==CardRank.ten) {
                //Special handling for 10-J-Q-K-A straight
                Debug.Log("Straight with Ace, card ="+card.cardName+" previousRank="+previousRank);
            }
            else if ((int)card.cardRank!=previousRank+1) {
                straight=false;
                Debug.Log("No Straight, card ="+card.cardName+" previousRank="+previousRank);
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
                Debug.Log("Hand has pair");
            }
            else if (cardsOfThisType==3) {
                threeOfAKind++;
                Debug.Log("Hand has 3 of a kind");
            }
            else if (cardsOfThisType==4) {
                fourOfAKind++;
                Debug.Log("Hand has 4 of a kind");
            }
            else if (cardsOfThisType==5) {
                fiveOfAKind++;
                Debug.Log("Hand has 5 of a kind");
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
        Debug.Log("ZZZ Deal all cards");
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
            discardCard(thisCard);
        }
        handToDiscard.Clear();
    }

    public void discardCard(Card card) {
            card.setZone(CardZone.discard);
            card.CardUI.transform.SetParent(discardContainer.transform);
            card.CardUI.transform.localPosition=new Vector3(0f, 0f, 0f);
            discard.Add(card);
    }

    public void dealNextUpCards() {
        //Tick down enemies
        gameManager.tickDownEnemies(mostRecentHandDrafted-1);

        Debug.Log("ZZZ Deal next up cards");
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

        if (gameManager.GameState!=GameState.between_games) {
            //gameManager.audioManager.playSound(GameSound.shuffle, .6f);
            //Removed - this sound was a bit grating
        }

        Card thisCard=deck[0];
        deck.Remove(thisCard);

        if (deck.Count==0) { //An empty deck looks shit - shuffle it
            shuffleDiscardIntoDeck();
        }

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
             gameManager.tutorialManager.draftedCard();
             gameManager.audioManager.playSound(GameSound.flick, 1f);
             mostRecentHandDrafted=hand;
             
             Card cardDrafted = currentDraftCards[cardPicked-1];
             CardZone zoneToMoveTo=(hand==1 ? CardZone.hand1 : (hand==2 ? CardZone.hand2 : CardZone.hand3));
             List<Card> handToMoveTo=(hand==1 ? hand1 : (hand==2 ? hand2 : hand3));
             List<GameObject> handContainers=(hand==1 ? hand1Containers : (hand==2 ? hand2Containers : hand3Containers));

             int timerIncrease=cardDrafted.getTimerIncrease();
             if (timerIncrease>0 && gameManager.GameState!=GameState.boss) {
                 gameManager.increaseEnemyTimer(timerIncrease, hand);
             }

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

    public string getHandTypeName(HandType handType) {
        foreach(HandSO handSO in allHandSO) {
            if (handSO.handType==handType) {
                return handSO.handName;
            }
        }
        return "Unknown Hand Type";
    }


    public void hoveredButton(int targetHand, int cardPicked) {
        float LOW_OPACITY=.1f;
        int containerUpto=0;
        foreach(GameObject container in nextUpHolders) {
            float transLevel=(containerUpto+1==cardPicked ? 1f : LOW_OPACITY);
            setContainerTrans(container,transLevel);
            containerUpto++;
        }
        containerUpto=0;
        foreach(GameObject container in nextUpHolderBackgrounds) {
            float transLevel=(containerUpto+1==cardPicked ? 1f : LOW_OPACITY);
            setContainerTrans(container,transLevel);
            containerUpto++;
        }
        containerUpto=0;
        foreach(GameObject container in handContainers) {
            float transLevel=(containerUpto+1==targetHand ? 1f : LOW_OPACITY);
            setContainerTrans(container, transLevel);
            containerUpto++;
        }
        containerUpto=0;
        foreach(GameObject container in currentDraftContainers) {
            float transLevel=(containerUpto+1==cardPicked ? 1f : LOW_OPACITY);
            setContainerTrans(container, transLevel);
            containerUpto++;
        }
    }
    
    //Set all button hovers back to normal
    public void unHoveredButton() {
        foreach(GameObject container in nextUpHolders) {
            setContainerTrans(container, 1f);
        }
        foreach(GameObject container in nextUpHolderBackgrounds) {
            setContainerTrans(container, .95f);
        }
        foreach(GameObject container in handContainers) {
            setContainerTrans(container, .95f);
        }
        foreach(GameObject container in currentDraftContainers) {
            setContainerTrans(container, 1f);
        }
    }

    void setContainerTrans(GameObject container, float transparency) {
        container.GetComponent<CanvasGroup>().alpha=transparency;
    }

    public CardBuffSO getRandomBuff() {
        return allBuffs[UnityEngine.Random.Range(0, allBuffs.Count)];
    }

    public void updateCardSprite(Card card) {
        foreach(CardSO cardSO in allCardSO) {
            if (cardSO.cardRank==card.cardRank && cardSO.cardSuit==card.CardSuit) {
                card.sprite=cardSO.sprite;
                card.updateUI();
            }
        }
    }

    public void registerCard(Card card) {
        allCards.Add(card);
    }

    
    public void resetDeck() {
        Debug.Log("Resestting deck - cards="+allCards.Count);
        
        foreach(Card card in allCards) {
            Debug.Log("Deleting card "+card.cardName);
            card.Destroy();
        }

        allCards.Clear();
        deck.Clear();
        discard.Clear();
        hand1.Clear();
        hand2.Clear();
        hand3.Clear(); 
        currentDraftCards.Clear();
        nextUp1DraftCards.Clear();
        nextUp2DraftCards.Clear();
        nextUp3DraftCards.Clear();

        generateInitialDeck();
    }

    public void loadDeckStats(CardZone zone) {
        List<int> suitTotalList=new List<int>();
        List<int> rankTotalList=new List<int>();

        CardZone zoneToShow=CardZone.all;
        if (zone==CardZone.deck) {
            zoneToShow=CardZone.deck;
        }
        else if (zone==CardZone.discard) {
            zoneToShow=CardZone.discard;
        }

        //Initialise card lists
        List<List<int>> allCards=new List<List<int>>();
        for(int suit=0; suit<4; suit++) {
            List<int> suitCards=new List<int>();
            suitTotalList.Add(0);
            Debug.Log("Suit=adding "+suit);
            for(int rank=0; rank<Enum.GetNames(typeof(CardRank)).Length; rank++) {
                suitCards.Add(0);
                if (suit==0) {
                    rankTotalList.Add(0);
                }
            }
            allCards.Add(suitCards);
        }

        //Loop through cards and figure out how much of each we have
        int cardCount=0;
        foreach(Card card in this.allCards) {
            if (zoneToShow!=CardZone.all && card.CardZone!=zoneToShow) {
                continue;
            }
            cardCount++;

            if(card.CardSuit==CardSuit.all) {
                for(int suit=0; suit<4; suit++) {
                    allCards[suit][(int)card.cardRank]++;
                    suitTotalList[suit]++;
                }   
            }
            else {
                allCards[(int)card.CardSuit][(int)card.cardRank]++;
                suitTotalList[(int)card.CardSuit]++;
            }
            rankTotalList[(int)card.cardRank]++;
        }

        //Add label texts
        for(int suit=0; suit<4; suit++) {
            for(int rank=0; rank<Enum.GetNames(typeof(CardRank)).Length; rank++) {
                int totalTxtIndex=suit*Enum.GetNames(typeof(CardRank)).Length+rank;
                
                tableTotals[totalTxtIndex].text=allCards[suit][rank].ToString();
                rankTotals[rank].text=rankTotalList[rank].ToString();
            }
            Debug.Log("Suit="+suit+" total="+suitTotalList[suit]);
            suitTotals[suit].text=suitTotalList[suit].ToString();
        }

        TXTtotalCards.text=cardCount.ToString();

        //Show/hide "tab" button/labels
        lstButtonAllDeck[0].SetActive(zoneToShow==CardZone.all);
        lstButtonAllDeck[1].SetActive(zoneToShow!=CardZone.all);
        lstButtonDraw[0].SetActive(zoneToShow==CardZone.deck);
        lstButtonDraw[1].SetActive(zoneToShow!=CardZone.deck);
        lstButtonDiscard[0].SetActive(zoneToShow==CardZone.discard);
        lstButtonDiscard[1].SetActive(zoneToShow!=CardZone.discard);
    }

    public void removeCard(Card card) {
        allCards.Remove(card);
        deck.Remove(card);
        discard.Remove(card);
        hand1.Remove(card);
        hand2.Remove(card);
        hand3.Remove(card);
        currentDraftCards.Remove(card);
        nextUp1DraftCards.Remove(card);
        nextUp2DraftCards.Remove(card);
        nextUp3DraftCards.Remove(card);
    }
}
