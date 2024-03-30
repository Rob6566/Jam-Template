using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "New Card", fileName = "New Card")] 

public class CardSO : ScriptableObject {
    
    [SerializeField]
    string cardName;
    public CardSuit cardSuit;
    public CardRank cardRank;
    public Sprite sprite;
}
