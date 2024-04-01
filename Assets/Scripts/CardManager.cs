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

    GameManager gameManager;

    public void init(GameManager newGameManager) {
        gameManager=newGameManager ;
    }
}
