using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Physics;
using BilliardMasterAi.UI;

namespace BilliardMasterAi.Replay
{
    // 리플레이 타임라인 에디터: 스크럽/플레이/구간 선택, 주석 토글
    public class ReplayTimelineEditor : MonoBehaviour
    {
        public ShotReplayController replay;
        public Slider timeline;
        public Toggle showIdeal;
        public Toggle showActual;
        public Button playButton;
        public Button pauseButton;
        public Button stepFwdButton;
        public Button stepBackButton;

        public PathAnnotationRenderer annotations;
        public Toggle showAnnotations;

        private float _duration = 5f;

        void Awake()
        {
            if (playButton) playButton.onClick.AddListener(()=> replay?.Play());
            if (pauseButton) pauseButton.onClick.AddListener(()=> replay?.Pause());
            if (stepFwdButton) stepFwdButton.onClick.AddListener(()=>Step(0.05f));
            if (stepBackButton) stepBackButton.onClick.AddListener(()=>Step(-0.05f));
            if (timeline) timeline.onValueChanged.AddListener(OnTimelineChanged);
            if (showAnnotations) showAnnotations.onValueChanged.AddListener(OnToggleAnnotations);
        }

        private void OnTimelineChanged(float v)
        {
            if (replay == null) return;
            float dur = Mathf.Max(0.001f, replay.Duration);
            replay.Seek(dur * v);
        }

        private void Step(float delta)
        {
            if (replay == null) return;
            float t = Mathf.Clamp(replay.Duration * (timeline ? timeline.value : 0f) + delta, 0f, replay.Duration);
            replay.Seek(t);
            if (timeline && replay.Duration > 0f) timeline.value = t / replay.Duration;
        }

        public void BuildAnnotationsFromIdeal(List<TrajectoryPoint> path)
        {
            if (annotations == null) return;
            if (showAnnotations && showAnnotations.isOn) annotations.Show(path);
        }

        private void OnToggleAnnotations(bool on)
        {
            if (!on) annotations?.Clear();
        }
    }
}
