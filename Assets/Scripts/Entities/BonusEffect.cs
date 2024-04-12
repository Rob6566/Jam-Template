//Abstract superclass for cards. When instantiating, use BasicCard or create a sub-class in the CardClasses folder

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class BonusEffect {

    BonusEffectSO bonusEffectSO;
    
    //UI
    GameObject bonusUI;
    public GameObject BonusUI {get {return bonusUI;}}

    public string effectName;
    public string effectText;
    public string cardClass="BasicEffect";

    Image cardImage;

    public void init(BonusEffectSO newEffectSO, GameObject newBonusUI) {
        bonusEffectSO=newEffectSO;
        effectName=bonusEffectSO.effectName;
        effectText=bonusEffectSO.effectText;
        bonusUI=newBonusUI;

        assignUIControls();
    }

    public void Destroy() {
        UnityEngine.Object.Destroy(bonusEffectSO);
        UnityEngine.Object.Destroy(bonusUI);
    }

    /*public void setParent(Transform parent, bool worldPositionStays=false) {
        cardUI.transform.SetParent(parent, worldPositionStays);
    }*/

    /**
    * Deep clones this card and returns it
    * @param cardPrefab    The card prefab used to instantiate the new gameobject
    */
    public BonusEffect cloneEffect(GameObject effectPrefab) {

        GameObject effectObject=UnityEngine.Object.Instantiate(effectPrefab);
        BonusEffectSO clonedEffectSO=UnityEngine.Object.Instantiate(bonusEffectSO);

        BonusEffect newEffect = (BonusEffect)System.Activator.CreateInstance(System.Type.GetType(clonedEffectSO.effectClass));

        newEffect.init(clonedEffectSO, effectObject);

        return newEffect;
    }


    /***************************** UI *****************************/    
    void assignUIControls() {
       // cardImage=cardUI.transform.GetComponent<Image>();
       // cardImage.sprite=sprite;
       // cardTransform = cardUI.transform.GetComponent<RectTransform>();
    }

    /*public void shrinkToRatio(float newRatio) {
        //Change the ratio of cards, as we designed them too big
        cardTransform.localScale = new Vector3(newRatio, newRatio, newRatio);
        cardTransform.pivot = new Vector2(1,1);
    }*/

    //Disables all All-In-One Shader animations on the card
    /*public void disableAllShaders() {
        Material mat = cardImage.material;
        mat.DisableKeyword("OUTBASE_ON"); //Disable Fire outline
    }*/

    //Enables All-In-One Shader animations on the card
    /*public void enableShader(CardShader shaderToEnable) {
        Material mat = cardImage.material;
        if (shaderToEnable==CardShader.Glow) {
            mat.EnableKeyword("OUTBASE_ON"); //Fire outline
        }
    }*/
}