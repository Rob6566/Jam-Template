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

    GameManager gameManager;
    
    //UI
    GameObject cardUI;
    public GameObject CardUI {get {return cardUI;}}
    RectTransform cardTransform;

    public string cardName;
    public CardSuit cardSuit;
    public CardRank cardRank;
    public float cardScore;
    public float cardMult=0;
    public Sprite sprite;
    public string cardClass="BasicCard";

    CardZone cardZone;
    public CardZone CardZone {get {return cardZone;}}

    Image cardImage;

    List<BonusEffect> bonusEffects = new List<BonusEffect>();


    public void init(GameManager newGameManager, CardSO newCardSO, GameObject newCardUI) {
        gameManager=newGameManager;
        cardSO=newCardSO;
        cardUI=newCardUI;
        cardName=cardSO.cardName;
        cardSuit=cardSO.cardSuit;
        cardRank=cardSO.cardRank;
        cardScore=cardSO.cardScore;
        sprite=cardSO.sprite;
        assignUIControls();
    }


    public void setZone(CardZone newZone) {
        cardZone=newZone;
        if (cardZone==CardZone.deck) {
            setCardImage(gameManager.cardManager.cardBackSprite);
        }
        else {
            setCardImage(sprite);
        }

        if (cardZone==CardZone.selectable || cardZone==CardZone.deck || cardZone==CardZone.discard) {
            shrinkToRatio(gameManager.cardManager.SMALL_CARD_SIZE);
        }
        else {
            shrinkToRatio(gameManager.cardManager.TINY_CARD_SIZE);
        }
    }

    public void Destroy() {
        //UnityEngine.Object.Destroy(currentHPUI);
        UnityEngine.Object.Destroy(cardSO);
        UnityEngine.Object.Destroy(cardUI);
    }

    public void setParent(Transform parent, bool worldPositionStays=false) {
        cardUI.transform.SetParent(parent, worldPositionStays);
    }

    /**
    * Deep clones this card and returns it
    * @param cardPrefab    The card prefab used to instantiate the new gameobject
    */
    public Card cloneCard(GameObject cardPrefab) {

        GameObject cardGameObject=UnityEngine.Object.Instantiate(cardPrefab);
        CardSO clonedCardSO=UnityEngine.Object.Instantiate(cardSO);

        Card newCard = (Card)System.Activator.CreateInstance(System.Type.GetType(cardSO.cardClass));

        newCard.init(gameManager, clonedCardSO, cardGameObject);
        //Probably want to overload init with a "Card" parameter, so we can clone any card modifications

        //Attach this card to the prefab, so it can respond to clicks
        //CardEventHandler cardEventHandler = cardGameObject.GetComponentInChildren<CardEventHandler>();
        //cardEventHandler.init(newCard);    

        return newCard;
    }


    /***************************** UI *****************************/    
    void assignUIControls() {
        setCardImage(sprite);
        cardTransform = cardUI.transform.GetComponent<RectTransform>();
    }

    void setCardImage(Sprite newSprite) {
        cardImage=cardUI.transform.GetComponent<Image>();
        cardImage.sprite=newSprite;
    }

    public void shrinkToRatio(float newRatio) {
        //Change the ratio of cards, as we designed them too big
        cardTransform.localScale = new Vector3(newRatio, newRatio, newRatio);
        //cardTransform.pivot = new Vector2(1,1);
    }

    //Disables all All-In-One Shader animations on the card
    public void disableAllShaders() {
        Material mat = cardImage.material;
        mat.DisableKeyword("OUTBASE_ON"); //Disable Fire outline
    }

    //Enables All-In-One Shader animations on the card
    public void enableShader(CardShader shaderToEnable) {
        Material mat = cardImage.material;
        if (shaderToEnable==CardShader.Glow) {
            mat.EnableKeyword("OUTBASE_ON"); //Fire outline
        }
    }
}