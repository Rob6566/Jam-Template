using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "New Game Length", fileName = "New Game Length")] 

public class GameLengthSO : ScriptableObject {
    public string gameLengthName;
    public GameLength gameLength;
    public List<EnemySO> enemies;
    public int BOSS_START_HP=10000;
    public int BOSS_START_TIMER=25;
    public int BOSS_SPAWN_TURN=100;
    public int SHOP_INCREMENT_INCREASE=20;
    public int SHOP_INITIAL_INCREMENT=100;
}
