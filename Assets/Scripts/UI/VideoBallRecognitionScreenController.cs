using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using BilliardMasterAi.Creator;
using BilliardMasterAi.Physics;

namespace BilliardMasterAi.UI
{
    // 영상 속 공 위치 자동 인식 + 궤적 추출 화면 컨트롤러
    public class VideoBallRecognitionScreenController : MonoBehaviour
    {
        [Header("Refs")]
        public VideoPlayer player;
        public VideoTableMapper mapper;
        public VideoBallTracker tracker;

        [Header("UI")] 
        public InputField startTimeInput; // seconds
        public InputField endTimeInput;   // seconds
        public Button calibrateButton;
        public Button trackButton;
        public Text statusText;

        [Header("Renderers")] 
        public TimedPathRenderer whitePath;
        public TimedPathRenderer yellowPath;
        public TimedPathRenderer redPath;

        void Awake()
        {
            if (trackButton) trackButton.onClick.AddListener(OnTrackClicked);
        }

        public void OnTrackClicked()
        {
            if (player == null || mapper == null || tracker == null) { SetStatus("구성 요소 누락"); return; }
            if (!player.isPrepared && player.clip == null && string.IsNullOrEmpty(player.url)) { SetStatus("영상이 로드되지 않았습니다."); return; }

            double start = 0, end = player.length;
            if (startTimeInput && double.TryParse(startTimeInput.text, out var s)) start = Mathf.Max(0f, (float)s);
            if (endTimeInput && double.TryParse(endTimeInput.text, out var e)) end = Mathf.Clamp((float)e, (float)start + 0.5f, (float)player.length);

            tracker.player = player;
            tracker.mapper = mapper;
            tracker.StartTrack(start, end);
            SetStatus($"추적 시작: {start:0.00}s ~ {end:0.00}s");
            StopAllCoroutines();
            StartCoroutine(WaitAndRender((float)(end - start) + 0.2f));
        }

        private System.Collections.IEnumerator WaitAndRender(float wait)
        {
            yield return new WaitForSeconds(wait);
            RenderPaths();
            SetStatus("추적 완료");
        }

        private void RenderPaths()
        {
            if (tracker.trajectories.TryGetValue(BilliardMasterAi.Perception.BallColor.White, out var w) && whitePath)
                whitePath.Draw(w);
            if (tracker.trajectories.TryGetValue(BilliardMasterAi.Perception.BallColor.Yellow, out var y) && yellowPath)
                yellowPath.Draw(y);
            if (tracker.trajectories.TryGetValue(BilliardMasterAi.Perception.BallColor.Red, out var r) && redPath)
                redPath.Draw(r);
        }

        private void SetStatus(string msg)
        {
            if (statusText) statusText.text = msg;
            Debug.Log(msg);
        }
    }
}

