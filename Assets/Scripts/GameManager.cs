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
public enum ScoringAnimation {about_to_start, fade_in,hand_fade_in, hand_type_fade_in, labels_fade_in, card_numbers_fade_in, card_1_score, card_2_score, card_3_score, card_4_score, card_5_score, hand_points_fade_in, hand_mult_fade_in, total_points_fade_in, total_x_fade_in, total_mult_fade_in, total_fade_in, fade_out, animate_score};

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

    public List<Enemy> enemies = new List<Enemy>();

    [SerializeField]
    List<EnemySO> enemySOs;

    public GameObject nemesis;
    public GameObject speechbubble;
    public TextMeshProUGUI speechText;
    int hideEnemySpeechInXRounds=0;
    List<string> nemesisSpeeches = new List<string>{
        "You fold faster than a cheap tent in a windstorm.",
        "I summon monstrosities from your darkest nightmares, and you deign to challenge me with little bits of cardboard?",
        "You might want to stick to rock-paper-scissors.",
        "We're both rulebreakers. I tear down the barriers between dimensions. You cheat at internet card games.",
        "I've got more trump support than a MAGA rally.",
        "Dealer! Are they allowed to do that?",
        "I saw you pull that ace out of your sleeve."
    };

    //Scoring Overlay
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

    
    //End Game Overlay
    public GameObject endGameFirstOverlay;
    public GameObject endGameSecondOverlay;
    public GameObject endGameTXTGameOver;
    public GameObject endGameTXTScore;
    public GameObject endGameLBLHandsPlayed;
    public GameObject endGameTXTHandsPlayed;
    public GameObject endGameLBLCardsDrafted;
    public GameObject endGameTXTCardsDrafted;
    public GameObject endGameLBLEnemiesUnsummoned;
    public GameObject endGameTXTEnemiesUnsummoned;
    public GameObject endGameLBLMostPlayedHand;
    public GameObject endGameTXTMostPlayedHand;
    public GameObject endGameHomeButton;


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
    private int turnUpto=0;
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
        //public enum ScoringAnimation {about_to_start, fade_in, hand_type_fade_in, hand_fade_in, labels_fade_in, card_numbers_fade_in, card_1_score, card_2_score, card_3_score, card_4_score, card_5_score, hand_points_fade_in, hand_mult_fade_in, total_points_fade_in, total_x_fade_in, total_mult_fade_in, total_fade_in, fade_out, animate_score};
        bool nextScoringEvent=false;
        switch (scoringAnimation) {
            case ScoringAnimation.about_to_start:
                nextScoringEvent=true;
                setDraftButtonsActive(false);
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

            case ScoringAnimation.hand_type_fade_in:
                scoreHandName.gameObject.SetActive(true);
                scoreHandName.GetComponent<CanvasGroup>().alpha=Mathf.Lerp(0f, 1f, tempScoreTimeSinceLastEvent/1f);
                if (tempScoreTimeSinceLastEvent>1f) {
                    nextScoringEvent=true;
                }
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
                if (tempScoreTimeSinceLastEvent>.5f) {
                    nextScoringEvent=true; 
                    audioManager.playSound(GameSound.deal, 1f);
                }
            break;

            case ScoringAnimation.card_1_score:
            case ScoringAnimation.card_2_score:
            case ScoringAnimation.card_3_score:
            case ScoringAnimation.card_4_score:
            case ScoringAnimation.card_5_score:

                int cardNo = scoringAnimation-ScoringAnimation.card_1_score;

                float currentScale=Mathf.Lerp(cardManager.SMALL_CARD_SIZE, HUGE_ANIMATED_CARD_SIZE, tempScoreTimeSinceLastEvent/.5f);
                tempScoreCardsInHand[cardNo].CardUI.transform.localScale=new Vector3(currentScale, currentScale, currentScale);

                if(tempScoreTimeSinceLastEvent>.5f) {
                    tempScoreCardPoints+=(int)tempScoreCardsInHand[cardNo].cardScore;
                    tempScoreCardMult+=(int)tempScoreCardsInHand[cardNo].cardMult;
                    scoreCardPoints.text=tempScoreCardPoints.ToString();
                    scoreCardMult.text=tempScoreCardMult.ToString();
                    nextScoringEvent=true;
                    tempScoreCardsInHand[cardNo].CardUI.transform.localScale=new Vector3(cardManager.SMALL_CARD_SIZE, cardManager.SMALL_CARD_SIZE, cardManager.SMALL_CARD_SIZE);
                    if (cardNo<4) {
                        audioManager.playSound(GameSound.deal, 1f);
                    }
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
                setDraftButtonsActive(true);

                List<Enemy> enemiesToRemove = new List<Enemy>();
                foreach (Enemy enemy in enemies) {
                    if ((enemy.enemyPosition+1)!=tempScoreHand) {
                        continue;
                    }
                   enemy.HP-=tempScoreTotal;
                   enemy.updateUI();
                   if (enemy.HP>0) {
                    enemy.animateHP();
                   }
                   if (enemy.HP<=0) {
                      enemiesToRemove.Add(enemy);
                   }
                   break;
                }

                foreach (Enemy enemyToRemove in enemiesToRemove) {
                    animationManager.animateObjectExpandAndFade(enemyToRemove.EnemyUI, 1f, 5f);
                    enemies.Remove(enemyToRemove);
                    enemyToRemove.Destroy();
                    scoreHolders[(int)tempScoreHandType].monstersKilledWithHandType++;
                }
                tempScoreHand=0;
                tempScoreTotal=0;
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
        endGameFirstOverlay.SetActive(false);
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
        turnUpto=0;
        initScore();
        speechbubble.SetActive(false);
        cardManager.dealAllCards();
        

        setCanvasStatus("GameCanvas", true);
        setCanvasStatus("ControlPanelCanvas", true, false);
        audioManager.changeMusicMood(MusicMood.bass_and_drums);
        //audioManager.changeMusicMood(MusicMood.slow_just_bass);
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

    public void setDraftButtonsActive(bool active) {
        foreach(GameObject button in moveCardButtons) {
            button.GetComponent<Button>().interactable=active;
        }
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
        scoreHandName.gameObject.SetActive(false);
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

    //Tick down the counters on enemies. We don't tick down the one in the hand played
    public void tickDownEnemies(int enemyToExclude) {
        turnUpto++;
        spawnEnemies();
        List<Enemy> enemiesToRemove = new List<Enemy>();
        Debug.Log("Tick down enemies, excluding "+enemyToExclude);
        foreach (Enemy enemy in enemies) {
            if (enemy.enemyPosition==enemyToExclude) {
                Debug.Log("Tick down - skipped enemy "+enemy.enemyPosition);
                continue;
            }
            enemy.turnsUntilAttack--;
            enemy.updateUI();
            enemy.animateTimer();
            if (enemy.turnsUntilAttack<=0) {
                //TODO - animate
                hp-=enemy.HP;
                updateUI();
                checkIfLostGame();
                enemiesToRemove.Add(enemy);
            }
        }
        foreach (Enemy enemyToRemove in enemiesToRemove) {
            enemies.Remove(enemyToRemove);
            enemyToRemove.Destroy();
        }



        //Enemy Speech
        hideEnemySpeechInXRounds--;
        if (hideEnemySpeechInXRounds<0) {
            speechbubble.SetActive(false);
        }
        if(hideEnemySpeechInXRounds<-3 && UnityEngine.Random.Range(0, 100)>80) {
            hideEnemySpeechInXRounds=2;
            speechbubble.SetActive(true);
            speechText.text=nemesisSpeeches[UnityEngine.Random.Range(0, nemesisSpeeches.Count)];
            animationManager.animateObjectExpandAndFade(speechbubble, .2f, 1.5f);
        }
    }

    public void spawnEnemies() {
        if (enemies.Count>2) {
            return;
        }

        if (enemies.Count>0 && UnityEngine.Random.Range(0, 100)>30) {
            return;
        }

        for (int i=0; i<3; i++) {
            bool enemyAlreadyThere=false;
            foreach(Enemy enemy in enemies) {
                if (enemy.enemyPosition==(i)) {
                    enemyAlreadyThere=true;
                    break;
                }
            }
            if (!enemyAlreadyThere) {
                //TODO - scale enemy difficulty
                GameObject enemyObject = Instantiate(enemyPrefab);
                enemyObject.transform.SetParent(enemyContainers[i].transform);
                enemyObject.transform.localPosition=Vector3.zero;
                enemyObject.transform.localScale=new Vector3(1.4f, 1.4f, 1.4f);
                
                EnemySO enemySO = getRandomEnemy();
                
                Enemy enemy = new Enemy();
                enemy.init(this, enemySO, enemyObject, i);
                enemies.Add(enemy);
                break;
            }
        }

    }

    private EnemySO getRandomEnemy() {
        EnemySO enemy = enemySOs[UnityEngine.Random.Range(0, enemySOs.Count)];
        if (enemy.spawnAfterRound>turnUpto) {
            return getRandomEnemy(); //Yay infinite loop
        }
        return enemy;
    }

    //TODO - implement
    public void checkIfLostGame() {
        if (hp<=0) {
            gameState=GameState.between_games;
            endGameFirstOverlay.SetActive(true);
            endGameTXTScore.GetComponent<TextMeshProUGUI>().text=score+" points";
            endGameTXTCardsDrafted.GetComponent<TextMeshProUGUI>().text=turnUpto.ToString();

            int enemiesUnsummoned=0;
            HandType mostPlayedHand=HandType.high_card;
            int mostPlayedHandCount=0;
            int handsPlayed=0;
            foreach(ScoreHolder scoreHolder in scoreHolders) {
                enemiesUnsummoned+=scoreHolder.monstersKilledWithHandType;
                handsPlayed+=scoreHolder.numberOfTimesScored;
                if (scoreHolder.numberOfTimesScored>mostPlayedHandCount) {
                    mostPlayedHand=scoreHolder.handType;
                    mostPlayedHandCount=scoreHolder.numberOfTimesScored;
                }
            }

            endGameTXTHandsPlayed.GetComponent<TextMeshProUGUI>().text=handsPlayed.ToString();
            endGameTXTEnemiesUnsummoned.GetComponent<TextMeshProUGUI>().text=enemiesUnsummoned.ToString();
            endGameTXTMostPlayedHand.GetComponent<TextMeshProUGUI>().text=cardManager.getHandTypeName(mostPlayedHand);

            audioManager.changeMusicMood(MusicMood.slow_just_bass);

            StartCoroutine(highScoreManager.SaveScore(score));
        }
    }

    public void clickHome() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

