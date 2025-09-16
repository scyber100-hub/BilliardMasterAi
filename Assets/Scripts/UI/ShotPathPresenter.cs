using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Physics;
using BilliardMasterAi.Recommendation;

namespace BilliardMasterAi.UI
{
    [RequireComponent(typeof(LineRenderer))]
    public class ShotPathPresenter : MonoBehaviour
    {
        public LineRenderer line;
        public Text thicknessText;
        public Text tipText;
        public Text powerText;
        public Color pathColor = new Color(0.2f, 0.7f, 1f);
        public float zOffset = 0.012f;

        void Reset()
        {
            line = GetComponent<LineRenderer>();
            if (line != null)
            {
                line.positionCount = 0;
                line.widthMultiplier = 0.012f;
            }
        }

        public void Present(ShotPlanResult plan, Vector2 cuePos, Vector2 targetPos)
        {
            if (line == null) line = GetComponent<LineRenderer>();
            DrawPath(plan.Path);

            float thickness = EstimateThicknessPercent(cuePos, targetPos, plan.Parameters.AngleDeg);
            if (thicknessText) thicknessText.text = $"두께 {Mathf.RoundToInt(thickness)}%";

            var (label, pct) = TipLabel(plan.Parameters.SpinZ);
            if (tipText) tipText.text = $"당점 {label} {Mathf.RoundToInt(pct)}%";

            int power = Mathf.RoundToInt(Normalize(plan.Parameters.Speed, 1.5f, 3.5f) * 100f);
            if (powerText) powerText.text = $"세기 {power}%";

            if (line != null)
            {
                line.startColor = pathColor;
                line.endColor = pathColor;
            }
        }

        private void DrawPath(List<TrajectoryPoint> path)
        {
            if (path == null || path.Count == 0 || line == null) return;
            line.positionCount = path.Count;
            for (int i = 0; i < path.Count; i++)
            {
                var p = path[i].Position;
                line.SetPosition(i, new Vector3(p.x, zOffset, p.y));
            }
        }

        private float EstimateThicknessPercent(Vector2 cuePos, Vector2 targetPos, float angleDeg)
        {
            Vector2 dirShot = new Vector2(Mathf.Cos(angleDeg * Mathf.Deg2Rad), Mathf.Sin(angleDeg * Mathf.Deg2Rad)).normalized;
            Vector2 dirToObj = (targetPos - cuePos).normalized;
            float cos = Mathf.Clamp01(Vector2.Dot(dirShot, dirToObj));
            return cos * 100f; // approx: 100%=정면 두께, 0%=퍼지
        }

        private (string, float) TipLabel(float spinZ)
        {
            float pct = Mathf.Clamp01(Mathf.Abs(spinZ) / 20f) * 100f;
            string label = spinZ >= 0f ? "상단" : "하단";
            return (label, pct);
        }

        private float Normalize(float v, float min, float max)
        {
            return Mathf.Clamp01((v - min) / Mathf.Max(0.0001f, max - min));
        }
    }
}

