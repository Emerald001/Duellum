using TMPro;
using UnityEngine;

public class InfoTextUpdater : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI infoText;

    private void OnEnable() {
        EventManager<UIEvents, string>.Subscribe(UIEvents.InfoTextUpdate, UpdateInfoUI);
    }
    private void OnDisable() {
        EventManager<UIEvents, string>.Unsubscribe(UIEvents.InfoTextUpdate, UpdateInfoUI);
    }

    private void UpdateInfoUI(string name) {
        infoText.text = name;
    }
}
