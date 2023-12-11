using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] private string TextToShow;

    public void OnPointerEnter(PointerEventData eventData) {
        if (TextToShow != "")
            Tooltip.ShowTooltip_Static(TextToShow);
    }

    public void OnPointerExit(PointerEventData eventData) {
        Tooltip.HideTooltip_Static();
    }
}