using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardAssetHolder : MonoBehaviour
{
    [Header("Visuals")]
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Discription;
    public TextMeshProUGUI ManaCost;

    public Image Icon;
    public Image Background;

    private readonly ActionQueue queue = new();

    public void Update() {
        queue.OnUpdate();
    }

    public void SetActionQueue(List<Action> actions) {
        foreach (var item in actions)
            queue.Enqueue(item);
    }

    public void ClearQueue() => queue.Clear();

    private void OnMouseEnter() {
        Debug.Log(gameObject.name);
    }
}
