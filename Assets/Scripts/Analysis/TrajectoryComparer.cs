using System.Collections.Generic;
using UnityEngine;
using BilliardMasterAi.Analysis;

namespace BilliardMasterAi.Analysis
{
    public static class TrajectoryComparer
    {
        public static ErrorReport Compare(List<Vector2> ideal, List<Vector2> actual)
        {
            int N = 100;
            var a1 = Resample(ideal, N);
            var a2 = Resample(actual, N);

            float sum2 = 0f, max = 0f;
            for (int i = 0; i < N; i++)
            {
                float d = Vector2.Distance(a1[i], a2[i]);
                sum2 += d * d; if (d > max) max = d;
            }
            float rms = Mathf.Sqrt(sum2 / N);

            float finalOff = Vector2.Distance(ideal.Count > 0 ? ideal[^1] : Vector2.zero, actual.Count > 0 ? actual[^1] : Vector2.zero);

            float lenIdeal = PathLength(ideal);
            float lenActual = PathLength(actual);
            int cIdeal = CountCushions(ideal);
            int cActual = CountCushions(actual);

            return new ErrorReport
            {
                RmsError = rms,
                MaxError = max,
                FinalOffset = finalOff,
                CushionDiff = cActual - cIdeal,
                PathLenIdeal = lenIdeal,
                PathLenActual = lenActual
            };
        }

        private static float PathLength(List<Vector2> pts)
        {
            float len = 0f; for (int i = 1; i < pts.Count; i++) len += Vector2.Distance(pts[i - 1], pts[i]); return len;
        }

        private static List<Vector2> Resample(List<Vector2> pts, int N)
        {
            var outPts = new List<Vector2>(N);
            if (pts == null || pts.Count == 0)
            {
                for (int i = 0; i < N; i++) outPts.Add(Vector2.zero);
                return outPts;
            }
            // cumulative arc lengths
            float total = 0f;
            var acc = new float[pts.Count]; acc[0] = 0f;
            for (int i = 1; i < pts.Count; i++) { total += Vector2.Distance(pts[i - 1], pts[i]); acc[i] = total; }
            if (total <= 1e-6f) { for (int i = 0; i < N; i++) outPts.Add(pts[0]); return outPts; }

            int j = 1; for (int i = 0; i < N; i++)
            {
                float t = (i / (float)(N - 1)) * total;
                while (j < pts.Count && acc[j] < t) j++;
                int j0 = Mathf.Clamp(j - 1, 0, pts.Count - 1);
                int j1 = Mathf.Clamp(j, 0, pts.Count - 1);
                float seg = Mathf.Max(1e-6f, acc[j1] - acc[j0]);
                float u = Mathf.Clamp01((t - acc[j0]) / seg);
                outPts.Add(Vector2.Lerp(pts[j0], pts[j1], u));
            }
            return outPts;
        }

        private static int CountCushions(List<Vector2> pts)
        {
            if (pts == null || pts.Count < 2) return 0;
            float halfW = Physics.CaromConstants.TableWidth * 0.5f - Physics.CaromConstants.BallRadius;
            float halfH = Physics.CaromConstants.TableHeight * 0.5f - Physics.CaromConstants.BallRadius;
            float eps = 0.02f;
            int hits = 0; bool nearPrev = false;
            foreach (var p in pts)
            {
                bool near = Mathf.Abs(p.x) > halfW - eps || Mathf.Abs(p.y) > halfH - eps;
                if (near && !nearPrev) hits++;
                nearPrev = near;
            }
            return hits;
        }
    }
}

