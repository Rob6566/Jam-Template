using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonEventHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public GameManager gameManager;
    public int targetHand;
    public int cardPicked;

    void Update() {
    
    }

    public void OnPointerEnter(PointerEventData eventData) {
        Debug.Log("Hovered over button"+targetHand+" "+cardPicked);
        gameManager.cardManager.hoveredButton(targetHand, cardPicked);
    }

    public void OnPointerExit(PointerEventData eventData) {
        Debug.Log("Unhovered over button"+targetHand+" "+cardPicked);
        gameManager.cardManager.unHoveredButton();
    }
}
