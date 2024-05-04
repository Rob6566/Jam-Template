//Abstract superclass for cards. When instantiating, use BasicCard or create a sub-class in the CardClasses folder

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class Enemy {

    EnemySO enemySO;

    GameManager gameManager;
    
    //UI
    GameObject enemyUI;
    public GameObject EnemyUI {get {return enemyUI;}}
    RectTransform enemyTransform;

    public string enemyName;
    public Sprite sprite;

    public int HP;
    public int turnsUntilAttack;

    Image enemyImage;

    public TextMeshProUGUI TXThp;
    public TextMeshProUGUI TXTturnsUntilAttack;
    public int enemyPosition;

    GameObject hpContainer;
    GameObject timerContainer;


    public void init(GameManager newGameManager, EnemySO newEnemySO, GameObject newEnemyUI, int newEnemyPosition) {
        gameManager=newGameManager;
        enemyUI=newEnemyUI;
        enemySO=newEnemySO;
        enemyName=enemySO.enemyName;
        enemyPosition=newEnemyPosition;
        sprite=enemySO.sprite;
        HP=enemySO.HP;
        turnsUntilAttack=enemySO.turnsUntilAttack;
        assignUIControls();
    }

    public void Destroy() {
        //UnityEngine.Object.Destroy(currentHPUI);
//        UnityEngine.Object.Destroy(enemySO);

        Debug.Log("iiii destroying enemy "+enemyUI.name);
        UnityEngine.Object.Destroy(enemyUI);
    }

    /***************************** UI *****************************/    
    void assignUIControls() {
        enemyImage=enemyUI.transform.GetComponent<Image>();
        enemyImage.sprite=sprite;
        enemyTransform = enemyUI.transform.GetComponent<RectTransform>();
        TXThp = enemyUI.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        hpContainer=enemyUI.transform.GetChild(0).gameObject;
        TXTturnsUntilAttack= enemyUI.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
        timerContainer=enemyUI.transform.GetChild(1).gameObject;

        updateUI();
    }

    public void updateUI() {
        TXThp.text = HP.ToString();
        TXTturnsUntilAttack.text = turnsUntilAttack.ToString();
    }

    public void animateHP() {
        //gameManager.animationManager.animateObjectExpandAndFade(hpContainer, .5f, 5f);
        gameManager.animationManager.animateObject(AnimationType.expandAndFade, hpContainer, null, null, 5f, .5f);
    }

    public void animateTimer() {
        //gameManager.animationManager.animateObjectExpandAndFade(timerContainer, .5f, 5f);
        gameManager.animationManager.animateObject(AnimationType.expandAndFade, timerContainer, null, null, 5f, .5f);
    }
}