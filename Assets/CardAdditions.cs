using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardAdditions : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler{
    [SerializeField] private BaseCardBehaviour cardBehaviour;
    [SerializeField] private CardAssetHolder cardAssetHolder;

    private bool hovering = false;
    private Coroutine hoveringRoutine;
    private float hoverDuration = 1f;

    public void OnPointerEnter(PointerEventData eventData) {
        hovering = true;

        if(hoveringRoutine != null) {
            StopCoroutine(hoveringRoutine);
        }

        hoveringRoutine = StartCoroutine(StartHoverTimer());
    }

    public void OnPointerExit(PointerEventData eventData) {
        hovering = false;
        Tooltip.instance.HideTooltip();
        if (hoveringRoutine != null) {
            StopCoroutine(hoveringRoutine);
        }
    }

    private IEnumerator StartHoverTimer() {
        yield return new WaitForSeconds(hoverDuration);
        if (hovering) {
            Tooltip.instance.ShowTooltip(cardAssetHolder.effect);
        }
    }
}
