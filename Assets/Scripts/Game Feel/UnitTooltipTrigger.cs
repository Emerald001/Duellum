using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] private UnitController controller;

    private void Start() {
    }

    public void OnPointerEnter(PointerEventData eventData) {
        string TextToShow = "Defense: " + controller.Values.currentStats.Defence.ToString();
        if (TextToShow != "")
            Tooltip.ShowTooltip_Static(controller.Values.currentStats.Defence.ToString());
    }

    public void OnPointerExit(PointerEventData eventData) {
        Tooltip.HideTooltip_Static();
    }
}
