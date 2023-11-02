using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image visuals;
    [SerializeField] private float hoverHeight = 0.1f; // Adjust the height as needed

    private Canvas canvas;
    private Vector3 originalPosition;

    public void SetUp(UnitData unit) {
        nameText.SetText(unit.name);
        descriptionText.SetText(unit.Description.ToString());
        defenseText.SetText(unit.BaseStatBlock.Defence.ToString());
        attackText.SetText(unit.BaseStatBlock.Attack.ToString());
        visuals.sprite = unit.Icon;

        canvas = GetComponentInChildren<Canvas>();
        canvas.worldCamera = Camera.main;
        originalPosition = transform.position;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        Vector3 newPosition = originalPosition + new Vector3(0, hoverHeight, 0);
        transform.position = newPosition;
    }

    public void OnPointerDown(PointerEventData eventData) {
        
    }

    public void OnPointerExit(PointerEventData eventData) {
        transform.position = originalPosition;
    }

    public void OnPointerUp(PointerEventData eventData) {
        
    }
}
