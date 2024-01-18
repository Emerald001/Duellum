using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFade : MonoBehaviour {
    [SerializeField] private Image blackScreen;

    private void OnEnable() {
        EventManager<UIEvents, EventMessage<float, System.Action>>.Subscribe(UIEvents.DoFade, (x) => StartCoroutine(Fade(x)));
    }
    private void OnDisable() {
        EventManager<UIEvents, EventMessage<float, System.Action>>.Unsubscribe(UIEvents.DoFade, (x) => StartCoroutine(Fade(x)));
    }

    private IEnumerator Fade(EventMessage<float, System.Action> message) {
        System.Action performOnEndFade = message.value2;

        float target = message.value1;
        float currentFade = blackScreen.color.a;

        while (currentFade != target) {
            currentFade = Mathf.MoveTowards(currentFade, target, Time.deltaTime);
            blackScreen.color = new Color(0f, 0f, 0f, currentFade);
            yield return null;
        }

        performOnEndFade?.Invoke();
    }
}