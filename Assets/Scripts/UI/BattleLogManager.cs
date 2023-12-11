using TMPro;
using UnityEngine;

public class BattleLogManager : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI text;
    private string log;

    private void OnEnable() {
        EventManager<BattleEvents, string>.Subscribe(BattleEvents.AddBattleInformation, AddBattleInformation);
    }
    private void OnDisable() {
        EventManager<BattleEvents, string>.Unsubscribe(BattleEvents.AddBattleInformation, AddBattleInformation);
    }

    private void AddBattleInformation(string action) {
        log += "\n" + action;

        text.text = log;
    }
}
