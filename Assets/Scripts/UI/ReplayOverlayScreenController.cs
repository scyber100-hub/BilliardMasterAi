using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Creator;
using BilliardMasterAi.Physics;
using BilliardMasterAi.Recommendation;

namespace BilliardMasterAi.UI
{
    // 비디오 위 오버레이: 실제 궤적 vs 이상(추천) 궤적 자동 시각화
    public class ReplayOverlayScreenController : MonoBehaviour
    {
        [Header("Refs")]
        public RawImage videoImage;
        public VideoTableMapper mapper;
        public VideoBallTracker tracker;

        [Header("Overlay Renderers")] 
        public ImagePathRenderer actualRenderer;
        public ImagePathRenderer idealRenderer;

        [Header("Cue/Target Selection")] 
        public bool yellowIsCue = true; // else white

        [Header("UI")] 
        public Button overlayButton;
        public Text statusText;

        void Awake()
        {
            if (overlayButton) overlayButton.onClick.AddListener(RenderOverlay);
            if (actualRenderer) actualRenderer.targetImage = videoImage;
            if (idealRenderer) idealRenderer.targetImage = videoImage;
        }

        public void RenderOverlay()
        {
            if (videoImage == null || mapper == null || tracker == null) { SetStatus("구성 요소 누락"); return; }
            // 1) 실제 궤적: 최근 추적 결과 사용
            if (!tracker.trajectories.TryGetValue(yellowIsCue ? BilliardMasterAi.Perception.BallColor.Yellow : BilliardMasterAi.Perception.BallColor.White, out var actual))
            {
                SetStatus("실제 궤적 데이터가 없습니다. 먼저 추적을 실행하세요.");
                return;
            }
            var actual2D = new List<Vector2>(actual.Count); foreach (var p in actual) actual2D.Add(p.Position);
            actualRenderer?.DrawTablePoints(actual2D, mapper);

            // 2) 이상 궤적: 현재 배치에서 추천 경로 계산
            // 추적 데이터에서 첫 프레임 기준 위치 추정
            Vector2 cuePos = actual2D.Count > 0 ? actual2D[0] : Vector2.zero;
            // 다른 공: 빨강/남은 하나 중 가까이 있는 것 택1 (간단화)
            tracker.trajectories.TryGetValue(BilliardMasterAi.Perception.BallColor.Red, out var redTraj);
            tracker.trajectories.TryGetValue(yellowIsCue ? BilliardMasterAi.Perception.BallColor.White : BilliardMasterAi.Perception.BallColor.Yellow, out var otherTraj);
            Vector2 redPos = (redTraj != null && redTraj.Count > 0) ? redTraj[0].Position : cuePos + new Vector2(0.1f, 0.1f);
            Vector2 otherPos = (otherTraj != null && otherTraj.Count > 0) ? otherTraj[0].Position : cuePos + new Vector2(-0.1f, -0.1f);

            var best = ShotPlanner.PlanShot(cuePos, redPos, otherPos);
            var ideal2D = new List<Vector2>(best.Path.Count); foreach (var tp in best.Path) ideal2D.Add(tp.Position);
            idealRenderer?.DrawTablePoints(ideal2D, mapper);

            SetStatus($"오버레이 완료 · 실제 {actual2D.Count}pts / 이상 {ideal2D.Count}pts");
        }

        private void SetStatus(string msg)
        {
            if (statusText) statusText.text = msg;
            Debug.Log(msg);
        }
    }
}

