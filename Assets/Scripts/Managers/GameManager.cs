using UnityEngine;

public class GameManager : MonoBehaviour {
    [SerializeField] private BattleData TMPData;

    private void Start() {
        EventManager<UIEvents, CursorType>.Invoke(UIEvents.UpdateCursor, CursorType.Normal);
        Tooltip.HideTooltip_Static();
    }

    private void Update() {
        if (Input.GetKeyUp(KeyCode.H)) {
            EventManager<BattleEvents>.Invoke(BattleEvents.StartBattle);
            EventManager<BattleEvents, BattleData>.Invoke(BattleEvents.StartBattle, TMPData);
        }
    }
}