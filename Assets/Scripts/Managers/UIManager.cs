using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private Transform popUp;

    private void OnEnable() {
        EventManager<UIEvents, string>.Subscribe(UIEvents.InfoTextUpdate, UpdateInfoUI);
        EventManager<UIEvents, bool>.Subscribe(UIEvents.ShowPauseMenu, ShowPauseMenu);
        //EventManager<UIEvents, EventMessage<Vector3, string>>.Unsubscribe(UIEvents.);
    }
    private void OnDisable() {
        EventManager<UIEvents, string>.Unsubscribe(UIEvents.InfoTextUpdate, UpdateInfoUI);
        EventManager<UIEvents, bool>.Unsubscribe(UIEvents.ShowPauseMenu, ShowPauseMenu);
    }

    private void Start() {
    }
    private void UpdateInfoUI(string name) {
        infoText.text = name;
        Invoke("HideInfoUI", 3);
    }

    private void HideInfoUI() {
        infoText.text = "";
    }
    private void ShowInfoPanel(bool trigger) {
    }

    private void ShowPauseMenu(bool trigger) {
        pauseMenu.SetActive(trigger);
    }
}
