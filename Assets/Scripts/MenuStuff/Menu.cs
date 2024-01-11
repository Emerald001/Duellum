using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Menu : MonoBehaviour
{
    public void Btn_StartGame() {
        EventManager<AudioEvents, string>.Invoke(AudioEvents.PlayAudio, "ui_Click");
        SceneManager.LoadScene("Tutorial");
    }

    public void Btn_QuitGame() {
        EventManager<AudioEvents, string>.Invoke(AudioEvents.PlayAudio, "ui_Click");
        Application.Quit();
    }
}
