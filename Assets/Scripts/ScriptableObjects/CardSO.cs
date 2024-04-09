using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "New Card", fileName = "New Card")] 

public class CardSO : ScriptableObject {
    
    [SerializeField]
    public string cardName;
    public CardSuit cardSuit;
    public CardRank cardRank;
    public float cardScore=0f;
    public Sprite sprite;
    public string cardClass="BasicCard";
}
