using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeckEventHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    public GameManager gameManager;
    public GameObject showDeckLabel;

    public void OnPointerEnter(PointerEventData eventData) {
        Debug.Log("Hovered over deck");
        showDeckLabel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData) {
        Debug.Log("Exit Hover deck");
        showDeckLabel.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData) {
        gameManager.clickDeck();
    }
}
