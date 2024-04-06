//Handles simple tween animations

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AnimationManager : MonoBehaviour {

    GameManager gameManager;
    List<AnimatedObject> animatedObjects = new List<AnimatedObject>();

    public void init(GameManager newGameManager) {
        gameManager=newGameManager;
    }

    void Update() {
        if (gameManager.GameState==GameState.paused) {
            return;
        }
        if (animatedObjects.Count>0) {
            List<AnimatedObject> objectsToRemove = new List<AnimatedObject>();
            foreach (AnimatedObject animatedObject in animatedObjects) {
                animatedObject.timeSpent+=Time.deltaTime;
                if (animatedObject.timeSpent>=animatedObject.totalTime) {
                    animatedObject.gameObject.transform.position=animatedObject.targetPosition;
                    objectsToRemove.Add(animatedObject);
                }
                else {
                    float lerpValue = animatedObject.timeSpent/animatedObject.totalTime;
                    animatedObject.gameObject.transform.position=Vector3.Lerp(animatedObject.initialPosition, animatedObject.targetPosition, lerpValue);
                }
            }
            foreach (AnimatedObject objectToRemove in objectsToRemove) {
                animatedObjects.Remove(objectToRemove);
            }
        }
    }

    public void animateObject(GameObject gameObject, Vector3 targetPosition, float totalTime) {
        AnimatedObject newAnimatedObject = new AnimatedObject();
        newAnimatedObject.gameObject=gameObject;
        newAnimatedObject.initialPosition=gameObject.transform.position;
        newAnimatedObject.targetPosition=targetPosition;
        newAnimatedObject.totalTime=totalTime;
        newAnimatedObject.timeSpent=0f;
        animatedObjects.Add(newAnimatedObject);
    }

    public bool isAnimating() {
        return animatedObjects.Count>0;
    }
}



public class AnimatedObject : MonoBehaviour {
    public GameObject gameObject;
    public Vector3 initialPosition;
    public Vector3 targetPosition;
    public float totalTime;
    public float timeSpent;
}