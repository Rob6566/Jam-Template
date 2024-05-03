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

    public void deleteAllAnimations() {
        foreach (AnimatedObject animatedObject in animatedObjects) {
            if (animatedObject.gameObject!=null) {
                UnityEngine.Object.Destroy(animatedObject.gameObject);
            }
        }
        animatedObjects.Clear();

        foreach(Transform child in animationContainer.transform) {
            Destroy(child.gameObject);
        }   
    }

    void Update() {
        if (gameManager.GameState==GameState.paused) {
            return;
        }
        if (animatedObjects.Count>0) {
            List<AnimatedObject> objectsToRemove = new List<AnimatedObject>();
            foreach (AnimatedObject animatedObject in animatedObjects) {
                if (animatedObject.gameObject==null) {
                    continue;
                }

                animatedObject.timeSpent+=(Time.deltaTime*gameManager.AnimationSpeed);
                CanvasGroup canvasGroup=animatedObject.gameObject.GetComponent<CanvasGroup>();
                if (animatedObject.timeSpent>=animatedObject.totalTime) {
                    if (animatedObject.expandAndFade) {
                        canvasGroup.alpha=animatedObject.endAlpha;
                        if (animatedObject.destroyOnFinish) {
                            animatedObject.gameObject.SetActive(false);
                            UnityEngine.Object.Destroy(animatedObject.gameObject);
                        }
                    }
                    else {
                        animatedObject.gameObject.transform.localPosition=animatedObject.targetPosition;
                        animatedObject.gameObject.transform.localScale=animatedObject.endScale;
                        if (animatedObject.targetParent!=null) {
                            animatedObject.gameObject.transform.SetParent(animatedObject.targetParent.transform, true);
                        }
                    }
//                    Debug.Log("Finished animating "+animatedObject.gameObject.name+" to "+animatedObject.targetParent.name+" in "+animatedObject.timeSpent+" seconds");
                    objectsToRemove.Add(animatedObject);
                }
                else {
                    if (animatedObject.expandAndFade) {
                        float lerpValue = animatedObject.timeSpent/animatedObject.totalTime;
                        animatedObject.gameObject.transform.localScale=Vector3.Lerp(animatedObject.startScale, animatedObject.endScale, lerpValue);
                        canvasGroup.alpha=Mathf.Lerp(animatedObject.startAlpha, animatedObject.endAlpha, lerpValue);
                    }
                    else {
                        float lerpValue = animatedObject.timeSpent/animatedObject.totalTime;
                        animatedObject.gameObject.transform.localPosition=Vector3.Lerp(animatedObject.initialPosition, animatedObject.targetPosition, lerpValue);
                        animatedObject.gameObject.transform.localScale=Vector3.Lerp(animatedObject.startScale, animatedObject.endScale, lerpValue);
                    }
                }
            }
            foreach (AnimatedObject objectToRemove in objectsToRemove) {
               animatedObjects.Remove(objectToRemove);
               //TODO Super hacky - we're currently not removing the object from the list or destroying it
            }
        }
    }

    public void animateObject(GameObject gameObject, GameObject newParentObject, Vector3 targetPosition, Vector3 endScale, float totalTime) {
        //Debug.Log("Animating object "+gameObject.name+" to "+newParentObject.name+" in "+totalTime+" seconds");
        AnimatedObject newAnimatedObject = new AnimatedObject();
        newAnimatedObject.gameObject=gameObject;
        newAnimatedObject.targetParent=newParentObject;
        newAnimatedObject.targetPosition=targetPosition;
        newAnimatedObject.totalTime=totalTime;
        newAnimatedObject.timeSpent=0f;
        newAnimatedObject.startScale=gameObject.transform.localScale;
        newAnimatedObject.endScale=endScale;
        gameObject.transform.SetParent(animationContainer.transform, true);
        newAnimatedObject.initialPosition=gameObject.transform.localPosition;
        newAnimatedObject.startAlpha=1f;
        newAnimatedObject.endAlpha=1f;
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


    public void animateObjectExpandAndFade(GameObject gameObject, float totalTime, float expandToScale, float startAlpha=1f, float endAlpha=0f, bool destroyOnFinish=true, bool cloneObject=true) {

        AnimatedObject newAnimatedObject = new AnimatedObject();


        GameObject clonedGameObject;
        
        if (cloneObject) {
            clonedGameObject=UnityEngine.Object.Instantiate(gameObject);
            clonedGameObject.transform.SetParent(gameObject.transform.parent);
            clonedGameObject.transform.position=gameObject.transform.position;
            clonedGameObject.transform.localScale=gameObject.transform.localScale;
        }
        else {
            clonedGameObject=gameObject;
        }
        
        newAnimatedObject.startAlpha=startAlpha;
        newAnimatedObject.endAlpha=endAlpha;
        newAnimatedObject.gameObject=clonedGameObject;
        newAnimatedObject.totalTime=totalTime;
        newAnimatedObject.timeSpent=0f;
        newAnimatedObject.expandAndFade=true;
        newAnimatedObject.startScale=gameObject.transform.localScale;
        newAnimatedObject.endScale=gameObject.transform.localScale*expandToScale;
        newAnimatedObject.destroyOnFinish=destroyOnFinish;
        newAnimatedObject.targetParent=clonedGameObject.transform.parent.gameObject;
        clonedGameObject.transform.SetParent(animationContainer.transform, true);
        animatedObjects.Add(newAnimatedObject);

        Debug.Log("Animating object expand and fade "+gameObject.name+" to scale "+newAnimatedObject.endScale+" ("+expandToScale+") in "+totalTime+" seconds");
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
    public bool expandAndFade=false;
    public float startAlpha;
    public float endAlpha;
    public bool destroyOnFinish=false;
}