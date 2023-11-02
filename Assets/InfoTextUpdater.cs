using TMPro;
using UnityEngine;

public class InfoTextUpdater : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI infoText;

    private void OnEnable() {
        EventManager<BattleEvents, string>.Subscribe(BattleEvents.InfoTextUpdate, UpdateInfoUI);
    }
    private void OnDisable() {
        EventManager<BattleEvents, string>.Unsubscribe(BattleEvents.InfoTextUpdate, UpdateInfoUI);
    }

    private void UpdateInfoUI(string name) {
        infoText.text = name;
    }
}
