using UnityEngine;

public class CardPickUp : MonoBehaviour {
    private void Update() {
        transform.Rotate(0, 50 * Time.deltaTime, 0);
    }
}
