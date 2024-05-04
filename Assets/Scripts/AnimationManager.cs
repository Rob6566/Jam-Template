//Handles simple tween animations

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public enum AnimationType {move, expandAndFade, riseThenMove};
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
                //UnityEngine.Object.Destroy(animatedObject.gameObject);
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
                if (animatedObject.gameObject==null || animatedObject.completed) {
                    Debug.Log("tttt DELETED Animating object "+animatedObject.name);
                    continue;
                }

                animatedObject.timeSpent+=(Time.deltaTime*gameManager.AnimationSpeed);
                CanvasGroup canvasGroup=animatedObject.gameObject.GetComponent<CanvasGroup>();

                //if (animatedObject.gameObject.name=="Enemy(Clone)") {
                    Debug.Log("tttt UPDATE Animating object "+animatedObject.name+" timeSpent="+animatedObject.timeSpent+" totalTime="+animatedObject.totalTime);
                //}
                
                if (animatedObject.timeSpent>=animatedObject.totalTime) {
                    //DONE - finalise and lock to final position

                    Debug.Log("tttt FIN Animating object "+animatedObject.name);

                    canvasGroup.alpha=animatedObject.endAlpha;
                    if (animatedObject.changeScale) {
                        animatedObject.gameObject.transform.localScale=animatedObject.endScale;
                    }
                    if (animatedObject.destroyOnFinish) {
                        animatedObject.gameObject.SetActive(false);
                        UnityEngine.Object.Destroy(animatedObject.gameObject);
                    }

                    if (animatedObject.animationType!=AnimationType.expandAndFade) {
                        //animatedObject.gameObject.transform.localPosition=animatedObject.targetPosition;
                        if (animatedObject.targetParent!=null) {
                            animatedObject.gameObject.transform.SetParent(animatedObject.targetParent.transform, true);
                            animatedObject.targetParent=null;
                        }
                    }
                    animatedObject.completed=true;
                    objectsToRemove.Add(animatedObject);
                }
                else {
                    //Animation in progress

                    float lerpValue = animatedObject.timeSpent/animatedObject.totalTime;
                    if (animatedObject.changeScale) {
                        animatedObject.gameObject.transform.localScale=Vector3.Lerp(animatedObject.startScale, animatedObject.endScale, lerpValue);                        
                    }
                    if (animatedObject.startAlpha!=animatedObject.endAlpha) {
                        canvasGroup.alpha=Mathf.Lerp(animatedObject.startAlpha, animatedObject.endAlpha, lerpValue);
                    }

                    if (animatedObject.animationType==AnimationType.expandAndFade) {
                        animatedObject.gameObject.transform.localScale=Vector3.Lerp(animatedObject.startScale, animatedObject.endScale, lerpValue);
                    }
                    else if (animatedObject.animationType==AnimationType.riseThenMove) {
                        Debug.Log("uuuu riseThenMove lerpValue="+lerpValue);
                        if (lerpValue<(animatedObject.timeInRisePhase*animatedObject.totalTime)) {
                            animatedObject.gameObject.transform.localPosition=Vector3.Lerp(animatedObject.initialPosition, animatedObject.midPosition, lerpValue/animatedObject.timeInRisePhase);
                            Debug.Log("uuuu Rising lerpValue="+lerpValue+" target= "+(animatedObject.timeInRisePhase*animatedObject.totalTime));
                        }
                        else {
                            animatedObject.gameObject.transform.localPosition=Vector3.Lerp(animatedObject.midPosition, animatedObject.targetPosition, (lerpValue-animatedObject.timeInRisePhase)/(1f-animatedObject.timeInRisePhase));
                            Debug.Log("uuuu Moving "+animatedObject.timeInRisePhase+" lerpValue="+lerpValue*animatedObject.totalTime);
                        }
                    }
                    else {
                        animatedObject.gameObject.transform.localPosition=Vector3.Lerp(animatedObject.initialPosition, animatedObject.targetPosition, lerpValue);
                    }
                }
            }
            foreach (AnimatedObject objectToRemove in objectsToRemove) {
                if (objectToRemove.name=="Card(Clone)") {
                    //Debug.Log("xxxx deleting object "+objectToRemove.name);
                    animatedObjects.Remove(objectToRemove);
                }
               //animatedObjects.Remove(objectToRemove);
               //TODO Super hacky - we're currently not removing the object from the list or destroying it
            }
        }
    }


    //GameObject gameObject, float totalTime, float expandToScale, float startAlpha=1f, float endAlpha=0f, bool destroyOnFinish=true, bool cloneObject=true

    public void animateObject(AnimationType animationType, GameObject gameObject, GameObject? newParentObject, Vector3? targetPosition, float endScale, float totalTime, float startAlpha=1f, float endAlpha=1f, bool destroyOnFinish=true, bool cloneObject=true, float timeInRisePhase=.5f) {
        Debug.Log("xxxx Animating object "+gameObject.name+" clone="+cloneObject+" destroyOnFinish="+destroyOnFinish);
        AnimatedObject newAnimatedObject = new AnimatedObject();
        newAnimatedObject.animationType=animationType;
        
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
        
        newAnimatedObject.gameObject=clonedGameObject;

        if (newParentObject==null) {
            newAnimatedObject.targetParent=(GameObject)gameObject.transform.parent.gameObject;
        }
        else {
            newAnimatedObject.targetParent=newParentObject;
        }
        
        if (targetPosition==null) {
            newAnimatedObject.targetPosition=gameObject.transform.localPosition;;
        }
        else {
            newAnimatedObject.targetPosition=(Vector3)targetPosition;
        }

        if (endScale!=1f) {
            newAnimatedObject.changeScale=true;
        }

        newAnimatedObject.name=gameObject.name;
        newAnimatedObject.totalTime=totalTime;
        newAnimatedObject.timeSpent=0f;
        newAnimatedObject.startScale=gameObject.transform.localScale;
        newAnimatedObject.endScale=new Vector3(endScale, endScale, endScale);
        clonedGameObject.transform.SetParent(animationContainer.transform, true);
        newAnimatedObject.initialPosition=gameObject.transform.localPosition;
        newAnimatedObject.timeInRisePhase=timeInRisePhase;

        if(animationType==AnimationType.riseThenMove) {
            newAnimatedObject.midPosition=newAnimatedObject.initialPosition+new Vector3(0f, 200f, 0f);

            Debug.Log("Rise then move "+gameObject.name+" from "+newAnimatedObject.initialPosition+" to "+newAnimatedObject.midPosition+" then to "+targetPosition);
        }
        
        //bool cloneObject=true
        newAnimatedObject.startAlpha=startAlpha;
        newAnimatedObject.endAlpha=endAlpha;
        newAnimatedObject.destroyOnFinish=destroyOnFinish;
    
        
        
        animatedObjects.Add(newAnimatedObject);

        Debug.Log("yyyy Animating object "+clonedGameObject.name+" object="+newAnimatedObject);
    }

    /*public void animateObjectToNewParent(GameObject gameObject, GameObject newParent, float totalTime, float localScaleMultiplier=1f) {
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


        animateObject(gameObject, newParent, newPosition, newLocalScale, totalTime);
    }*/


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
        newAnimatedObject.animationType=AnimationType.expandAndFade;
        newAnimatedObject.startScale=gameObject.transform.localScale;
        newAnimatedObject.endScale=gameObject.transform.localScale*expandToScale;
        if (expandToScale!=1f) {
            newAnimatedObject.changeScale=true;
        }

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
    public AnimationType animationType;
    public GameObject gameObject;
    public GameObject targetParent;
    public Vector3 initialPosition;
    public Vector3 targetPosition;
    public Vector3 midPosition;
    public Vector3 startScale;
    public Vector3 endScale;
    public float totalTime;
    public float timeSpent;
    public float startAlpha;
    public float endAlpha;
    public bool destroyOnFinish=false;
    public bool changeScale=false;
    public float timeInRisePhase; //Time in % to spend in the rise phase
    public string name;
    public bool completed=false;
}