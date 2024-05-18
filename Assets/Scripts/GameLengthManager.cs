//Manages game speed

using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public enum GameLength {quick, normal, slow};
public class GameLengthManager : MonoBehaviour {

    GameLength gameLength;
    GameManager gameManager;
    public GameLength GameLength {get {return gameLength;}}
    public List<GameObject> scoreHolders=new List<GameObject>();
    public List<GameObject> scoreButtons=new List<GameObject>();
    public List<GameObject> scoreLabels=new List<GameObject>();
    public TextMeshProUGUI splashHighScoreTitle;

    [SerializeField]
    List<GameLengthSO> gameLengthSOs;

    GameLengthSO gameLengthSO;
    public GameLengthSO GameLengthSO {get {return gameLengthSO;}}

    public void init(GameManager newGameManager) {
        gameManager=newGameManager;
        setGameLength((int)GameLength.normal);
    }

    public void setGameLength(int newGameLength) {
        gameLength=(GameLength)newGameLength;
        int i=0;
        foreach(GameLengthSO gameLengthSO in gameLengthSOs) {
            scoreButtons[i].SetActive(gameLengthSO.gameLength!=gameLength);
            scoreLabels[i].SetActive(gameLengthSO.gameLength==gameLength);
            scoreHolders[i].SetActive(gameLengthSO.gameLength==gameLength);
            if (gameLengthSO.gameLength==gameLength) {
                this.gameLengthSO=gameLengthSO;
                splashHighScoreTitle.text="High Scores - "+gameLengthSO.gameLengthName;
            }
            i++;
        }
    }

    public EnemySO getRandomEnemy(int turnUpto) {
        EnemySO enemy = gameLengthSO.enemies[UnityEngine.Random.Range(0, gameLengthSO.enemies.Count)];
        if (enemy.spawnAfterRound>turnUpto || enemy.stopSpawningAfterRound<turnUpto) {
            return getRandomEnemy(turnUpto);
        }
        return enemy;
    }

    public void loadHighScores() {
        int i=0;
        foreach(GameLengthSO gameLengthSO in gameLengthSOs) {
            StartCoroutine(gameManager.highScoreManager.LoadScores(scoreHolders[i], Color.black, gameLengthSO.HIGH_SCORE_GAME_ID));
            i++;
        }
    }

}

