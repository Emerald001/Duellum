using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DungeonChest : MonoBehaviour, IPointerClickHandler {
    private bool hasClicked = false;

    public void OnPointerClick(PointerEventData eventData) {
        if (hasClicked)
            return;

        Debug.Log(1);

        hasClicked = true;
        StartCoroutine(ChestSequence());
    }

    private IEnumerator ChestSequence() {
        // Do desolve on Chest

        yield return new WaitForSeconds(3f);

        EventManager<UIEvents, int>.Invoke(UIEvents.GiveCard, 0);
    }
}
