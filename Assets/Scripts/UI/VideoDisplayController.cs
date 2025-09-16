using UnityEngine;
using UnityEngine.UI;

namespace BilliardMasterAi.UI
{
    // Binds a VideoPlayer's texture to a RawImage each frame; basic URL load/play controls.
    public class VideoDisplayController : MonoBehaviour
    {
        public UnityEngine.Video.VideoPlayer videoPlayer;
        public RawImage rawImage;
        public InputField urlInput;
        public Button loadButton;
        public Button playButton;
        public Button pauseButton;

        void Awake()
        {
            if (loadButton) loadButton.onClick.AddListener(LoadUrl);
            if (playButton) playButton.onClick.AddListener(()=>{ if (videoPlayer) videoPlayer.Play(); });
            if (pauseButton) pauseButton.onClick.AddListener(()=>{ if (videoPlayer) videoPlayer.Pause(); });
        }

        void Update()
        {
            if (videoPlayer != null && rawImage != null && videoPlayer.texture != null)
            {
                if (rawImage.texture != videoPlayer.texture)
                    rawImage.texture = videoPlayer.texture;
            }
        }

        public void LoadUrl()
        {
            if (videoPlayer == null || urlInput == null) return;
            var url = urlInput.text;
            if (string.IsNullOrEmpty(url)) return;
            videoPlayer.source = UnityEngine.Video.VideoSource.Url;
            videoPlayer.url = url;
            videoPlayer.Prepare();
        }
    }
}

