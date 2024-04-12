using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "New Bonus Effect", fileName = "New Bonus Effect")] 

public class BonusEffectSO : ScriptableObject {
    
    [SerializeField]
    public string effectName;
    public string effectText;
    public string effectClass="BasicEffect";
}
