using System.Collections.Generic;
using UnityEngine;
using BilliardMasterAi.Physics;

namespace BilliardMasterAi.Recommendation
{
    public struct ShotMetrics
    {
        public float SuccessProb; // 0..1
        public float Risk;        // 0..1 (higher = riskier)
    }

    public static class ShotEvaluator
    {
        private static BilliardMasterAi.Analytics.LogisticModel _successModel;
        public static void SetSuccessModel(BilliardMasterAi.Analytics.LogisticModel model) => _successModel = model;

        // Lightweight heuristic estimator for success probability and risk. Supports risk profiles.
        public static ShotMetrics Evaluate(ShotPlanResult plan, Vector2 cuePos, Vector2 targetPos, Vector2 otherPos, RiskProfile? profile = null)
        {
            var prof = profile ?? RiskProfile.Neutral;
            var path = plan.Path;
            int cushions = 0; for (int i = 0; i < path.Count; i++) if (path[i].IsCushion) cushions++;
            float len = PathLength(path);
            float proxTarget = DistanceToPath(targetPos, path);
            float proxOther = DistanceToPath(otherPos, path);
            float spinMag = Mathf.Abs(plan.Parameters.SpinZ);

            // Normalize terms
            float cushionsOk = Mathf.Clamp01((cushions >= 3 ? 1f : 0.3f * cushions) * prof.cushionsWeight);
            float lenPenalty = Mathf.Clamp01((len / 8f) * prof.lengthPenalty);
            float spinPenalty = Mathf.Clamp01((spinMag / 25f) * prof.spinPenalty);
            float targetBonus = Mathf.Clamp01((Mathf.Max(0f, 1.2f - proxTarget) / 1.2f) * prof.targetBonus);
            float otherPenalty = Mathf.Clamp01((Mathf.Max(0f, 1.0f - proxOther)) * prof.otherPenalty);

            // Success probability via model if available, else heuristic
            float success;
            if (_successModel != null)
            {
                var features = new float[] { cushions, len, spinMag, proxTarget, proxOther };
                success = Mathf.Clamp01(_successModel.Predict(features));
            }
            else
            {
                success = 0.15f + 0.55f * cushionsOk + 0.25f * targetBonus - 0.25f * lenPenalty - 0.20f * spinPenalty;
                success = Mathf.Clamp01(success);
            }

            // Risk heuristic
            float risk = 0.20f + 0.50f * otherPenalty + 0.20f * lenPenalty + 0.20f * spinPenalty;
            // Slightly tie to (1-success)
            risk = Mathf.Clamp01(0.7f * risk + 0.3f * (1f - success));

            return new ShotMetrics { SuccessProb = success, Risk = risk };
        }

        public static float ExpectedValue(ShotMetrics m, float pointValue = 1f, float riskPenalty = 0.5f)
        {
            // Simple EV: P(success)*points - Risk*penalty
            return Mathf.Clamp01(m.SuccessProb) * pointValue - Mathf.Clamp01(m.Risk) * riskPenalty;
        }

        private static float PathLength(List<TrajectoryPoint> pts)
        {
            float len = 0f; for (int i = 1; i < pts.Count; i++) len += Vector2.Distance(pts[i - 1].Position, pts[i].Position); return len;
        }

        private static float DistanceToPath(Vector2 p, List<TrajectoryPoint> path)
        {
            float d = float.PositiveInfinity;
            for (int i = 1; i < path.Count; i++)
            {
                var a = path[i - 1].Position; var b = path[i].Position;
                d = Mathf.Min(d, DistancePointToSegment(p, a, b));
            }
            return d;
        }

        private static float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a; var ap = p - a;
            float t = Vector2.Dot(ap, ab) / Mathf.Max(ab.sqrMagnitude, 1e-6f);
            t = Mathf.Clamp01(t);
            Vector2 q = a + t * ab;
            return Vector2.Distance(p, q);
        }
    }
}
