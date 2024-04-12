//Main game manager
//IMPORTANT - make sure you set a unique GAME_ID in HighScoreManager

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public enum GameState {paused, picking_card, scoring, between_games};

public class GameManager : MonoBehaviour {
    //Managers
    public AudioManager audioManager;
    public HighScoreManager highScoreManager;
    public CardManager cardManager;
    public AnimationManager animationManager;
    
    public List<GameObject> canvasses = new List<GameObject>();
    
    public GameObject testCard;

    //Camera handling
    Camera camera;
    [SerializeField]
    Camera UIcamera;

    //Menu
    [SerializeField]
    GameObject menuContainer;

    private GameState gameState=GameState.between_games;
    private GameState previousGameState=GameState.between_games;
    public GameState GameState {get {return gameState;}}
    private float gameSleepFor=0f;

    //Animation Speed
    [SerializeField]
    Scrollbar animationSpeedScrollbar;
    private float animationSpeed=1f; 
    public float AnimationSpeed {get {return animationSpeed;}}
    public void setAnimationSpeed() {
        switch (animationSpeedScrollbar.value) {
        case 1f:
            animationSpeed=5f;
            break;
        case .75f:
            animationSpeed=2f;
            break;
        case .5f:
            animationSpeed=1f;
            break;
        case .25f:
            animationSpeed=.7f;
            break;
        case 0f:
            animationSpeed=.35f;
            break;
        default:
            animationSpeed=1f;
            break;
        }
    }



    //Auto-runs when game starts
    void Start() {
        //Setup cameras
        camera=GameObject.FindWithTag("MainCamera").GetComponent<Camera>();   
        camera.backgroundColor=Color.gray;
        UIcamera.enabled=false;
        UIcamera.enabled=true;

        //Link up managers
        audioManager=GameObject.FindWithTag("AudioManager").GetComponent<AudioManager>();   
        animationManager=GameObject.FindWithTag("AnimationManager").GetComponent<AnimationManager>();
        animationManager.init(this);   
        highScoreManager=GameObject.FindWithTag("HighScoreManager").GetComponent<HighScoreManager>();   
        highScoreManager.init();
        
        
        cardManager=GameObject.FindWithTag("CardManager").GetComponent<CardManager>();   
        cardManager.init(this);

        setCanvasStatus("SplashCanvas", true);

        setMenuActive(false);

        setAnimationSpeed();

        //Play music
        audioManager.initAdaptiveMusic(); //fadeInMusic(0, 0, 1f);

        //Load high scores
        StartCoroutine(highScoreManager.LoadScores());
    }

    //Handle game loop
    void Update() {
        if (gameState==GameState.between_games || gameState==GameState.paused) {
            return;
        }

        if (gameSleepFor>0) {
            gameSleepFor-=(Time.deltaTime*animationSpeed);
            return;
        }

        if (animationManager.isAnimating()) {
            return;
        }
    }

    public void pauseGame(bool pauseStatus) {
        if (pauseStatus) {
            previousGameState=gameState;
            gameState=GameState.paused;
        }
        else if (gameState==GameState.paused) {
            gameState=previousGameState;
        }
    }



    //Testing
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
        //audioManager.changeMusicMood(MusicMood.bass_and_drums);
        audioManager.changeMusicMood(MusicMood.slow_just_bass);
        //audioManager.changeMusicMoodAfterCurrentLoop(MusicMood.bass_drums_and_boopboop);
    }

    void setCanvasStatus(string canvasTag, bool newState, bool hideOthers=true) {
        foreach(GameObject thisCanvas in canvasses) {
            if (thisCanvas.tag=="ControlPanelCanvas") {
                thisCanvas.SetActive(true);
            }
            else if (thisCanvas.tag==canvasTag) {
                thisCanvas.SetActive(newState);
            }
            else if (hideOthers) {
                thisCanvas.SetActive(false);
            }
        }
    }

    public void toggleMenu() {
        menuContainer.SetActive(!menuContainer.activeSelf);
        //Might want to pause game
    }
    public void setMenuActive(bool active) {
        menuContainer.SetActive(active);
        //Might want to pause game
    }
}
