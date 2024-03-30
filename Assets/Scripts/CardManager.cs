//Handles generating cards

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public enum CardSuit {hearts, diamonds, clubs, spades, all};
public enum CardRank {A, two, three, four, five, six, seven, eight, nine, ten, J, Q, K, Joker};

public class CardManager : MonoBehaviour {

    GameManager gameManager;

    public void init(GameManager newGameManager) {
        gameManager=newGameManager ;
    }
}
