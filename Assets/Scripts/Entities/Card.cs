//Abstract superclass for cards. When instantiating, use BasicCard or create a sub-class in the CardClasses folder

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class Card {

    CardSO cardSO;

    protected int turnsUntilFirstAttack=1;
    public int TurnsUntilFirstAttack {get {return turnsUntilFirstAttack;}  set {turnsUntilFirstAttack=value;}}
    
    //UI
    GameObject cardUI;
    //TextMeshProUGUI currentHPUI;
    RectTransform cardTransform;

    public string cardName;
    public CardSuit cardSuit;
    public CardRank cardRank;
    public Sprite sprite;
    public string cardClass="BasicCard";


    public void init(CardSO newCardSO, GameObject newCardUI) {
        cardSO=newCardSO;
        cardUI=newCardUI;
        cardName=cardSO.cardName;
        cardSuit=cardSO.cardSuit;
        cardRank=cardSO.cardRank;
        sprite=cardSO.sprite;
        assignUIControls();
    }

    public void Destroy() {
        //UnityEngine.Object.Destroy(currentHPUI);
        UnityEngine.Object.Destroy(cardSO);
        UnityEngine.Object.Destroy(cardUI);
    }

    /**
    * Deep clones this card and returns it
    * @param cardPrefab    The card prefab used to instantiate the new gameobject
    */
    public Card cloneCard(GameObject cardPrefab) {

        GameObject cardGameObject=UnityEngine.Object.Instantiate(cardPrefab);
        CardSO clonedCardSO=UnityEngine.Object.Instantiate(cardSO);

        Card newCard = (Card)System.Activator.CreateInstance(System.Type.GetType(cardSO.cardClass));

        newCard.init(clonedCardSO, cardGameObject);

        //Attach this card to the prefab, so it can respond to clicks
        //CardEventHandler cardEventHandler = cardGameObject.GetComponentInChildren<CardEventHandler>();
        //cardEventHandler.init(newCard);    

        return newCard;
    }


    /***************************** UI *****************************/    
    void assignUIControls() {
        //TODO - improve these references
        //currentHPUI=cardUI.transform.GetChild(10).GetComponent<TextMeshProUGUI>();
        cardTransform = cardUI.transform.GetComponent<RectTransform>();
    }

    public void shrinkToRatio(float newRatio) {
        //Change the ratio of cards, as we designed them too big
        cardTransform.localScale = new Vector3(newRatio, newRatio, newRatio);
        cardTransform.pivot = new Vector2(1,1);
    }
}