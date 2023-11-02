using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler {
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image visuals;
    [SerializeField] private float hoverHeight = 0.1f; // Adjust the height as needed
    [SerializeField] private float moveSpeed = 50f; // Adjust the height as needed

    private Canvas canvas;
    private Vector3 originalPosition;
    private Transform hoverPos;

    private ActionQueue actionQueue;
    private bool isUp;

    private void Update() {
        actionQueue.OnUpdate();
    }

    // Hovertransform is real dirty
    public void SetUp(UnitData unit, Transform hoverTransform) {
        actionQueue = new();

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
        if (isUp)
            return;

        Vector3 newPosition = originalPosition + new Vector3(0, hoverHeight, 0);
        actionQueue.Enqueue(new MoveObjectAction(gameObject, moveSpeed, newPosition));

        transform.position = newPosition;
    }

    public void OnPointerDown(PointerEventData eventData) {
        isUp = !isUp;

        if (isUp) {
            actionQueue.Clear();
            actionQueue.Enqueue(new MoveObjectAction(gameObject, moveSpeed, hoverPos));
        }
        else {
            actionQueue.Clear();
            actionQueue.Enqueue(new MoveObjectAction(gameObject, moveSpeed, originalPosition));
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        transform.position = originalPosition;
    }
}
