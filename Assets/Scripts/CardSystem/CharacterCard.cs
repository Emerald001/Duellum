using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;

public class CharacterCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [Header("References")]
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image visuals;

    private Canvas canvas;
    private Vector3 originalPos;
    private Vector3 hoverPos;

    private ActionQueue actionQueue;

    string nameTT;
    string attackTT;
    string defenseTT;
    string speedTT;
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

        nameTT = unit.Name;
        attackTT = unit.BaseStatBlock.Attack.ToString();
        defenseTT = unit.BaseStatBlock.Defence.ToString();
        speedTT = unit.BaseStatBlock.Speed.ToString();

        canvas = GetComponentInChildren<Canvas>();
        canvas.worldCamera = Camera.main;

        originalPos = transform.position;
        this.hoverPos = hoverPos;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        EventManager<BattleEvents, bool>.Invoke(BattleEvents.SetPlayerInteractable, false);
        Tooltip.instance.ShowTooltip($"{nameTT} <br> defense: <color=green>{defenseTT}</color> <br> base attack: <color=red>{attackTT}</color> <br> speed: <color=yellow>{speedTT}</color>");
        Invoke("Hide", 3);
        actionQueue.Clear();
        actionQueue.Enqueue(new MoveObjectAction(gameObject, 15, hoverPos));
    }

    public void OnPointerExit(PointerEventData eventData) {
        EventManager<BattleEvents, bool>.Invoke(BattleEvents.SetPlayerInteractable, true);
        actionQueue.Clear();
        actionQueue.Enqueue(new MoveObjectAction(gameObject, 15, originalPos));
    }

    void Hide() {
        Tooltip.HideTooltip_Static();
    }
}
