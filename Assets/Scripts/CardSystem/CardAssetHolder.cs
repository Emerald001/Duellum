using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardAssetHolder : MonoBehaviour
{
    [Header("References")]
    public BaseCardBehaviour cardBehaviour;

    [Header("Visuals")]
    public GameObject ToggleVisual;
    public CanvasGroup Fader;

    public TextMeshProUGUI Name;
    public TextMeshProUGUI Discription;
    public TextMeshProUGUI ManaCost;

    public Image Icon;
    public Image Background;
    public Image Border;
}
