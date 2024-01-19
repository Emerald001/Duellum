using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleEndScreen : MonoBehaviour {
    [SerializeField] private GameObject WinScreen;
    [SerializeField] private GameObject LoseScreen;

    private void OnEnable() {
        EventManager<BattleEvents, int>.Subscribe(BattleEvents.BattleEnd, DisplayScreen);
    }
    private void OnDisable() {
        EventManager<BattleEvents, int>.Unsubscribe(BattleEvents.BattleEnd, DisplayScreen);
    }

    private void DisplayScreen(int LoseIndex) {
        if (LoseIndex == 0)
            LoseScreen.SetActive(true);
        else {
            WinScreen.SetActive(true);
            Invoke(nameof(ResetScreens), 2f);
        }
    }

    private void ResetScreens() {
        WinScreen.SetActive(false);
        LoseScreen.SetActive(false);
    }

    public void Btn_OnLoseExit() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
