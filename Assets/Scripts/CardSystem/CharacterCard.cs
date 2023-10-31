using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharacterCard : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {
    public TextMeshProUGUI attackText; 
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Image visuals;

    private Canvas canvas;

    private RectTransform objectToMove;
    public float hoverHeight = 0.1f; // Adjust the height as needed

    private Vector3 originalPosition;

    private void Start() {
        canvas = GetComponentInChildren<Canvas>();
        canvas.worldCamera = Camera.main;
        originalPosition = objectToMove.position;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        Vector3 newPosition = originalPosition + new Vector3(0, hoverHeight, 0);
        objectToMove = eventData.pointerEnter.GetComponent<RectTransform>();
        objectToMove.transform.position = newPosition;
    }

    public void OnPointerDown(PointerEventData eventData) {
        
    }

    public void OnPointerExit(PointerEventData eventData) {
        objectToMove = eventData.pointerEnter.GetComponent<RectTransform>();
        objectToMove.transform.position = originalPosition;
    }

    public void OnPointerUp(PointerEventData eventData) {
        
    }
}
