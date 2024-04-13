using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "New Enemy", fileName = "New Enemy")] 

public class EnemySO : ScriptableObject {
    
    [SerializeField]
    public string enemyName;
    public Sprite sprite;
    public int HP;
    public int turnsUntilAttack;
    public int difficulty;
}
