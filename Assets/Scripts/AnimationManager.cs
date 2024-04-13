//Handles simple tween animations

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AnimationManager : MonoBehaviour {

    [SerializeField]
    GameObject animationContainer; //Used to make animated objects appear above everything else. We give them a temporary parent, then switch them back to their desired parent at the end

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
                    animatedObject.gameObject.transform.position=animatedObject.targetPosition;
                    animatedObject.gameObject.transform.localScale=animatedObject.endScale;
                    animatedObject.gameObject.transform.SetParent(animatedObject.targetParent.transform, true);
                    objectsToRemove.Add(animatedObject);
                }
                else {
                    float lerpValue = animatedObject.timeSpent/animatedObject.totalTime;
                    animatedObject.gameObject.transform.position=Vector3.Lerp(animatedObject.initialPosition, animatedObject.targetPosition, lerpValue);
                    animatedObject.gameObject.transform.localScale=Vector3.Lerp(animatedObject.startScale, animatedObject.endScale, lerpValue);
                }
            }
            foreach (AnimatedObject objectToRemove in objectsToRemove) {
                animatedObjects.Remove(objectToRemove);
            }
        }
    }

    public void animateObject(GameObject gameObject, GameObject newParentObject, Vector3 targetPosition, Vector3 endScale, float totalTime) {
        AnimatedObject newAnimatedObject = new AnimatedObject();
        newAnimatedObject.gameObject=gameObject;
        newAnimatedObject.targetParent=newParentObject;
        newAnimatedObject.initialPosition=gameObject.transform.position;
        newAnimatedObject.targetPosition=targetPosition;
        newAnimatedObject.totalTime=totalTime;
        newAnimatedObject.timeSpent=0f;
        newAnimatedObject.startScale=gameObject.transform.localScale;
        newAnimatedObject.endScale=endScale;
        gameObject.transform.SetParent(animationContainer.transform, true);
        animatedObjects.Add(newAnimatedObject);
    }

    public void animateObjectToNewParent(GameObject gameObject, GameObject newParent, float totalTime, float localScaleMultiplier=1f) {
        gameObject.transform.SetParent(newParent.transform, true);

        float initialWidth=gameObject.GetComponent<RectTransform>().rect.width;
        float initialHeight=gameObject.GetComponent<RectTransform>().rect.height;
        float newWidth=newParent.GetComponent<RectTransform>().rect.width;
        float newHeight=newParent.GetComponent<RectTransform>().rect.height;

        //Vector3 newLocalScale = new Vector3(gameObject.GetComponent<RectTransform>().localScale.x * localScaleMultiplier * newWidth/initialWidth , gameObject.GetComponent<RectTransform>().localScale.y* localScaleMultiplier *newHeight/initialHeight, 1f);
        Vector3 newLocalScale = new Vector3(localScaleMultiplier, localScaleMultiplier, localScaleMultiplier);

        Vector3 newPosition = newParent.transform.position;

        Debug.Log("Moving "+gameObject.name+"Initial width: "+initialWidth+" Initial height: "+initialHeight+" New width: "+newWidth+" New height: "+newHeight+"newPosition="+newPosition+" newLocalScale="+newLocalScale);

//        newPosition.x+=(newWidth/4f);
//        newPosition.y-=(newHeight/4f);


        animateObject(gameObject, newParent, newPosition/*new Vector3(0f, 0f, 0f)*/, newLocalScale, totalTime);
    }

    public bool isAnimating() {
        return animatedObjects.Count>0;
    }
}



public class AnimatedObject : MonoBehaviour {
    public GameObject gameObject;
    public GameObject targetParent;
    public Vector3 initialPosition;
    public Vector3 targetPosition;
    public Vector3 startScale;
    public Vector3 endScale;
    public float totalTime;
    public float timeSpent;
}