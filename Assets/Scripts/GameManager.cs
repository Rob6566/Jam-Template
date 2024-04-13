//Main game manager
//IMPORTANT - make sure you set a unique GAME_ID in HighScoreManager

using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public enum GameState {paused, picking_card, scoring, between_games};
public enum ScoringAnimation {about_to_start, fade_in, hand_fade_in, labels_fade_in, card_numbers_fade_in, card_1_score, card_2_score, card_3_score, card_4_score, card_5_score, hand_points_fade_in, hand_mult_fade_in, total_points_fade_in, total_x_fade_in, total_mult_fade_in, total_fade_in, fade_out, animate_score};

public class GameManager : MonoBehaviour {
    //Managers
    public AudioManager audioManager;
    public HighScoreManager highScoreManager;
    public CardManager cardManager;
    public AnimationManager animationManager;
    
    public List<GameObject> canvasses = new List<GameObject>();

    public List<GameObject> moveCardButtons = new List<GameObject>();
    
    public GameObject testCard;

    public GameObject enemyPrefab;

    public List<GameObject> enemyContainers = new List<GameObject>();
    public GameObject nemesis;
    public GameObject speechbubble;
    public TextMeshProUGUI speechText;


    public GameObject scoreOverlay;
    public TextMeshProUGUI scoreHandName;
    public TextMeshProUGUI scoreLblPoints;
    public TextMeshProUGUI scoreLblMult;
    public TextMeshProUGUI scoreLblCards;
    public TextMeshProUGUI scoreLblHand;
    public TextMeshProUGUI scoreCardPoints;
    public TextMeshProUGUI scoreCardMult;
    public TextMeshProUGUI scoreHandPoints;
    public TextMeshProUGUI scoreHandMult;
    public TextMeshProUGUI scoreTotalPoints;
    public TextMeshProUGUI scoreTotalMult;
    public TextMeshProUGUI scoreLblX;
    public TextMeshProUGUI scoreTotal;


    //Vars used for card scoring animation
    public List<Card> tempScoreCardsInHand;
    int tempScoreCardPoints;
    int tempScoreCardMult;
    int tempScoreHandPoints;
    int tempScoreHandMult;
    int tempScoreTotalPoints;
    int tempScoreTotal;
    int tempScoreHand;
    HandType tempScoreHandType;
    float tempScoreTimeSinceLastEvent=0f;
    float HUGE_ANIMATED_CARD_SIZE=.8f;
    ScoringAnimation scoringAnimation;


    private int score=0;
    private int hp=100;
    private const int START_HP=100;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI hpText;

    private List<ScoreHolder> scoreHolders = new List<ScoreHolder>();
    

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

        hideAllContainerImages();

        cardManager.dealAllCards();

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

        //if (animationManager.isAnimating()) {
        //    return;
       // }

        if (gameState==GameState.scoring) {
            updateScoring();            
        }
    }


    //Animate scoring a hand. Why did I do this?
    void updateScoring() {
        tempScoreTimeSinceLastEvent+=Time.deltaTime*animationSpeed;
        List<GameObject> handObjects;
        Debug.Log("Update scoring yo");
        //public enum ScoringAnimation {about_to_start, fade_in, hand_fade_in, labels_fade_in, card_numbers_fade_in, card_1_score, card_2_score, card_3_score, card_4_score, card_5_score, hand_points_fade_in, hand_mult_fade_in, total_points_fade_in, total_x_fade_in, total_mult_fade_in, total_fade_in, fade_out, animate_score};
        bool nextScoringEvent=false;
        switch (scoringAnimation) {
            case ScoringAnimation.about_to_start:
                nextScoringEvent=true;
            break;
            
            case ScoringAnimation.fade_in:
                scoreOverlay.SetActive(true);
                scoreOverlay.GetComponent<Image>().color=Color.Lerp(Color.clear, Color.black, tempScoreTimeSinceLastEvent/1f);
                if (scoreOverlay.GetComponent<Image>().color==Color.black) {
                    nextScoringEvent=true;
                }
            break;

            case ScoringAnimation.hand_fade_in:
                handObjects = (tempScoreHand==1 ? cardManager.hand1Containers : tempScoreHand==2 ? cardManager.hand2Containers : cardManager.hand3Containers);
                foreach (GameObject handObject in handObjects) {
                    handObject.transform.SetParent(scoreOverlay.transform);
                }
                if (tempScoreTimeSinceLastEvent>.5f) {nextScoringEvent=true;}
            break;

            case ScoringAnimation.labels_fade_in:
                scoreLblPoints.text="Points";
                scoreLblMult.text="Mult";
                scoreLblCards.text="Cards";
                scoreLblHand.text="Hand";
                if (tempScoreTimeSinceLastEvent>.5f) {nextScoringEvent=true;}
            break;

            case ScoringAnimation.card_numbers_fade_in:
                scoreCardPoints.text="";
                scoreCardMult.text="";
                scoreHandPoints.text="";
                scoreHandMult.text="";
                if (tempScoreTimeSinceLastEvent>.5f) {nextScoringEvent=true;}
            break;

            case ScoringAnimation.card_1_score:
            case ScoringAnimation.card_2_score:
            case ScoringAnimation.card_3_score:
            case ScoringAnimation.card_4_score:
            case ScoringAnimation.card_5_score:

                int cardNo = scoringAnimation-ScoringAnimation.card_1_score;

                float currentScale=Mathf.Lerp(cardManager.SMALL_CARD_SIZE, HUGE_ANIMATED_CARD_SIZE, tempScoreTimeSinceLastEvent/.5f);
                tempScoreCardsInHand[cardNo].CardUI.transform.localScale=new Vector3(currentScale, currentScale, currentScale);
                //scoreTotalPoints.text="";
                //scoreTotalMult.text="";
                //scoreLblX.text="";
                //scoreTotal.text="";
                //tempScoreCardsInHand[0].enableShader(CardShader.Glow);

                if(tempScoreTimeSinceLastEvent>.5f) {
                    tempScoreCardPoints+=(int)tempScoreCardsInHand[0].cardScore;
                    tempScoreCardMult+=(int)tempScoreCardsInHand[0].cardMult;
                    scoreCardPoints.text=tempScoreCardPoints.ToString();
                    scoreCardMult.text=tempScoreCardMult.ToString();
                    nextScoringEvent=true;
                    tempScoreCardsInHand[cardNo].CardUI.transform.localScale=new Vector3(cardManager.SMALL_CARD_SIZE, cardManager.SMALL_CARD_SIZE, cardManager.SMALL_CARD_SIZE);
                }
            break;

            break;

            case ScoringAnimation.hand_points_fade_in:
                scoreHandPoints.text=tempScoreHandPoints.ToString();
                if (tempScoreTimeSinceLastEvent>.5f) {nextScoringEvent=true;}
            break;

            case ScoringAnimation.hand_mult_fade_in:
                scoreHandMult.text=tempScoreHandMult.ToString();
                if (tempScoreTimeSinceLastEvent>.5f) {nextScoringEvent=true;}
            break;

            case ScoringAnimation.total_points_fade_in:
                scoreTotalPoints.text=(tempScoreCardPoints+tempScoreHandPoints).ToString();
                if (tempScoreTimeSinceLastEvent>.5f) {nextScoringEvent=true;}
            break;

            case ScoringAnimation.total_x_fade_in:
                scoreLblX.text="x";
                nextScoringEvent=true;
            break;

            case ScoringAnimation.total_mult_fade_in:
                scoreTotalMult.text=(tempScoreHandMult+tempScoreCardMult).ToString();
                if (tempScoreTimeSinceLastEvent>.5f) {nextScoringEvent=true;}
            break;

            case ScoringAnimation.total_fade_in:
                tempScoreTotal=((tempScoreCardPoints+tempScoreHandPoints)*(tempScoreHandMult+tempScoreCardMult));
                scoreTotal.text=tempScoreTotal.ToString();
                if (tempScoreTimeSinceLastEvent>2f) {nextScoringEvent=true;}
            break;

            case ScoringAnimation.fade_out:
                scoreOverlay.SetActive(true);
                scoreOverlay.GetComponent<Image>().color=Color.Lerp(Color.black, Color.clear, tempScoreTimeSinceLastEvent/1f);
                if (scoreOverlay.GetComponent<Image>().color==Color.clear) {
                    scoreOverlay.SetActive(false);
                    nextScoringEvent=true;
                    handObjects = (tempScoreHand==1 ? cardManager.hand1Containers : tempScoreHand==2 ? cardManager.hand2Containers : cardManager.hand3Containers);
                    foreach (GameObject handObject in handObjects) {
                        handObject.transform.SetParent(cardManager.handContainers[tempScoreHand-1].transform);
                    }
                }
            break;

            case ScoringAnimation.animate_score:
                nextScoringEvent=true;
                score+=tempScoreTotal;
                scoreHolders[(int)tempScoreHandType].numberOfTimesScored++;
                scoreHolders[(int)tempScoreHandType].scoreGainedFromType+=tempScoreTotal;
                gameState=GameState.picking_card;
                cardManager.discardHand(tempScoreHand);
                cardManager.dealNextUpCards();
                tempScoreCardPoints=0;
                tempScoreCardMult=0;
                tempScoreHandPoints=0;
                tempScoreHandMult=0;
                tempScoreTotalPoints=0;
                tempScoreTotal=0;
                tempScoreHand=0;

                 
                    /*TODO if (monsterKilled) {
                        scoreHolders[(int)handType].monstersKilledWithHandType++;
                     }*/
                updateUI();
            break;
        }

        if (nextScoringEvent) {
            tempScoreTimeSinceLastEvent=0;
            scoringAnimation++;
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

    public void hideAllContainerImages() {
        foreach(GameObject enemy in enemyContainers) {
            Image enemyImage = enemy.GetComponent<Image>();
            enemyImage.enabled=false;
        }
        scoreOverlay.SetActive(false);
        cardManager.hideAllContainerImages();
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

        hp=START_HP;
        initScore();
        

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

    public void draftCardToHand1(int cardPicked) {
        cardManager.draftCardToHand(1, cardPicked);
    }
    
    public void draftCardToHand2(int cardPicked) {
        cardManager.draftCardToHand(2, cardPicked);
    }

    public void draftCardToHand3(int cardPicked) {
        cardManager.draftCardToHand(3, cardPicked);
    }

    void initScore() {
        score=0;
        scoreHolders.Clear();
        for(int i=0; i<Enum.GetNames(typeof(HandType)).Length; i++) {
            ScoreHolder scoreHolder = new ScoreHolder();
            scoreHolder.handType=(HandType)i;
            scoreHolders.Add(scoreHolder);
        }
        updateUI();
    }

    public void scoreHand(int handNo, HandSO handSO, List<Card> cardsInHand) {

        gameState=GameState.scoring;
        scoringAnimation=ScoringAnimation.about_to_start;
        tempScoreTimeSinceLastEvent=0f;
        tempScoreCardsInHand=cardsInHand;

        tempScoreHandPoints=(int)handSO.score;
        tempScoreHandMult=(int)handSO.mult;
        tempScoreHandType=handSO.handType;

        scoreHandName.text=handSO.handName;
        scoreLblPoints.text="";
        scoreLblMult.text="";
        scoreLblCards.text="";
        scoreLblHand.text="";
        scoreCardPoints.text="";
        scoreCardMult.text="";
        scoreHandPoints.text="";
        scoreHandMult.text="";
        scoreTotalPoints.text="";
        scoreTotalMult.text="";
        scoreLblX.text="";
        scoreTotal.text="";
        tempScoreHand=handNo;
    }

    public void updateUI() {
        scoreText.text="Score: "+score.ToString();
        hpText.text=hp.ToString();
    }
}

