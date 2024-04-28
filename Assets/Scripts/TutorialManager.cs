using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TutorialStep {intro1, intro2, deck, shop1, shop2, threeCards, threeHands, upcomingCards, handEffect, betterHands, enemies, outtro, complete};

public class TutorialManager : MonoBehaviour {
    public GameManager gameManager;
    TutorialStep tutorialStep;

    public void init(GameManager newGameManager) {
        gameManager=newGameManager;
    }

    public void startTutorial() {
        tutorialStep=TutorialStep.intro1;
        loadTutorialStep();
    }

    public void nextTutorialStep() {
        tutorialStep++;
        loadTutorialStep();
    }

    public void skipTutorial() {
        tutorialStep=TutorialStep.complete;
        gameManager.endTutorial();
    }

    void loadTutorialStep() {
        //Add code to display all tutorial steps
    }
   
}