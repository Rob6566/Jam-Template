//Main game manager
//IMPORTANT - make sure you set a unique GAME_ID in HighScoreManager

using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public enum GameState {paused, picking_card, scoring, between_games, tutorial, boss};
public enum ScoringAnimation {about_to_start, fade_in,hand_fade_in, hand_type_fade_in, labels_fade_in, card_numbers_fade_in, card_1_score, card_2_score, card_3_score, card_4_score, card_5_score, hand_points_fade_in, hand_mult_fade_in, total_points_fade_in, total_x_fade_in, total_mult_fade_in, total_fade_in, fade_out, animate_score};

public class GameManager : MonoBehaviour {

    //Managers
    public AudioManager audioManager;
    public HighScoreManager highScoreManager;
    public CardManager cardManager;
    public AnimationManager animationManager;
    public TutorialManager tutorialManager;
    public GameLengthManager gameLengthManager;
    
    public List<GameObject> canvasses = new List<GameObject>();

    public List<GameObject> moveCardButtons = new List<GameObject>();
    
    public GameObject testCard;

    public GameObject enemyPrefab;

    public List<GameObject> enemyContainers = new List<GameObject>();

    public List<Enemy> enemies = new List<Enemy>();

    //Text Mesh Pro checkbox for skipping tutorial
    public Toggle skipTutorialToggle;

    //Panels with "Select Name" and "Game Speed" on splash
    public List<GameObject> startGamePanels = new List<GameObject>();


    //Boss stuff
    public GameObject bossObject;
    public GameObject bossAttackContainer;
    public GameObject bossFinalSpeech;
    public GameObject bossHighlighter;
    Transform bossHighligherParent;
    Vector3 bossHighlighterPosition;
    Vector3 bossHighlighterScale;
    

    public EnemySO bossSO;
    Vector3 bossScale;
    Vector3 bossPosition;
    Transform bossContainer;


    public GameObject speechbubble;
    public TextMeshProUGUI speechText;
    int hideEnemySpeechInXRounds=0;
    List<string> nemesisSpeeches = new List<string>{
        "You fold faster than a cheap tent in a windstorm.",
        "I summon monstrosities from your darkest nightmares, and you deign to challenge me with little bits of cardboard?",
        "You might want to stick to rock-paper-scissors.",
        "We're both rule breakers. I tear down the barriers between dimensions. You cheat at internet card games.",
        "Dealer! Are they allowed to do that?",
        "I saw you pull that ace out of your sleeve.",
        "This ain't Texas<br>(sung in a gravelly voice)",
        "I've upgraded my minions. Pray I don't upgrade them further.",
        "I solemnly swear I am up to no good.",
        "Give a man a fire and he's warm for a day, but set fire to him and he's warm for the rest of his life.",
        "There are no stupid answers, just stupid people.",
        "Trying is the first step towards failure.",
        "If you think this has a happy ending, you haven't been paying attention.",
        "When you play Arcane Ante™, you win or you die.",
        "I'm coming for you.<br>Boom, phrasing.",
        "Goood. Let the hate flow through you.",
        "Did you ever hear the tragedy of Darth Plagueis the Wise?",
        "Good vibes only."
    };

    List<string> bluortSpeeches = new List<string>{
        "Smile, you're on camera",
        "You didn't buy these from me",
        "These fell off the back of a truck",
        "Buy 1 get 1 - and that's cutting my own throat",
        "If you see one-eyed Margaret from Marketing, give her one from me",
        "I can't fix your poker face, but watch his when you play 5 Aces",
        "You break it, you bought it",
        "You wouldn't steal a car",
        "No refunds or exchanges",
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


    
    //End Game Overlay;
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

    //Deck Overlay
    public GameObject deckOverlay;

    //Shop controls
    public GameObject shopButton;
    public GameObject shopOverlay;
    public List<GameObject> shopCardContainers = new List<GameObject>();
    public List<GameObject> shopCardHighlighters = new List<GameObject>();
    public List<GameObject> shopBuffContainers = new List<GameObject>();
    public TextMeshProUGUI shopVouchersTXT;
    public TextMeshProUGUI shopSpeechTXT;
    public GameObject shopPurchaseButton;
    public GameObject shopSkipButton;
    List<Card> cardsInShop = new List<Card>();
    List<CardBuffSO> buffsInShop = new List<CardBuffSO>();

    float animatePendingAttackTimer=0f;
    bool animatePendingAlternator=false;


    //Vars used for card scoring animation
    public List<Card> tempScoreCardsInHand;
    int tempScoreCardPoints;
    int tempScoreCardMult;
    int tempScoreHandPoints;
    int tempScoreHandMult;
    int tempScoreTotalPoints;
    int tempScoreTotal;
    int tempScoreHand;
    bool tempFirstTimeProcessingScoringAnimation=true;
    HandType tempScoreHandType;
    float tempScoreTimeSinceLastEvent=0f;
    float HUGE_ANIMATED_CARD_SIZE=.8f;
    ScoringAnimation scoringAnimation;


    private int score=0;
    private int nextShopScore=0;
    private int shopIncrement=0;    
    private int shopUsesAvailable=0;
    private int hp=200;
    private int turnUpto=0;
    private const int START_HP=200;
    private const int MAX_TURNS=100;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI turnText;
    private List<int> shopCardSelected=new List<int>();
    private int shopBuffSelected=-1;

    private bool runningTutorial;
    private bool animatingScoring=false;

    //Boss
    public GameObject bossHP;
    public GameObject bossTimer;


    private List<ScoreHolder> scoreHolders = new List<ScoreHolder>();

    public GameObject splashScoreHolder;
    public GameObject endGameScoreHolder;
    

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

        bossScale=bossObject.transform.localScale;
        bossPosition=bossObject.transform.localPosition;
        bossContainer=bossObject.transform.parent;
        Debug.Log("iiii Setup Boss parent="+bossContainer.gameObject.name+" scale="+bossScale);

        bossHighligherParent=bossHighlighter.transform.parent;
        bossHighlighterPosition=bossHighlighter.transform.localPosition;
        bossHighlighterScale=bossHighlighter.transform.localScale;

        //Link up managers
        audioManager=GameObject.FindWithTag("AudioManager").GetComponent<AudioManager>();   
        animationManager=GameObject.FindWithTag("AnimationManager").GetComponent<AnimationManager>();
        animationManager.init(this);   
        highScoreManager=GameObject.FindWithTag("HighScoreManager").GetComponent<HighScoreManager>();   
        highScoreManager.init();
        tutorialManager=GameObject.FindWithTag("TutorialManager").GetComponent<TutorialManager>();   
        tutorialManager.init(this);
        gameLengthManager=GameObject.FindWithTag("GameLengthManager").GetComponent<GameLengthManager>();   
        gameLengthManager.init(this);
        
        
        cardManager=GameObject.FindWithTag("CardManager").GetComponent<CardManager>();   
        cardManager.init(this);

        setCanvasStatus("SplashCanvas", true);

        setMenuActive(false);

        setAnimationSpeed();

        setStartGamePanel(0);

        //Play music
        audioManager.initAdaptiveMusic(); //fadeInMusic(0, 0, 1f);

        hideAllContainerImages();

        //Load high scores
        gameLengthManager.loadHighScores();
        audioManager.changeMusicMood(MusicMood.intro);
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

        if (animatingScoring) {
            updateScoring();            
        }

        animatePendingAttackTimer+=Time.deltaTime;
        if (animatePendingAttackTimer>1.5f && !showingOverlay()) {
            animatePendingAlternator=!animatePendingAlternator;
            foreach (Enemy enemy in enemies) {
                if (enemy.turnsUntilAttack<3) { //Animate faster if attack is imminent
                    enemy.animateTimer();
                }
                else if (enemy.turnsUntilAttack<5 && animatePendingAlternator) {
                    enemy.animateTimer();
                }
            }
            animatePendingAttackTimer=0f;
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
                if (gameState!=GameState.boss) {
                    audioManager.changeMusicMood(getGameMood(true));
                }
                animationManager.deleteAllAnimations();
                
            break;

            case ScoringAnimation.fade_in:
                scoreOverlay.SetActive(true);
                Color black=new Color(0f, 0f, 0f, .93f);
                scoreOverlay.GetComponent<Image>().color=Color.Lerp(Color.clear, black, tempScoreTimeSinceLastEvent/1f);
                if (scoreOverlay.GetComponent<Image>().color==black) {
                    nextScoringEvent=true;
                }
            break;

            case ScoringAnimation.hand_fade_in:
                //Move hand above overlay
                handObjects = (tempScoreHand==1 ? cardManager.hand1Containers : tempScoreHand==2 ? cardManager.hand2Containers : cardManager.hand3Containers);
                foreach (GameObject handObject in handObjects) {
                    handObject.transform.SetParent(scoreOverlay.transform);
                }

                //Move enemy above overlay
                foreach(Enemy thisEnemy in enemies) {
                    if (thisEnemy.enemyPosition==(tempScoreHand-1)) {
                        thisEnemy.EnemyUI.transform.SetParent(scoreOverlay.transform);
                    }
                }
                bossObject.transform.SetParent(scoreOverlay.transform);
                bossObject.transform.SetAsFirstSibling();
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
                    audioManager.playSound(GameSound.score, 1f, 0);
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

                if (tempFirstTimeProcessingScoringAnimation) {
                    tempScoreCardPoints+=(int)tempScoreCardsInHand[cardNo].CardScore;
                    tempScoreCardMult+=(int)tempScoreCardsInHand[cardNo].CardMult;
                    scoreCardPoints.text=tempScoreCardPoints.ToString();
                    scoreCardMult.text=tempScoreCardMult.ToString();
                }

                if(tempScoreTimeSinceLastEvent>.4f) {
                    nextScoringEvent=true;
                    tempScoreCardsInHand[cardNo].CardUI.transform.localScale=new Vector3(cardManager.SMALL_CARD_SIZE, cardManager.SMALL_CARD_SIZE, cardManager.SMALL_CARD_SIZE);
                    if (cardNo<4) {
                        audioManager.playSound(GameSound.score, 1f, cardNo+1);
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
                cardManager.discardHand(tempScoreHand);
                tempScoreCardPoints=0;
                tempScoreCardMult=0;
                tempScoreHandPoints=0;
                tempScoreHandMult=0;
                tempScoreTotalPoints=0;
                setDraftButtonsActive(true);
                checkShopVoucherThreshold();

                List<Enemy> enemiesToRemove = new List<Enemy>();
                foreach (Enemy enemy in enemies) {
                    if (gameState!=GameState.boss && (enemy.enemyPosition+1)!=tempScoreHand) {
                        continue;
                    }
                   enemy.EnemyUI.transform.SetParent(enemyContainers[enemy.enemyPosition].transform);
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
                    enemies.Remove(enemyToRemove);
                    scoreHolders[(int)tempScoreHandType].monstersKilledWithHandType++;
                    if (gameState==GameState.boss) {
                        scoringAnimation++;
                        winGame();
                        return;
                    }
                    //animationManager.animateObjectExpandAndFade(enemyToRemove.EnemyUI, 1f, 5f);
                    animationManager.animateObject(AnimationType.expandAndFade, enemyToRemove.EnemyUI, null, null, 5f, 1f, 1f, 0f);
                    //AnimationType animationType, GameObject gameObject, GameObject newParentObject?, Vector3? targetPosition, Vector3 endScale, float totalTime, float startAlpha=1f, float endAlpha=1f, bool destroyOnFinish=true, bool cloneObject=true
                    enemyToRemove.Destroy();
                }

                bossObject.transform.SetParent(bossAttackContainer.transform);

                tempScoreHand=0;
                tempScoreTotal=0;
                updateUI();
                playGameMusic(true);
                cardManager.dealNextUpCards();
                Debug.Log("Show Shop 1");
                showShopIfAvailable();
                animatingScoring=false;
            break;
        }

        tempFirstTimeProcessingScoringAnimation=false;
        if (nextScoringEvent) {
            tempScoreTimeSinceLastEvent=0;
            scoringAnimation++;
            tempFirstTimeProcessingScoringAnimation=true;;
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
        foreach(GameObject cardContainer in shopCardContainers) {
            cardContainer.GetComponent<Image>().enabled=false;
        }
        scoreOverlay.SetActive(false);
        deckOverlay.SetActive(false);
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
        audioManager.changeMusicMood(MusicMood.start_game);
        //setCanvasStatus("GameSpeedCanvas", true);
        setStartGamePanel(1);
    }

    void setStartGamePanel(int panelIndex) {
        int i=0;
        foreach(GameObject thisPanel in startGamePanels) {
            thisPanel.SetActive(i==panelIndex);
            i++;
        }
    }


    public void startGameWithSpeed(int gameSpeed) {

        audioManager.changeMusicMood(MusicMood.start_game);

        gameLengthManager.setGameLength(gameSpeed);

        hp=START_HP;
        turnUpto=0;
        initScore();
        speechbubble.SetActive(false);
        shopOverlay.SetActive(false);
        bossHP.SetActive(false);
        bossTimer.SetActive(false);
        bossFinalSpeech.SetActive(false);
        bossHighlighter.SetActive(false);
        bossHighlighter.transform.localScale=new Vector3(5f, 5f, 5f);

        animatePendingAttackTimer=0f;

        setCanvasStatus("GameCanvas", true);
        Debug.Log("uuuu game canvas");
        tutorialManager.resetTutorialStep();
        if (skipTutorialToggle.isOn) {
            Debug.Log("STARTGAME No tutorial");
            audioManager.changeMusicMoodAfterCurrentLoop(MusicMood.game);
            tutorialManager.skipTutorial();
        }
        else {
            Debug.Log("STARTGAME Tutorial running");
            gameState=GameState.tutorial;
            audioManager.changeMusicMoodAfterCurrentLoop(MusicMood.tutorial);
            runningTutorial=true;
            tutorialManager.startTutorial();
        }

        cardManager.dealAllCards();

        //setCanvasStatus("ControlPanelCanvas", true, true);

        animationManager.deleteAllAnimations();
        

        updateUI();
    }


    public void setActiveCanvas(string canvasTag) {
        Debug.Log("Set Active Canvas "+canvasTag);
        if (canvasTag=="GameCanvas") {
            Debug.Log("PlaygameMusic mood gamecanvas");
            playGameMusic();
        }
        setCanvasStatus(canvasTag, true);
    }

    public void endTutorial(bool beforeGotFreeUpgrade) {
        if (beforeGotFreeUpgrade) {
            shopUsesAvailable++;
            Debug.Log("Show Shop 2");
            showShopIfAvailable();
        }
        gameState=GameState.picking_card;
        runningTutorial=false;
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
        shopIncrement=100; 
        nextShopScore=0;
        shopUsesAvailable=-1;
        scoreHolders.Clear();
        checkShopVoucherThreshold();
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

        animatingScoring=true;
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
        scoreText.text="Score<br><b>"+score.ToString()+"</b>";
        hpText.text="HP<br><b>"+hp.ToString()+"</b>";

        shopButton.SetActive(shopUsesAvailable>0);
        shopVouchersTXT.text="Shop vouchers: "+(shopUsesAvailable+1).ToString();
        turnText.text="Turn<br><b>"+turnUpto.ToString()+"/"+gameLengthManager.GameLengthSO.BOSS_SPAWN_TURN.ToString()+"</b>";
    }

    //Tick down the counters on enemies. We don't tick down the one in the hand played
    public void tickDownEnemies(int enemyToExclude) {
        if (runningTutorial) {
            return;
        }

        turnUpto++;
        bool spawningBoss=false;
        if (turnUpto>=gameLengthManager.GameLengthSO.BOSS_SPAWN_TURN && gameState!=GameState.boss) {
            gameState=GameState.boss;
            audioManager.changeMusicMood(MusicMood.boss_intro);
            Invoke("startBossMusicLoop", 3f);
            Invoke("finishBossAnimation", 8f);
            speechbubble.SetActive(false);
            bossFinalSpeech.SetActive(true);
            cardManager.unHoveredButton();
            cardManager.deckContainer.SetActive(false);

            Enemy boss = new Enemy();
            boss.init(this, bossSO, bossObject, 1);
            boss.HP=gameLengthManager.GameLengthSO.BOSS_START_HP;
            boss.turnsUntilAttack=gameLengthManager.GameLengthSO.BOSS_START_TIMER;
            boss.updateUI();

            foreach (Enemy enemy in enemies) {
                enemy.HP=0;
            }
            enemies.Add(boss);
            spawningBoss=true;
        }


        spawnEnemies();
        List<Enemy> enemiesToRemove = new List<Enemy>();
        Debug.Log("Tick down enemies, excluding "+enemyToExclude);
        foreach (Enemy enemy in enemies) {
            if (gameState==GameState.boss || (enemy.enemyPosition!=enemyToExclude && enemy.HP>0)) {
                enemy.turnsUntilAttack--;
                enemy.animateTimer();
            }
            
            enemy.updateUI();
            if (enemy.turnsUntilAttack<=0 && enemy.HP>0) {
                //TODO - animate

                hp-=enemy.HP;
                enemy.HP=0;
                animationManager.animateObjectExpandAndFade(enemy.EnemyUI, 1f, 5f);
                updateUI();
                if (checkIfLostGame()) {
                    return;
                }
            }
            if (enemy.HP<=0) {
                enemiesToRemove.Add(enemy);
            }
        }

        foreach (Enemy enemyToRemove in enemiesToRemove) {
            enemies.Remove(enemyToRemove);
            enemyToRemove.Destroy();
        }

        if(spawningBoss) {
            bossHP.SetActive(true);
            bossTimer.SetActive(true);
            Invoke("startBossAnimation", 6f);
            tutorialManager.buttons.SetActive(false);   
            //animationManager.animateObject(bossObject, bossAttackContainer, new Vector3(610.5352f+96f-140f/*755f*//*-101f*/, 487.7401f/*900f*//*-855.414f*/, 0f), new Vector3(2.5f, 2.5f, 2.5f), 5f);
            bossHighlighter.SetActive(true);
            
            animationManager.animateObjectExpandAndFade(bossHighlighter, 3f, .18f, 1f, 1f, false, false);
        }

        //Switch music intensity if needed
        playGameMusic();


        //Enemy Speech
        if (gameState!=GameState.boss) {
            hideEnemySpeechInXRounds--;
            if (hideEnemySpeechInXRounds<0) {
                speechbubble.SetActive(false);
            }
            if(hideEnemySpeechInXRounds<-3 && UnityEngine.Random.Range(0, 100)>50) {
                hideEnemySpeechInXRounds=2;
                speechbubble.SetActive(true);
                speechText.text=nemesisSpeeches[UnityEngine.Random.Range(0, nemesisSpeeches.Count)];
            }
            Debug.Log("Show Shop 3");
            showShopIfAvailable();
        }


        updateUI();
    }

    void startBossMusicLoop() {
        audioManager.changeMusicMoodAfterCurrentLoop(MusicMood.boss_loop);
    }

    void startBossAnimation() {
        //AnimationType animationType, GameObject gameObject, GameObject? newParentObject, Vector3? targetPosition, float endScale, float totalTime, float startAlpha=1f, float endAlpha=1f, bool destroyOnFinish=true, bool cloneObject=true
        animationManager.animateObject(AnimationType.move, bossObject, bossAttackContainer, new Vector3(610.5352f+96f-140f/*755f*//*-101f*/, 487.7401f/*900f*//*-855.414f*/, 0f), 2.5f, 2f, 1f, 1f, false, false);
        animationManager.animateObjectExpandAndFade(bossHighlighter, 2f, 10f, 1f, 1f, false, false);
        bossFinalSpeech.SetActive(false);
    }

    void finishBossAnimation() {
        tutorialManager.buttons.SetActive(true);   
        bossHighlighter.SetActive(false);
        cardManager.deckContainer.SetActive(true);
    }

    public void showShopIfAvailable() {
        if (shopUsesAvailable>0 && gameState!=GameState.boss && shopOverlay.activeSelf==false) {
            clickShop();
        }
    }

    public void spawnEnemies() {
        if (enemies.Count>2 || gameState==GameState.boss) {
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
                
                EnemySO enemySO = gameLengthManager.getRandomEnemy(turnUpto);
                
                Enemy enemy = new Enemy();
                enemy.init(this, enemySO, enemyObject, i);
                enemies.Add(enemy);
                break;
            }
        }

    }

    public void winGame() {
        audioManager.changeMusicMood(MusicMood.victory);
        endGameTXTGameOver.GetComponent<TextMeshProUGUI>().text="<color=green>Victory!</color>";
        endGame();   
    }

    public bool checkIfLostGame() {
        if (hp<=0) {
            endGameTXTGameOver.GetComponent<TextMeshProUGUI>().text="<color=#ffffff>Game Over</color>";
            audioManager.changeMusicMood(MusicMood.dead);
            endGame();

            return true;
        }
        return false;
    }

    private void endGame() {
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

        bossObject.transform.SetParent(bossContainer.transform);
        bossObject.transform.localPosition=bossPosition;

        bossHighlighter.transform.SetParent(bossHighligherParent);
        bossHighlighter.transform.localPosition=bossHighlighterPosition;
        bossHighlighter.transform.localScale=bossHighlighterScale;
        

        StartCoroutine(highScoreManager.SaveScore(score, gameLengthManager.GameLengthSO.HIGH_SCORE_GAME_ID));
        Invoke("loadHighScoresAtEndOfGame", 2f); //Load scores after score is saved

    }

    public void clickHome() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    //Reset game state
    public void clickRetry() {
        skipTutorialToggle.isOn=true;
        cardManager.resetDeck();     
        hideAllContainerImages(); 
        destroyAllEnemies();  
        startGameWithSpeed((int)gameLengthManager.GameLength);
        //audioManager.changeMusicMood(MusicMood.intro);
    }

    public void destroyAllEnemies() {
        List<Enemy> enemiesToRemove = new List<Enemy>();
        foreach (Enemy enemy in enemies) {
            enemiesToRemove.Add(enemy);
        }
        foreach (Enemy enemyToRemove in enemiesToRemove) {
            enemies.Remove(enemyToRemove);
            enemyToRemove.Destroy();
        }
        enemies.Clear();
    }

    public void clickShop() {
        shopOverlay.SetActive(true);
        shopSpeechTXT.text= bluortSpeeches[UnityEngine.Random.Range(0, bluortSpeeches.Count)];
        shopUsesAvailable--;
        shopPurchaseButton.GetComponent<Button>().interactable=false;
        enableAllShopObjects();

        foreach(GameObject cardContainer in shopCardContainers) {
            Card card = cardManager.drawCard();
            card.setZone(CardZone.shop);
            cardsInShop.Add(card);
            card.CardUI.transform.SetParent(cardContainer.transform);
            card.CardUI.transform.localPosition=Vector3.zero;
            card.CardUI.transform.localScale=new Vector3(cardManager.SMALL_CARD_SIZE, cardManager.SMALL_CARD_SIZE, cardManager.SMALL_CARD_SIZE);
        }

        foreach(GameObject shopBuffContainer in shopBuffContainers) {
            CardBuffSO cardBuff = getUniqueBuff(buffsInShop);
            buffsInShop.Add(cardBuff);
            shopBuffContainer.transform.GetChild(0).GetComponent<Image>().sprite=cardBuff.sprite;
            shopBuffContainer.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text=cardBuff.buffName;
        }

        if (!runningTutorial) {
            audioManager.changeMusicMood(MusicMood.shop);
        }

        updateUI();
        
    }

    public CardBuffSO getUniqueBuff(List<CardBuffSO> existingBuffs) {
        CardBuffSO cardBuff = cardManager.getRandomBuff();
        foreach(CardBuffSO buff in existingBuffs) {
            if (buff.buffName==cardBuff.buffName) {
                return getUniqueBuff(existingBuffs);
            }
        }
        return cardBuff;
    }

    public void clickSkipShop() {
        shopOverlay.SetActive(false);
        foreach(Card card in cardsInShop) {
            cardManager.discardCard(card);
            card.CardUI.GetComponent<CanvasGroup>().alpha=1f;
        }
        cardsInShop.Clear();
        buffsInShop.Clear();
        if(shopUsesAvailable==0) {
            playGameMusic(true);
        }
        
        Debug.Log("Show Shop 4");
        showShopIfAvailable();
    }

    public void enableShopCard() {

    }

    public void clickShopPurchase() {
       
       
        List<Card> cards = new List<Card>();
        foreach(int cardID in shopCardSelected) {
            cards.Add(cardsInShop[cardID]);
        }
       
        CardBuffSO buff = buffsInShop[shopBuffSelected];    

        List<Card> cardsToAnimate=new List<Card>(cards);

        //Apply our buff to our card
        switch (buff.cardEnhancement) {
            case CardEnhancement.increase_score:
            case CardEnhancement.increase_mult:
            case CardEnhancement.all_suits:
            case CardEnhancement.increase_timer:
                cards[0].addBuff(buff);
            break;
            case CardEnhancement.remove_card:
                
            break;
            case CardEnhancement.increase_rank:
                cards[0].modifyRank(1);
            break;
            case CardEnhancement.decrease_rank:
                cards[0].modifyRank(-1);
            break;
            case CardEnhancement.copy_card:
                Card newCard=cards[0].cloneCard(cardManager.cardPrefab);
                //cardsInShop.Add(newCard);
                cardManager.registerCard(newCard);
                newCard.CardUI.transform.SetParent(cards[0].CardUI.transform.parent);
                newCard.CardUI.transform.localPosition = cards[0].CardUI.transform.parent.localPosition+new Vector3(150f, 0, 0);
                newCard.shrinkToRatio(cardManager.TINY_CARD_SIZE);
                newCard.setZone(CardZone.shop);
                cardsInShop.Add(newCard);
                cardsToAnimate.Add(newCard);
            break;
            case CardEnhancement.transform_heart:
            case CardEnhancement.transform_diamond:
            case CardEnhancement.transform_club:
            case CardEnhancement.transform_spade:
                    CardSuit cardSuit=CardSuit.hearts;
                    if (buff.cardEnhancement==CardEnhancement.transform_diamond) {
                        cardSuit=CardSuit.diamonds;
                    } 
                    else if (buff.cardEnhancement==CardEnhancement.transform_club) {
                        cardSuit=CardSuit.clubs;
                    }
                    else if (buff.cardEnhancement==CardEnhancement.transform_spade) {
                        cardSuit=CardSuit.spades;
                    }
                    foreach(Card card in cards) {
                        card.setSuit(cardSuit);
                    }

            break;
        }

        foreach(Card card in cards) {
            card.updateUI();
        }
    
        float riseAndFadeAnimationTime=1f;
        float hideOtherCardAnimationTime=0.5f;
        float animationPauseAtEnd=.1f;
        float animationPercRising=0.7f;

        //Animate chosen card(s) - rise, then animate to deck
        foreach(Card cardToAnimate in cardsToAnimate) {
            float endAlpha=((buff.cardEnhancement==CardEnhancement.remove_card) ? 0f : 1f);
            AnimationType animationType=(buff.cardEnhancement==CardEnhancement.remove_card) ? AnimationType.expandAndFade : AnimationType.riseThenMove;
            if (buff.cardEnhancement==CardEnhancement.remove_card) {
                cardToAnimate.CardUI.GetComponent<CanvasGroup>().blocksRaycasts=false;
            }
            animationManager.animateObject(animationType, cardToAnimate.CardUI, null, new Vector3(-800f,-425f,0), 1f, riseAndFadeAnimationTime, 1f, endAlpha, false, buff.cardEnhancement==CardEnhancement.remove_card, animationPercRising);
            //public void animateObject(AnimationType animationType, GameObject gameObject, GameObject? newParentObject, Vector3? targetPosition, float endScale, float totalTime, float startAlpha=1f, float endAlpha=1f, bool destroyOnFinish=true, bool cloneObject=true, float timeInRisePhase=.5f) {
            if (buff.cardEnhancement==CardEnhancement.remove_card) {
                cardToAnimate.CardUI.GetComponent<CanvasGroup>().alpha=0f;
            }
                
        }

        foreach(GameObject cardHighlighter in shopCardHighlighters) {
            cardHighlighter.SetActive(false);
        }

        //Switch card animation to face down after it's finished rising
        Invoke("clickShopPurchase_setToDiscard", riseAndFadeAnimationTime/animationSpeed);

        //Fade out non-purchased cards
        /*foreach(Card thisCard in cardsInShop) {
            if (!cardsToAnimate.Contains(thisCard)) {
                animationManager.animateObject(AnimationType.expandAndFade, thisCard.CardUI, null, null, 1f, hideOtherCardAnimationTime, 1f, 0f, false, false);
            }
        }*/

        shopPurchaseButton.SetActive(false);
        shopSkipButton.SetActive(false);
        
        audioManager.playSound(buff.soundToPlay, 1f, 0);

        Invoke("clickShopPurchase_complete", ((riseAndFadeAnimationTime/animationSpeed)+animationPauseAtEnd));
        Debug.Log("AnimationSpeed="+animationSpeed+" - timeToComplete="+((riseAndFadeAnimationTime/animationSpeed)+animationPauseAtEnd));
    }

    public void clickShopPurchase_setToDiscard() {
        foreach(Card thisCard in cardsInShop) {
            thisCard.setZone(CardZone.discard);
        }
    }

    public void clickShopPurchase_complete() {
        Debug.Log("AnimationSpeed COMPLETE");
        Card card = cardsInShop[shopCardSelected[0]];
        CardBuffSO buff = buffsInShop[shopBuffSelected];   
        switch (buff.cardEnhancement) { 
            case CardEnhancement.remove_card:
                cardsInShop.Remove(card);
                cardManager.removeCard(card);
                card.Destroy();
            break;
            /*case CardEnhancement.copy_card:
                cardManager.discardCard(newCard);
                newCard.shrinkToRatio(cardManager.TINY_CARD_SIZE);
            break;*/
        }
        clickSkipShop();
        shopPurchaseButton.SetActive(true);
        shopSkipButton.SetActive(true);
        tutorialManager.clickedApplyUpgrade();
        shopCardSelected.Clear();
    }

    public void clickedShopObject(bool isCard, int cardPicked) {

        CardBuffSO selectedBuff=null;
        if (isCard) {
            int allowedCards=1;
            if (shopBuffSelected>-1) {
                selectedBuff=buffsInShop[shopBuffSelected];
                allowedCards=selectedBuff.cardsAffected;
            }
            if (shopCardSelected.Contains(cardPicked)) {
                shopCardSelected.Remove(cardPicked);
            }
            else if (shopCardSelected.Count<allowedCards) {
                shopCardSelected.Add(cardPicked);
            }
            else {
                shopCardSelected.Clear();
                shopCardSelected.Add(cardPicked);
            }
        }
        else {
            int buffUpto=0;
            shopBuffSelected=cardPicked;
            foreach(GameObject buffContainer in shopBuffContainers) {
                buffContainer.GetComponent<CanvasGroup>().alpha= (buffUpto==cardPicked ? 1f : .2f);
                buffUpto++;
                if (buffUpto==cardPicked) {
                    selectedBuff=buffsInShop[buffUpto];
                    if (selectedBuff.cardsAffected>shopCardSelected.Count) {
                        shopCardSelected.Clear();
                    }
                }
            }
        }

        int cardUpto=0;
        foreach(GameObject cardHighlighter in shopCardHighlighters) {
            shopCardContainers[cardUpto].GetComponent<CanvasGroup>().alpha= (shopCardSelected.Contains(cardUpto) ? 1f : .7f);
            cardHighlighter.SetActive(shopCardSelected.Contains(cardUpto));
            cardUpto++;
        }

        Debug.Log("Clicked shop object"+isCard+" "+cardPicked+" cards selected="+shopCardSelected.Count);

        bool selectBtnEnabled=(shopBuffSelected>-1 && shopCardSelected.Count==buffsInShop[shopBuffSelected].cardsAffected);
        shopPurchaseButton.GetComponent<Button>().interactable=selectBtnEnabled;
    }

    public void enableAllShopObjects() {
        int cardUpto=0;
        foreach(GameObject cardHighlighter in shopCardHighlighters) {
            cardHighlighter.SetActive(false);
            shopCardContainers[cardUpto].GetComponent<CanvasGroup>().alpha=.7f;
            cardUpto++;
        }
        foreach(GameObject buffContainer in shopBuffContainers) {
            buffContainer.GetComponent<CanvasGroup>().alpha=1f;
        }
    }

    private void checkShopVoucherThreshold(int depth=0) {
        if (score>=nextShopScore) {
            shopUsesAvailable++;
            shopIncrement+=gameLengthManager.GameLengthSO.SHOP_INCREMENT_INCREASE;
            nextShopScore+=shopIncrement;
            if (depth==0) {
                animationManager.animateObjectExpandAndFade(shopButton, .3f, 2f);
            }
            checkShopVoucherThreshold(depth++);
        }
    }

    public void increaseEnemyTimer(int timerIncrease, int enemy) {
        foreach(Enemy thisEnemy in enemies) {
                if (thisEnemy.enemyPosition==(enemy-1)) {
                    thisEnemy.turnsUntilAttack+=timerIncrease;
                    thisEnemy.updateUI();
                    if (!showingOverlay()) {
                        thisEnemy.animateTimer();
                    }
                    break;
                }
            }
    }

    public MusicMood getGameMood(bool showingScore=false) {
        MusicMood mood=showingScore ? MusicMood.scoring : MusicMood.game;

        if (gameState==GameState.boss) {
            return MusicMood.boss_loop;
        }   
        
        if (runningTutorial) {
            return MusicMood.tutorial;
        }   

        if (shopOverlay.activeSelf) {
            return MusicMood.shop;
        }   

        foreach (Enemy enemy in enemies) {
            if (enemy.turnsUntilAttack<3) {
                mood= showingScore ? MusicMood.scoring_very_intense : MusicMood.very_intense;
                break;
            }
            else if (enemy.turnsUntilAttack<5) {
                mood= showingScore ? MusicMood.scoring_intense : MusicMood.intense;
            }
        }
        return mood;
    }

    public void playGameMusic(bool forceMood=false) {
        Debug.Log("PlayGameMusic mood");
        if (forceMood) {

        }
        else if (audioManager.moodPlaying==MusicMood.boss_intro || audioManager.desiredMood==MusicMood.boss_intro || audioManager.moodPlaying==MusicMood.intro ) {
            return;
        }

        audioManager.changeMusicMood(getGameMood());
    }

    void loadHighScoresAtEndOfGame() {
        Debug.Log("iiii reset boss parent="+bossContainer.gameObject.name+" scale="+bossScale);
        bossObject.transform.localScale=bossScale;
        StartCoroutine(highScoreManager.LoadScores(endGameScoreHolder, Color.white, gameLengthManager.GameLengthSO.HIGH_SCORE_GAME_ID));
    }

    public void clickDeck() {
        deckOverlay.SetActive(!deckOverlay.activeSelf);
        if (deckOverlay.activeSelf) {
            cardManager.loadDeckStats(CardZone.all);
        }
        tutorialManager.clickedDeck();
    }

    public void clickViewAllDeck() {
        deckOverlay.SetActive(true);
        cardManager.loadDeckStats(CardZone.all);
    }
    
    public void clickViewDraw() {
        deckOverlay.SetActive(true);
        cardManager.loadDeckStats(CardZone.deck);
    }

    public void clickViewDiscard() {
        deckOverlay.SetActive(true);
        cardManager.loadDeckStats(CardZone.discard);
    }
    
    public void clickCloseDeckOverlay() {
        deckOverlay.SetActive(false);
        tutorialManager.closedDeck();
    }

    public void addStoreCreditForTutorial() {
        shopUsesAvailable=1;
        Debug.Log("Show Shop 5");
        showShopIfAvailable();
    }

    public bool showingOverlay() {
        return deckOverlay.activeSelf || shopOverlay.activeSelf || scoreOverlay.activeSelf;
    }
}

