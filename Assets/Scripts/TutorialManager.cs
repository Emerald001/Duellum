using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class TutorialManager : MonoBehaviour {
    public VideoPlayer videoPlayer;
    public RawImage rawImage;
    public TextMeshProUGUI videoTitleText;
    public TextMeshProUGUI videoDescriptionText;

    public string[] videoTitles;
    public string[] videoDescriptions;

    public List<VideoClip> videoClips;
    private int currentVideoIndex = 0;

    private void Start() {
        videoPlayer.loopPointReached += OnVideoEnd;
        PlayVideo();
    }

    public void PlayVideo() {
        videoPlayer.clip = videoClips[currentVideoIndex];
        videoPlayer.Prepare();
        videoPlayer.Play();

        // Update the video title and description
        videoTitleText.text = videoTitles[currentVideoIndex];
        videoDescriptionText.text = videoDescriptions[currentVideoIndex];
    }

    public void OnVideoEnd(VideoPlayer vp) {
        // Video has ended, do something (e.g., automatically play the next video).
        if (currentVideoIndex + 1 >= videoClips.Count) {
            SceneManager.LoadScene(1);
            return;
        }

        //NextVideo();
    }

    public void NextVideo() {
        if (currentVideoIndex + 1 < videoClips.Count)
            currentVideoIndex += 1;

        PlayVideo();
    }

    public void PreviousVideo() {
        if (currentVideoIndex > 0)
            currentVideoIndex -= 1;

        PlayVideo();
    }
}
