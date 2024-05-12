using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "New Card Buff", fileName = "Card Buff")] 

public class CardBuffSO : ScriptableObject {
    
    public string buffName;
    public Color color;
    public string buffString;
    public Sprite sprite;
    public CardEnhancement cardEnhancement;
    public bool fontWhite;
    public GameSound soundToPlay;
}
