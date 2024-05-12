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
    public GameLength GameLength {get {return gameLength;}}

    [SerializeField]
    List<GameLengthSO> gameLengthSOs;

    GameLengthSO gameLengthSO;
    public GameLengthSO GameLengthSO {get {return gameLengthSO;}}

    public void init() {
        setGameLength((int)GameLength.normal);
    }

    public void setGameLength(int newGameLength) {
        gameLength=(GameLength)newGameLength;
        foreach(GameLengthSO gameLengthSO in gameLengthSOs) {
            if (gameLengthSO.gameLength==gameLength) {
                this.gameLengthSO=gameLengthSO;
            }
        }
    }

    public EnemySO getRandomEnemy(int turnUpto) {
        EnemySO enemy = gameLengthSO.enemies[UnityEngine.Random.Range(0, gameLengthSO.enemies.Count)];
        if (enemy.spawnAfterRound>turnUpto || enemy.stopSpawningAfterRound<turnUpto) {
            return getRandomEnemy(turnUpto);
        }
        return enemy;
    }

}

