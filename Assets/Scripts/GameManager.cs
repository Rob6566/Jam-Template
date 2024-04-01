//Main game manager
//IMPORTANT - make sure you set a unique GAME_ID in HighScoreManager

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //Managers
    public AudioManager audioManager;
    public HighScoreManager highScoreManager;
    public CardManager cardManager;

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
        cardManager=GameObject.FindWithTag("CardManager").GetComponent<CardManager>();   

        //Play music
        audioManager.fadeInMusic(0, 0, 1f);

        //Load high scores
        StartCoroutine(highScoreManager.LoadScores());
    }


    public void startGame() {
        if (!highScoreManager.validateName()) {
            return;
        }

        //Add game start stuff here
    }
}
