using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "New Hand", fileName = "New Hand")] 

public class HandSO : ScriptableObject {
    public string handName;
    public HandType handType;
    public float mult;
    public float score;
}
