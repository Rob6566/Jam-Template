using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "New Music Set", fileName = "New Music Set")] 

public class MusicSet : ScriptableObject {
    public List<AudioClip> tracks;
    public MusicMood musicMood;
    public int loopsInTrack;
}