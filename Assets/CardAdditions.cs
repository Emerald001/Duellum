using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardAdditions : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] private BaseCardBehaviour cardBehaviour;
    [SerializeField] private CardAssetHolder cardAssetHolder;

    [SerializeField] private float tooltipDelay = 1f;
    [SerializeField] private float tooltipDuration = 3f;

    private bool hovering = false;
    private bool tooltipShowing = false;
    private float tooltipTimer = 0f;

    public void OnPointerEnter(PointerEventData eventData) {
        hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        hovering = false;
        Tooltip.instance.HideTooltip();
    }
    private void Update() {
        if (hovering && !tooltipShowing) {
            tooltipTimer += Time.deltaTime;
            if (tooltipTimer >= tooltipDelay) {
                Tooltip.instance.ShowTooltip(cardAssetHolder.effect);
                tooltipShowing = true;
                Invoke("Hide", 3);
            }
        }
        else {
            tooltipTimer = 0f;
        }

        if (tooltipShowing) {
            if (tooltipTimer >= tooltipDuration) {
                Hide();
            }
            else {
                tooltipTimer += Time.deltaTime;
            }
        }
    }

    private void Hide() {
        Tooltip.instance.HideTooltip();

    }
}
