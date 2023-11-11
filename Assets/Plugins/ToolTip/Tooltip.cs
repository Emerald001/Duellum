using TMPro;
using UnityEngine;

public class Tooltip : MonoBehaviour {
    public static Tooltip instance;

    private TextMeshProUGUI tooltipText;
    private RectTransform backgroundRectTransform;
    private RectTransform parentRect;

    private void Awake() {
        instance = this;

        tooltipText = GetComponentInChildren<TextMeshProUGUI>();
        parentRect = transform.parent.GetComponent<RectTransform>();
        backgroundRectTransform = transform.GetChild(0).GetComponent<RectTransform>();
    }

    private void Update() {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, Input.mousePosition, null, out Vector2 localPoint);

        if (localPoint.x + backgroundRectTransform.rect.width > parentRect.rect.width / 2)
            localPoint.x = parentRect.rect.width / 2 - backgroundRectTransform.rect.width;
        if (localPoint.y + backgroundRectTransform.rect.height > parentRect.rect.height / 2)
            localPoint.y = parentRect.rect.height / 2 - backgroundRectTransform.rect.height;

        Vector2 editedPos = new(localPoint.x, localPoint.y - 90f);
        transform.localPosition = editedPos;
    }

    public void ShowTooltip(string tooltipString) {
        float textpadding = 10f;
        Vector2 backgroundSize = new(tooltipText.preferredWidth + textpadding * 2f, tooltipText.preferredHeight + textpadding * 2f);

        tooltipText.text = tooltipString;
        backgroundRectTransform.sizeDelta = backgroundSize;

        gameObject.SetActive(true);
    }

    public void HideTooltip() {
        gameObject.SetActive(false);
    }

    public static void ShowTooltip_Static(string tooltipString) {
        instance.ShowTooltip(tooltipString);
    }

    public static void HideTooltip_Static() {
        instance.HideTooltip();
    }
}