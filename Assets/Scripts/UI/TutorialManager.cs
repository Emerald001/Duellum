using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class TutorialManager : MonoBehaviour {
    [SerializeField] private List<VideoClip> videoClips;

    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage rawImage;
    [SerializeField] private TextMeshProUGUI videoTitleText;
    [SerializeField] private TextMeshProUGUI videoDescriptionText;
    
    [SerializeField] private string[] videoTitles;
    [SerializeField] private string[] videoDescriptions;

    private int currentVideoIndex = 0;

    private void Start() {
        PlayVideo();
    }

    private void PlayVideo() {
        videoPlayer.clip = videoClips[currentVideoIndex];
        videoPlayer.Prepare();
        videoPlayer.Play();

        // Update the video title and description
        videoTitleText.text = videoTitles[currentVideoIndex];
        videoDescriptionText.text = videoDescriptions[currentVideoIndex];
    }

    public void Btn_NextVideo() {
        EventManager<AudioEvents, string>.Invoke(AudioEvents.PlayAudio, "ui_Click");
        if (currentVideoIndex + 1 >= videoClips.Count) {
            SceneManager.LoadScene("SampleScene");
            return;
        }

        if (currentVideoIndex + 1 < videoClips.Count)
            currentVideoIndex += 1;

        PlayVideo();
    }

    public void Btn_PreviousVideo() {
        EventManager<AudioEvents, string>.Invoke(AudioEvents.PlayAudio, "ui_Click");
        if (currentVideoIndex > 0)
            currentVideoIndex -= 1;

        PlayVideo();
    }
}
