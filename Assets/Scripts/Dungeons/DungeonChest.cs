using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DungeonChest : MonoBehaviour, IPointerClickHandler {
    public static int Dissolve = Shader.PropertyToID("_Dissolve");

    [SerializeField] private GameObject lockObject;

    private bool hasClicked = false;

    public void OnPointerClick(PointerEventData eventData) {
        if (hasClicked)
            return;

        hasClicked = true;
        StartCoroutine(ChestSequence());
    }

    private IEnumerator ChestSequence() {
        EventManager<CameraEventType, EventMessage<Transform, float, float>>.Invoke(CameraEventType.QuickZoom, new(transform, 4, 3f));

        yield return new WaitForSeconds(1f);

        Material mat = lockObject.GetComponent<Renderer>().material;

        float amount = 0;
        while (1 - amount > .01f) {
            amount = Mathf.Lerp(amount, 1f, Time.deltaTime * 2);

            mat.SetFloat(Dissolve, amount);
            yield return null;
        }

        EventManager<UIEvents, int>.Invoke(UIEvents.GiveCard, 0);
    }
}
