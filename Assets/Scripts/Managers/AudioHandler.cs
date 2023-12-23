using UnityEngine;

public class AudioHandler : MonoBehaviour
{
    [SerializeField] AudioManager audioManager;

    private void OnEnable() {
        EventManager<AudioEvents, string>.Subscribe(AudioEvents.PlayAudio, audioManager.PlayAudio);
        EventManager<AudioEvents, EventMessage<string, bool>>.Subscribe(AudioEvents.PlayLoopedAudio, PlayLoopedAudio);
    }

    private void OnDisable() {
        EventManager<AudioEvents, string>.Unsubscribe(AudioEvents.PlayAudio, audioManager.PlayAudio);
        EventManager<AudioEvents, EventMessage<string, bool>>.Unsubscribe(AudioEvents.PlayLoopedAudio, PlayLoopedAudio);
    }

    private void Start() {
        EventManager<AudioEvents, EventMessage<string, bool>>.Invoke(AudioEvents.PlayLoopedAudio, new("Music", true));
    }

    private void PlayLoopedAudio(EventMessage<string, bool> eventMessage) {

        string audioClipName = eventMessage.value1;
        bool isLooped = eventMessage.value2;

        audioManager.PlayLoopedAudio(audioClipName, isLooped);
    }
}

public enum AudioEvents {
    PlayAudio,
    PlayLoopedAudio,
}
