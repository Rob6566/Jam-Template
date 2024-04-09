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
                animatedObject.timeSpent+=(Time.deltaTime*gameManager.AnimationSpeed);
                if (animatedObject.timeSpent>=animatedObject.totalTime) {
                    animatedObject.gameObject.transform.localPosition=animatedObject.targetPosition;
                    animatedObject.gameObject.transform.localScale=animatedObject.endScale;
                    objectsToRemove.Add(animatedObject);
                }
                else {
                    float lerpValue = animatedObject.timeSpent/animatedObject.totalTime;
                    animatedObject.gameObject.transform.localPosition=Vector3.Lerp(animatedObject.initialPosition, animatedObject.targetPosition, lerpValue);
                    animatedObject.gameObject.transform.localScale=Vector3.Lerp(animatedObject.startScale, animatedObject.endScale, lerpValue);
                }
            }
            foreach (AnimatedObject objectToRemove in objectsToRemove) {
                animatedObjects.Remove(objectToRemove);
            }
        }
    }

    public void animateObject(GameObject gameObject, Vector3 targetPosition, Vector3 endScale, float totalTime) {
        AnimatedObject newAnimatedObject = new AnimatedObject();
        newAnimatedObject.gameObject=gameObject;
        newAnimatedObject.initialPosition=gameObject.transform.position;
        newAnimatedObject.targetPosition=targetPosition;
        newAnimatedObject.totalTime=totalTime;
        newAnimatedObject.timeSpent=0f;
        newAnimatedObject.startScale=gameObject.transform.localScale;
        newAnimatedObject.endScale=endScale;
        animatedObjects.Add(newAnimatedObject);
    }

    public void animateObjectToNewParent(GameObject gameObject, GameObject newParent, float totalTime) {
        gameObject.transform.SetParent(newParent.transform, true);

        float initialWidth=gameObject.GetComponent<RectTransform>().rect.width;
        float initialHeight=gameObject.GetComponent<RectTransform>().rect.height;
        float newWidth=newParent.GetComponent<RectTransform>().rect.width;
        float newHeight=newParent.GetComponent<RectTransform>().rect.height;

        Vector3 newLocalScale = new Vector3(gameObject.GetComponent<RectTransform>().localScale.x * newWidth/initialWidth, gameObject.GetComponent<RectTransform>().localScale.y*newHeight/initialHeight, 1f);

        animateObject(gameObject, /*newParent.transform.position*/ new Vector3(0f, 0f, 0f), newLocalScale, totalTime);
    }

    public bool isAnimating() {
        return animatedObjects.Count>0;
    }
}



public class AnimatedObject : MonoBehaviour {
    public GameObject gameObject;
    public Vector3 initialPosition;
    public Vector3 targetPosition;
    public Vector3 startScale;
    public Vector3 endScale;
    public float totalTime;
    public float timeSpent;
}