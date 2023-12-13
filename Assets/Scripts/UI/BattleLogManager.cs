using TMPro;
using UnityEngine;

public class BattleLogManager : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI text;
    private string log;

    private void OnEnable() {
        EventManager<UIEvents, string>.Subscribe(UIEvents.AddBattleInformation, AddBattleInformation);
    }
    private void OnDisable() {
        EventManager<UIEvents, string>.Unsubscribe(UIEvents.AddBattleInformation, AddBattleInformation);
    }

    private void AddBattleInformation(string action) {
        log += "\n" + action;

        text.text = log;
    }
}
