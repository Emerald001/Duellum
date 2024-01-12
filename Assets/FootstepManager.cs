using System.Collections.Generic;
using UnityEngine;

public class FootstepManager : MonoBehaviour {
    [SerializeField] private List<string> stringList = new List<string>();
    [SerializeField] private string groundTag = "WalkableTile";

    public Transform leftFootTransform;
    public Transform rightFootTransform;
    private string previousString;

    public void CheckFootstep(Transform footTransform) {
        RaycastHit hit;
        if (Physics.Raycast(footTransform.position, Vector3.down, out hit, 0.1f) && hit.collider.CompareTag(groundTag)) {

            string randomString = GetRandomString();
            Debug.Log("Random String: " + randomString);
            EventManager<AudioEvents, string>.Invoke(AudioEvents.PlayAudio, randomString);
        }
    }
    private string GetRandomString() {
        if (stringList.Count == 0) {
            Debug.LogWarning("String list is empty!");
            return null;
        }

        int randomIndex;
        do {
            randomIndex = Random.Range(0, stringList.Count);
            // Repeat until a different string is selected
        } while (stringList[randomIndex] == previousString);

        // Store the current string for the next iteration
        previousString = stringList[randomIndex];

        
        return stringList[randomIndex];
    }

}
