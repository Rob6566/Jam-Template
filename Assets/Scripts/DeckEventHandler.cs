using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeckEventHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public GameManager gameManager;
    public int targetHand;
    public int cardPicked;

    public void OnPointerEnter(PointerEventData eventData) {
        Debug.Log("Hovered over deck");
        //gameManager.cardManager.hoveredButton(targetHand, cardPicked);
    }

    public void OnPointerExit(PointerEventData eventData) {
        Debug.Log("Exit Hover deck");
        //gameManager.cardManager.unHoveredButton();
    }
}
