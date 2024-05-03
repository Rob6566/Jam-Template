using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TutorialStep {intro1, intro2, deck1, deck2, shop1, shop2, threeCards, upcomingCards, handEffect, betterHands, enemies, outtro, complete};

public class TutorialManager : MonoBehaviour {
    public GameManager gameManager;
    TutorialStep tutorialStep;
    public List<GameObject> tutorialUIs;

    //Stuff that gets hidden for tutorial
    public GameObject nemesis;
    public GameObject blourtInShop;
    public GameObject skipUpgradeInShop;
    public GameObject blourtSpeechBubbleInShop;
    public GameObject skipTutorialBtn;
    public GameObject buttons;

    public void init(GameManager newGameManager) {
        gameManager=newGameManager;
    }

    public void startTutorial() {
        tutorialStep=TutorialStep.intro1;
        toggleObjectsForTutorial(false);
        loadTutorialStep();
    }

    public void nextTutorialStep() {
        tutorialStep++;

        skipTutorialBtn.SetActive(tutorialStep!=TutorialStep.deck2 && tutorialStep!=TutorialStep.outtro);
        buttons.SetActive(tutorialStep==TutorialStep.threeCards);

        if (tutorialStep==TutorialStep.upcomingCards) {
            gameManager.cardManager.unHoveredButton();
        }

        if (tutorialStep==TutorialStep.enemies) {
            skipTutorialBtn.SetActive(false);
            nemesis.SetActive(true);
            gameManager.spawnEnemies();
        }


        /*if (tutorialStep==TutorialStep.shop2) {
            blourtInShop.SetActive(true);
            blourtSpeechBubbleInShop.SetActive(true);

            return;
        }*/


        loadTutorialStep();
    }

    public void skipTutorial() {
        gameManager.endTutorial(tutorialStep<=TutorialStep.threeCards);
        tutorialStep=TutorialStep.complete;
        loadTutorialStep();
        toggleObjectsForTutorial(true);
        skipTutorialBtn.SetActive(false);
    }

    public void clickedDeck() {
        if (tutorialStep==TutorialStep.deck1) {
            nextTutorialStep();
        }
    }

   public void draftedCard() {
        if (tutorialStep==TutorialStep.threeCards) {
            nextTutorialStep();
        }
    }

    public void closedDeck() {
        if (tutorialStep==TutorialStep.deck2) {
            nextTutorialStep();
            gameManager.addStoreCreditForTutorial();
        }
    }

    public void clickedApplyUpgrade() {
        if (tutorialStep==TutorialStep.shop2) {
            nextTutorialStep();
        }
    }

    //Hide/show gameobjects that aren't needed for tutorial
    void toggleObjectsForTutorial(bool visibility) {
        nemesis.SetActive(visibility);
        blourtInShop.SetActive(visibility);
        skipUpgradeInShop.SetActive(visibility);
        blourtSpeechBubbleInShop.SetActive(visibility);
        buttons.SetActive(visibility);
    }

    void loadTutorialStep() {
        //Add code to display all tutorial steps
        int i=0;
        foreach (GameObject tutorialUI in tutorialUIs) {
            tutorialUI.SetActive(i==(int)tutorialStep);
            i++;
        }
    }
}