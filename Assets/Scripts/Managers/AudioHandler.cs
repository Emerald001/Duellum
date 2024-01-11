using UnityEngine;

public class AudioHandler : MonoBehaviour
{
    [SerializeField] AudioManager audioManager;

    private void OnEnable() {
        EventManager<AudioEvents, string>.Subscribe(AudioEvents.PlayAudio, audioManager.PlayAudio);
    }

    private void OnDisable() {
        EventManager<AudioEvents, string>.Unsubscribe(AudioEvents.PlayAudio, audioManager.PlayAudio);
    }

    private void Start() {
        audioManager.PlayLoopedAudio("Music", true);
    }
}

public enum AudioEvents {
    PlayAudio,
    PlayLoopedAudio,
}
