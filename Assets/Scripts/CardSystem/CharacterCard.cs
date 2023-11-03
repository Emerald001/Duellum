using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler {
    [Header("References")]
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image visuals;

    [Header("Values")]
    [SerializeField] private float hoverHeight = 0.1f; // Adjust the height as needed
    [SerializeField] private float moveSpeed = 50f; // Adjust the speed as needed

    private Canvas canvas;
    private Transform originalTransform;
    private Transform hoverTransform;

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

        originalTransform = new GameObject().transform;
        originalTransform.SetPositionAndRotation(transform.position, transform.rotation);
        this.hoverTransform = hoverTransform;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (isUp)
            return;

        Vector3 newPosition = originalTransform.position + new Vector3(0, hoverHeight, 0);
        actionQueue.Enqueue(new MoveObjectAction(gameObject, moveSpeed, newPosition));

        transform.position = newPosition;
    }

    public void OnPointerDown(PointerEventData eventData) {
        isUp = !isUp;

        if (isUp) {
            actionQueue.Clear();
            actionQueue.Enqueue(new ActionStack(new MoveObjectAction(gameObject, moveSpeed, hoverTransform), new ResizeAction(transform, moveSpeed / 5, new Vector3(1, 1, 1))));
        }
        else {
            actionQueue.Clear();
            actionQueue.Enqueue(new ActionStack(new MoveObjectAction(gameObject, moveSpeed, originalTransform), new ResizeAction(transform, moveSpeed / 5, new Vector3(.3f, .3f, .3f))));
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (isUp)
            return;

        transform.position = originalTransform.position;
    }
}
