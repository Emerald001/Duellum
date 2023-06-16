using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardAssetHolder : MonoBehaviour
{
    [Header("References")]
    public CardBehaviour cardBehaviour;

    [Header("Visuals")]
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Discription;
    public TextMeshProUGUI ManaCost;

    public Image Icon;
    public Image Background;
    public Image Border;
}
