//Main game manager
//IMPORTANT - make sure you set a unique GAME_ID in HighScoreManager

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    //Managers
    public AudioManager audioManager;
    public HighScoreManager highScoreManager;
    public CardManager cardManager;
    
    public List<GameObject> canvasses = new List<GameObject>();
    
    public GameObject testCard;

    //Camera handling
    Camera camera;
    public Camera UIcamera;

    //Menu
    public GameObject menuContainer;


    //Auto-runs when game starts
    void Start() {
        //Setup cameras
        camera=GameObject.FindWithTag("MainCamera").GetComponent<Camera>();   
        camera.backgroundColor=Color.gray;
        UIcamera.enabled=false;
        UIcamera.enabled=true;

        //Link up managers
        audioManager=GameObject.FindWithTag("AudioManager").GetComponent<AudioManager>();   
        highScoreManager=GameObject.FindWithTag("HighScoreManager").GetComponent<HighScoreManager>();   
        highScoreManager.init();
        cardManager=GameObject.FindWithTag("CardManager").GetComponent<CardManager>();   

        //Play music
        audioManager.fadeInMusic(0, 0, 1f);

        //Load high scores
        StartCoroutine(highScoreManager.LoadScores());
    }

    public void setCardGlow(bool useGlow) {
        Material mat = testCard.GetComponent<Image>().material;
        if (useGlow) {
            mat.EnableKeyword("OUTBASE_ON");
        }
        else {
            mat.DisableKeyword("OUTBASE_ON");
        }
        
    }


    public void startGame() {
        if (!highScoreManager.validateName()) {
            return;
        }

        setCanvasStatus("GameCanvas", true);
        setCanvasStatus("ControlPanelCanvas", true, false);
    }

    void setCanvasStatus(string canvasTag, bool newState, bool hideOthers=true) {
        foreach(GameObject thisCanvas in canvasses) {
            if (thisCanvas.tag==canvasTag) {
                thisCanvas.SetActive(newState);
            }
            else if (hideOthers) {
                thisCanvas.SetActive(false);
            }
        }
    }
}
