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
        videoPlayer.loopPointReached += OnVideoEnd;
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

    private void OnVideoEnd(VideoPlayer vp) {
        // Video has ended, do something (e.g., automatically play the next video).
        if (currentVideoIndex + 1 >= videoClips.Count) {
            SceneManager.LoadScene(1);
            return;
        }
    }

    public void Btn_NextVideo() {
        if (currentVideoIndex + 1 < videoClips.Count)
            currentVideoIndex += 1;

        PlayVideo();
    }

    public void Btn_PreviousVideo() {
        if (currentVideoIndex > 0)
            currentVideoIndex -= 1;

        PlayVideo();
    }
}
