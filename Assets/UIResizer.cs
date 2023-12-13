using TMPro;
using UnityEngine;

public class UIResizer : MonoBehaviour {
    [SerializeField] private RectTransform rect;
    [SerializeField] private TextMeshProUGUI UIElement;

    private void Update() {
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, UIElement.preferredHeight);
    }
}
