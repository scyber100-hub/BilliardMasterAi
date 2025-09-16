using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Perception;
using BilliardMasterAi.Recommendation;

namespace BilliardMasterAi.UI
{
    // 학생이 카메라 인식 → 즉시 시뮬레이션/가이드 실행하는 화면 컨트롤러
    public class StudentTrainingScreenController : MonoBehaviour
    {
        [Header("Recognition + Table")]
        public Transform tableRoot;
        public BallRecognitionController recognition;

        [Header("Overlay/Presenters")]
        public BilliardMasterAi.AR.ARGuideOverlay overlay; // AR 오버레이 라인
        public ShotPathPresenter pathA; // 선택 사항(화면 라인 표시)
        public ShotPathPresenter pathB; // 선택 사항(비교용)

        [Header("UI")]
        public Button detectAndSimulateButton;
        public Text statusText;

        void Awake()
        {
            if (detectAndSimulateButton != null)
                detectAndSimulateButton.onClick.AddListener(DetectAndSimulate);
        }

        public void DetectAndSimulate()
        {
            if (recognition == null)
            {
                SetStatus("인식 컨트롤러가 없습니다.");
                return;
            }

            // 1) 카메라 인식 즉시 수행
            recognition.CaptureAndDetect();
            if (recognition.placeTransforms)
                recognition.ApplyTransformsNow();

            // 2) 좌표 취득(테이블 로컬 XZ)
            var cueT = recognition.yellowIsCueBall ? recognition.yellowBall : recognition.whiteBall;
            var obj1T = recognition.redBall; // 일반적으로 적구
            var obj2T = recognition.yellowIsCueBall ? recognition.whiteBall : recognition.yellowBall;

            if (cueT == null || obj1T == null || obj2T == null)
            {
                SetStatus("공 트랜스폼 참조가 누락되었습니다.");
                return;
            }

            Vector2 cue = ToTable2D(cueT.position);
            Vector2 tar = ToTable2D(obj1T.position);
            Vector2 oth = ToTable2D(obj2T.position);

            // 3) 추천 경로 계산(상위 2개)
            var top2 = ShotPlanner.PlanTopShots(cue, tar, oth, 2);
            if (top2.Count == 0)
            {
                SetStatus("추천 경로를 찾지 못했습니다.");
                return;
            }

            // 4) AR 오버레이 표시(최적 1개)
            if (overlay != null)
            {
                overlay.tableRoot = tableRoot;
                overlay.ShowPlan(top2[0]);
            }

            // 5) 화면 라인 표시(옵션)
            if (pathA != null) pathA.Present(top2[0], cue, tar);
            if (top2.Count > 1 && pathB != null) pathB.Present(top2[1], cue, tar);

            SetStatus($"인식/시뮬레이션 완료 · 후보 {top2.Count}개");
        }

        private Vector2 ToTable2D(Vector3 world)
        {
            if (tableRoot == null) return new Vector2(world.x, world.z);
            var local = tableRoot.InverseTransformPoint(world);
            return new Vector2(local.x, local.z);
        }

        private void SetStatus(string msg)
        {
            if (statusText) statusText.text = msg;
        }
    }
}

