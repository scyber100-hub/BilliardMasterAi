using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Physics;
using BilliardMasterAi.Creator;
using BilliardMasterAi.Recommendation;
using BilliardMasterAi.Replay;
using BilliardMasterAi.Analysis;

namespace BilliardMasterAi.UI
{
    public class ReplayCompareScreenController : MonoBehaviour
    {
        [Header("Table + Balls (local meters)")]
        public Transform tableRoot;
        public Transform cueBall;
        public Transform targetBall;
        public Transform otherBall;

        [Header("Replay")]
        public ShotReplayController replay;
        public BallTrajectoryRecorder recorder; // or provide recorded points via SetActual
        public ErrorReportPresenter errorPresenter;
        public ChoiceVerdictPresenter verdictPresenter;
        public Text scoringText;
        public Text idealScoringText;
        public EventTimelineUI eventTimelineUI;

        [Header("Video Tracking (optional)")]
        public VideoBallTracker videoTracker;
        public bool trackerYellowIsCue = true; // if true: cue=Yellow, obj2=White; else cue=White, obj2=Yellow
        [Tooltip("Alignment search window (seconds) between tracker and recorder times")] public float alignWindow = 0.8f;
        public Text alignStatusText;
        [Header("Alignment Options")]
        public Toggle rigidOnlyToggle;
        public Slider rejectFractionSlider; // 0..0.5

        private bool _hasChosenParams;
        private ShotParams _chosen;
        private AlignmentResult? _lastAlign;
        private float _lastAlignTimeOffset = 0f;

        [Header("Alignment Save/Load (optional)")]
        public InputField alignNameInput;
        public Button saveAlignButton;
        public Button loadAlignButton;

        public void SetChosenParams(float angleDeg, float speed, float spinZ)
        {
            _chosen = new ShotParams { AngleDeg = angleDeg, Speed = speed, SpinZ = spinZ, TipOffset = Vector2.zero };
            _hasChosenParams = true;
        }

        public void RunAnalysis()
        {
            if (replay == null)
            {
                Debug.LogWarning("ReplayCompare: replay controller missing");
                return;
            }

            Vector2 cue = ToTable2D(cueBall.position);
            Vector2 tar = ToTable2D(targetBall.position);
            Vector2 oth = ToTable2D(otherBall.position);

            // Build chosen plan
            ShotPlanResult chosenPlan;
            if (_hasChosenParams)
            {
                var state = new BallState { Position = cue, Velocity = AngleToDir(_chosen.AngleDeg) * _chosen.Speed, SpinZ = _chosen.SpinZ };
                var path = TrajectorySimulator.Simulate(state, 7f, 0.006f, 12);
                chosenPlan = new ShotPlanResult { Parameters = _chosen, Path = path, CushionCount = CountCushions(path), Score = 0f };
            }
            else
            {
                chosenPlan = ShotPlanner.PlanShot(cue, tar, oth);
            }

            // Ideal from plan
            var start = new BallState { Position = cue, Velocity = AngleToDir(chosenPlan.Parameters.AngleDeg) * chosenPlan.Parameters.Speed, SpinZ = chosenPlan.Parameters.SpinZ };
            replay.tableRoot = tableRoot;
            replay.SetIdealFromPlan(start, 8f, 0.01f);

            // Scoring of ideal using detailed simulation (moving object balls)
            var detailed = PhysicsFacade.SimulateCueDetailed(cue, tar, oth, chosenPlan.Parameters.AngleDeg, chosenPlan.Parameters.Speed, chosenPlan.Parameters.SpinZ);
            var idealScore = CaromScorer.EvaluateThreeCushion(detailed.Cue, detailed.Obj1, detailed.Obj2);
            if (idealScoringText)
            {
                idealScoringText.text = idealScore.Success ? $"이상 득점 인정 · 쿠션 {idealScore.CushionBeforeSecond}회 (first {idealScore.FirstObject})" : $"이상 노득점 · {idealScore.Reason}";
            }

            // Actual from recorder
            List<TimedTrajectoryPoint> actualFromRecorder = null;
            if (recorder != null)
            {
                actualFromRecorder = recorder.StopRecording();
                replay.SetActual(actualFromRecorder);
            }

            // Compare and present
            var err = replay.ComputeError();
            errorPresenter?.Show(err);

            var chosenMetrics = ShotEvaluator.Evaluate(chosenPlan, cue, tar, oth);
            var top3 = ShotPlanner.PlanTopShots(cue, tar, oth, 3);
            var bestMetrics = top3.Count > 0 ? ShotEvaluator.Evaluate(top3[0], cue, tar, oth) : chosenMetrics;

            var verdict = ChoiceValidator.Evaluate(chosenMetrics.SuccessProb, chosenMetrics.Risk, bestMetrics.SuccessProb, bestMetrics.Risk, err);
            verdictPresenter?.Show(verdict);

            // Build actual cue path preferring tracker
            if (recorder != null || (videoTracker != null && videoTracker.trajectories != null))
            {
                List<TimedTrajectoryPoint> actual = null;
                if (videoTracker != null && videoTracker.trajectories != null)
                {
                    var cueColor = trackerYellowIsCue ? Perception.BallColor.Yellow : Perception.BallColor.White;
                    if (videoTracker.trajectories.TryGetValue(cueColor, out var cueTracked) && cueTracked != null && cueTracked.Count > 0)
                    {
                        if (actualFromRecorder != null && actualFromRecorder.Count > 0)
                        {
                            var opt = new TrajectoryAligner.AlignOptions { window = alignWindow, dt = 0.02f, rigidOnly = (rigidOnlyToggle && rigidOnlyToggle.isOn), rejectFraction = (rejectFractionSlider ? Mathf.Clamp01(rejectFractionSlider.value) : 0f) };
                            var ar = TrajectoryAligner.Align(actualFromRecorder, cueTracked, opt);
                            actual = ar.AlignedTracked;
                            _lastAlign = ar; _lastAlignTimeOffset = ar.TimeOffset; AlignmentBus.Publish(ar);
                            if (alignStatusText) alignStatusText.text = $"정렬 dt={ar.TimeOffset:+0.00;-0.00}s, d={ar.Offset.magnitude*100f:0.0}cm, RMS={ar.RmsError*100f:0.0}cm";
                        }
                        else actual = cueTracked;
                    }
                }
                if (actual == null) actual = actualFromRecorder;

                if (actual == null || actual.Count == 0)
                {
                    if (scoringText) scoringText.text = "실제 궤적 없음";
                    return;
                }

                // Dashboard metrics
                bool success; float tti;
                Analysis.DashboardHelpers.ComputeSuccessAndTTI(actual, tar, out success, out tti);
                var tag = tableRoot ? tableRoot.GetComponent<Analysis.TableTag>() : null;
                string tableId = tag ? tag.tableId : "table-1";
                Analysis.DashboardService.RecordShot(tableId, success, err.RmsError, tti);

                // Prefer tracked object trajectories and align with same transform
                if (videoTracker != null && videoTracker.trajectories != null && videoTracker.trajectories.Count > 0)
                {
                    var otherColor = trackerYellowIsCue ? Perception.BallColor.White : Perception.BallColor.Yellow;
                    videoTracker.trajectories.TryGetValue(Perception.BallColor.Red, out var redPathRaw);
                    videoTracker.trajectories.TryGetValue(otherColor, out var otherPathRaw);
                    List<TimedTrajectoryPoint> redPath = redPathRaw, otherPath = otherPathRaw;
                    if (_lastAlign.HasValue)
                    {
                        var ar = _lastAlign.Value;
                        if (redPathRaw != null) redPath = ApplyAlignment(ar, redPathRaw);
                        if (otherPathRaw != null) otherPath = ApplyAlignment(ar, otherPathRaw);
                    }
                    var detailedRule = BilliardMasterAi.Rules.RuleEngine.EvaluateDetailed(null, actual, redPath, otherPath);
                    var detailsTracked = detailedRule.verdict;
                    if (scoringText)
                    {
                        scoringText.text = detailsTracked.Success ? $"득점 인정(추적) · 쿠션 {detailsTracked.Cushions}회 (first {detailsTracked.FirstObject})" : $"노득점(추적) · {detailsTracked.Reason}";
                    }
                    eventTimelineUI?.Show(detailedRule.log);
                }
                else
                {
                    var detailedRule = BilliardMasterAi.Rules.RuleEngine.EvaluateDetailed(null, actual, null, null);
                    var details = detailedRule.verdict;
                    if (scoringText)
                    {
                        scoringText.text = details.Success ? $"득점 인정 · 쿠션 {details.Cushions}회 (first {details.FirstObject})" : $"노득점 · {details.Reason}";
                    }
                    eventTimelineUI?.Show(detailedRule.log);
                }
            }
        }

        void Awake()
        {
            if (saveAlignButton) saveAlignButton.onClick.AddListener(SaveAlignment);
            if (loadAlignButton) loadAlignButton.onClick.AddListener(LoadAlignment);
        }

        public void SaveAlignment()
        {
            if (!_lastAlign.HasValue) { if (alignStatusText) alignStatusText.text = "정렬 정보 없음"; return; }
            string name = alignNameInput ? alignNameInput.text : "alignment";
            AlignmentStore.Save(name, _lastAlign.Value);
            if (alignStatusText) alignStatusText.text = $"정렬 저장: {name}";
        }

        public void LoadAlignment()
        {
            string name = alignNameInput ? alignNameInput.text : "alignment";
            if (AlignmentStore.Load(name, out var ar))
            {
                _lastAlign = ar; _lastAlignTimeOffset = ar.TimeOffset; AlignmentBus.Publish(ar);
                if (alignStatusText) alignStatusText.text = $"정렬 로드: dt={ar.TimeOffset:+0.00;-0.00}s, d={ar.Offset.magnitude*100f:0.0}cm";
            }
            else
            {
                if (alignStatusText) alignStatusText.text = "정렬 파일 없음";
            }
        }

        private Vector2 ToTable2D(Vector3 world)
        {
            if (tableRoot == null) return new Vector2(world.x, world.z);
            var local = tableRoot.InverseTransformPoint(world);
            return new Vector2(local.x, local.z);
        }

        private Vector2 AngleToDir(float deg)
        {
            float r = deg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(r), Mathf.Sin(r)).normalized;
        }

        private int CountCushions(List<TrajectoryPoint> path)
        {
            int c = 0; foreach (var p in path) if (p.IsCushion) c++; return c;
        }

        private List<TimedTrajectoryPoint> ApplyAlignment(AlignmentResult ar, List<TimedTrajectoryPoint> raw)
        {
            float c = Mathf.Cos(ar.AngleRad), s = Mathf.Sin(ar.AngleRad);
            var list = new List<TimedTrajectoryPoint>(raw.Count);
            for (int i = 0; i < raw.Count; i++)
            {
                var tp = raw[i];
                var rot = new Vector2(c * tp.Position.x - s * tp.Position.y, s * tp.Position.x + c * tp.Position.y);
                var p = ar.Scale * rot + ar.Offset;
                list.Add(new TimedTrajectoryPoint { Position = p, Time = tp.Time + ar.TimeOffset, IsCushion = tp.IsCushion });
            }
            return list;
        }
    }
}
