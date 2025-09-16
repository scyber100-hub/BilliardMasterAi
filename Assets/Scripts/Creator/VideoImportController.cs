using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using BilliardMasterAi.Creator;

namespace BilliardMasterAi.Creator
{
    // 경기 영상을 불러오고 재생/스크럽 컨트롤을 제공
    public class VideoImportController : MonoBehaviour
    {
        [Header("Video")]
        public VideoPlayer player;
        public RawImage display;

        [Header("Controls")]
        public InputField pathInput;   // 파일 경로 입력(에디터/데스크탑), 모바일은 사전 배치/다운로드 경로 사용
        public Button loadButton;
        public Button playPauseButton;
        public Button stopButton;
        public Slider timeSlider;
        public Text timeText;

        private bool _isDragging;

        void Awake()
        {
            if (loadButton) loadButton.onClick.AddListener(LoadFromPath);
            if (playPauseButton) playPauseButton.onClick.AddListener(TogglePlayPause);
            if (stopButton) stopButton.onClick.AddListener(StopPlayback);
            if (timeSlider)
            {
                timeSlider.minValue = 0f;
                timeSlider.onValueChanged.AddListener(OnSliderChanged);
            }
            if (player != null)
            {
                player.prepareCompleted += OnPrepared;
                player.errorReceived += (vp, msg) => { if (timeText) timeText.text = $"오류: {msg}"; };
                player.loopPointReached += vp => { /* keep last frame */ };
            }
        }

        void Update()
        {
            if (player == null || player.clip == null && string.IsNullOrEmpty(player.url)) return;
            if (!_isDragging && player.isPrepared)
            {
                var t = player.time;
                var d = Math.Max(0.001, player.length);
                if (timeSlider) timeSlider.value = (float)(t / d);
                if (timeText) timeText.text = $"{TimeUtil.FormatTime(t)} / {TimeUtil.FormatTime(d)}";
            }
            if (display != null && player != null && player.texture != null)
            {
                display.texture = player.texture;
            }
        }

        public void LoadFromPath()
        {
            if (player == null) return;
            var path = pathInput != null ? pathInput.text : string.Empty;
            if (string.IsNullOrEmpty(path))
            {
                if (timeText) timeText.text = "경로를 입력하세요.";
                return;
            }
            player.source = VideoSource.Url;
            player.url = path;
            player.Prepare();
        }

        private void OnPrepared(VideoPlayer vp)
        {
            if (timeSlider) timeSlider.value = 0f;
            if (timeText) timeText.text = $"{TimeUtil.FormatTime(0)} / {TimeUtil.FormatTime(player.length)}";
        }

        public void TogglePlayPause()
        {
            if (player == null || !player.isPrepared) return;
            if (player.isPlaying) player.Pause(); else player.Play();
        }

        public void StopPlayback()
        {
            if (player == null) return;
            player.Stop();
            if (timeSlider) timeSlider.value = 0f;
        }

        private void OnSliderChanged(float v)
        {
            if (player == null || !player.isPrepared) return;
            if (!_isDragging) return; // we change time only on end drag
        }

        // UI Event: called on BeginDrag
        public void OnSliderBeginDrag() { _isDragging = true; }
        // UI Event: called on EndDrag
        public void OnSliderEndDrag()
        {
            if (player == null || !player.isPrepared) { _isDragging = false; return; }
            var d = Math.Max(0.001, player.length);
            player.time = d * timeSlider.value;
            _isDragging = false;
        }
    }
}

