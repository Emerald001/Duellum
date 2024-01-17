using System.Collections.Generic;
using UnityEngine;

public class FootstepManager : MonoBehaviour {
    [SerializeField] private List<string> stringList = new();
    [SerializeField] private ParticleSystem dustParticles;
    [SerializeField] private Transform leftFootTransform;
    [SerializeField] private Transform rightFootTransform;

    [SerializeField] private string groundTag = "WalkableTile";

    public Transform LeftFootTransform => leftFootTransform;
    public Transform RightFootTransform => rightFootTransform;

    private string previousString;

    public void CheckFootstep(Transform footTransform) {
        if (Physics.Raycast(footTransform.position, Vector3.down, out RaycastHit hit, 0.1f)) {
            if (!hit.collider.CompareTag(groundTag))
                return;

            string randomString = GetRandomString();
            EventManager<AudioEvents, string>.Invoke(AudioEvents.PlayAudio, randomString);
            dustParticles.Play();
        }
    }

    private string GetRandomString() {
        if (stringList.Count == 0)
            return null;

        int randomIndex;
        do {
            randomIndex = Random.Range(0, stringList.Count);
        } while (stringList[randomIndex] == previousString);

        previousString = stringList[randomIndex];
        return stringList[randomIndex];
    }
}
