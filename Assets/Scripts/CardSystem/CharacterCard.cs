using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [Header("References")]
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image visuals;

    [Header("Values")]
    [SerializeField] private float hoverHeight = 0.1f; // Adjust the height as needed

    private Canvas canvas;
    private Vector3 originalPos;
    private Vector3 hoverPos;

    private ActionQueue actionQueue;

    private void Update() {
        actionQueue.OnUpdate();
    }

    public void SetUp(UnitData unit, Vector3 hoverPos) {
        actionQueue = new();

        nameText.SetText(unit.name);
        descriptionText.SetText(unit.Description.ToString());
        defenseText.SetText(unit.BaseStatBlock.Defence.ToString());
        attackText.SetText(unit.BaseStatBlock.Attack.ToString());
        speedText.SetText(unit.BaseStatBlock.Speed.ToString());
        visuals.sprite = unit.Icon;

        canvas = GetComponentInChildren<Canvas>();
        canvas.worldCamera = Camera.main;

        originalPos = transform.position;
        this.hoverPos = hoverPos;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        EventManager<BattleEvents, bool>.Invoke(BattleEvents.SetPlayerInteractable, false);

        actionQueue.Clear();
        actionQueue.Enqueue(new MoveObjectAction(gameObject, 5, hoverPos));
    }

    public void OnPointerExit(PointerEventData eventData) {
        EventManager<BattleEvents, bool>.Invoke(BattleEvents.SetPlayerInteractable, true);

        actionQueue.Clear();
        actionQueue.Enqueue(new MoveObjectAction(gameObject, 5, originalPos));
    }
}
