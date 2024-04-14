using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopContainerHandler : MonoBehaviour, IPointerClickHandler {

    public GameManager gameManager;
    public bool isCard;
    public int cardPicked;

    public void OnPointerClick(PointerEventData eventData) {
        gameManager.clickedShopObject(isCard, cardPicked);
    }

}
