using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class AnimationsUI : MonoBehaviour
{
    private bool highlighted = false;
    [SerializeField] private float hoverIntensity = 1.2f;
    [SerializeField] private float clickIntensity = 1.5f;
    [SerializeField] private string hoverSfxName;
    [SerializeField] private string clickSfxName;
    [SerializeField] private string highlightSfxName;
    public void Btn_HoverOver() {
        transform.DOScale(new Vector3(hoverIntensity, hoverIntensity, hoverIntensity), 0.1f);
        EventManager<AudioEvents, string>.Invoke(AudioEvents.PlayAudio, hoverSfxName);
    }

    public void Btn_Exit() {
        transform.DOScale(new Vector3(1f, 1f, 1f), 0.1f);
    }

    public void Btn_Click() {
        EventManager<AudioEvents, string>.Invoke(AudioEvents.PlayAudio, "ui_Click");
        EventManager<AudioEvents, string>.Invoke(AudioEvents.PlayAudio, clickSfxName);
        transform.DOScale(new Vector3(clickIntensity, clickIntensity, clickIntensity), 0.2f);
        transform.DOScale(new Vector3(clickIntensity, clickIntensity, clickIntensity), 0.2f).From();
    }

    public void Btn_HighLight() {
        if(!highlighted) {
        EventManager<AudioEvents, string>.Invoke(AudioEvents.PlayAudio, "ui_Click");
        EventManager<AudioEvents, string>.Invoke(AudioEvents.PlayAudio, highlightSfxName);
        transform.DOLocalMoveY(-100, 0.5f);
            highlighted = true;
        }
        else {
        transform.DOLocalMoveY(100, 0.5f).From();
            highlighted = false;
        }
    }
}
