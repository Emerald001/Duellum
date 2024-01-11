using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private GameObject pauseMenu;

    private TextMeshProUGUI panelTitleText;
    private TextMeshProUGUI panelContentText;

    private void Start() {
        infoPanel.SetActive(false);
        panelTitleText = infoPanel.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable() {
        EventManager<UIEvents, string>.Subscribe(UIEvents.InfoTextUpdate, UpdateInfoUI);
        EventManager<UIEvents, bool>.Subscribe(UIEvents.PopUpWindow, ShowInfoPanel);
        EventManager<UIEvents, bool>.Subscribe(UIEvents.ShowPauseMenu, ShowPauseMenu);
    }
    private void OnDisable() {
        EventManager<UIEvents, string>.Unsubscribe(UIEvents.InfoTextUpdate, UpdateInfoUI);
        EventManager<UIEvents, bool>.Unsubscribe(UIEvents.PopUpWindow, ShowInfoPanel);
        EventManager<UIEvents, bool>.Unsubscribe(UIEvents.ShowPauseMenu, ShowPauseMenu);
    }

    private void UpdateInfoUI(string name) {
        infoText.text = name;
    }

    private void ShowInfoPanel(bool trigger) {
        infoPanel.SetActive(trigger);
    }

    private void ShowPauseMenu(bool trigger) {
        pauseMenu.SetActive(trigger);
    }
}
