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
    CardSuit cardSuit;
    public CardSuit CardSuit {get {
        foreach (CardBuffSO cardBuff in cardBuffs) {
            if (cardBuff.cardEnhancement==CardEnhancement.all_suits) {
                return CardSuit.all;
            }
        }
        return cardSuit;
    } 
    set {cardSuit=value;}}
    public CardRank cardRank;
    float cardScore;
    public float CardScore {get {
        float calculatedScore=cardScore;
        foreach (CardBuffSO cardBuff in cardBuffs) {
            if (cardBuff.cardEnhancement==CardEnhancement.increase_score) {
                calculatedScore+=gameManager.cardManager.CARD_SCORE_BUFF_VALUE;
            }
        }
        return calculatedScore;
    }}

    float cardMult=0;
    public float CardMult {get {
        float calculatedMult=cardMult;
        foreach (CardBuffSO cardBuff in cardBuffs) {
            if (cardBuff.cardEnhancement==CardEnhancement.increase_mult) {
                calculatedMult+=gameManager.cardManager.CARD_MULT_BUFF_VALUE;
            }
        }
        return calculatedMult;    
    }}
    public Sprite sprite;
    public string cardClass="BasicCard";

    List<CardBuffSO> cardBuffs = new List<CardBuffSO>();

    List<GameObject> bonusEffectUIs = new List<GameObject>();

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
        updateUI();
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
        for(int x=0; x<4; x++) {
            GameObject bonusEffectUI = cardUI.transform.GetChild(x).gameObject;
            bonusEffectUIs.Add(bonusEffectUI);
        }
        updateUI();
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

    public void updateUI() {
        if (cardZone==CardZone.deck) {
            setCardImage(gameManager.cardManager.cardBackSprite);
            hideAllBonusEffects();
        }
        else {
            setCardImage(sprite);
            showBonusEffects();
        }

        if (cardZone==CardZone.selectable || cardZone==CardZone.deck || cardZone==CardZone.discard || cardZone==CardZone.shop) {
            shrinkToRatio(gameManager.cardManager.SMALL_CARD_SIZE);
        }
        else {
            shrinkToRatio(gameManager.cardManager.TINY_CARD_SIZE);
        }      
    }

    private void showBonusEffects() {
        hideAllBonusEffects();
        int x=0;
        foreach (CardBuffSO cardBuff in cardBuffs) {
            if (x>3) {
                break;
            }
            bonusEffectUIs[x].SetActive(true);
            bonusEffectUIs[x].transform.GetChild(0).gameObject.GetComponent<Image>().sprite=cardBuff.sprite;
            bonusEffectUIs[x].transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text=cardBuff.buffString;
            x++;
        }
    }

    public void addBuff(CardBuffSO newBuff) {
        if (cardBuffs.Count>3) {
            return;
        }
        cardBuffs.Add(newBuff);
        updateUI();
    }


    private void hideAllBonusEffects() {
        foreach (GameObject bonusEffectUI in bonusEffectUIs) {
            bonusEffectUI.SetActive(false);
        }
    }

    public void modifyRank(int modification) {
        cardRank+=modification;
        if ((int)cardRank<0) {
            cardRank=CardRank.K;
        }
        if ((int)cardRank>12) {
            cardRank=CardRank.A;
        }
        gameManager.cardManager.updateCardSprite(this);

        updateUI();
    }
}
