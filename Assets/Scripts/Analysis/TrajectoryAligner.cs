using System;
using System.Collections.Generic;
using UnityEngine;
using BilliardMasterAi.Physics;

namespace BilliardMasterAi.Analysis
{
    public struct AlignmentResult
    {
        public float TimeOffset;   // tracked time shift to align to reference
        public Vector2 Offset;     // additive XY offset to align positions
        public float Scale;        // uniform scale (1=none)
        public float AngleRad;     // rotation angle (radians)
        public float RmsError;     // after alignment (meters)
        public List<TimedTrajectoryPoint> AlignedTracked; // shifted/translated copy
    }

    public static class TrajectoryAligner
    {
        public struct AlignOptions
        {
            public float window;         // seconds
            public float dt;             // search step
            public bool rigidOnly;       // if true, scale=1
            public float rejectFraction; // 0..0.5, trim worst residuals fraction
        }

        private static float TrimmedRms(List<Vector2> refs, List<Vector2> est, float rejectFraction)
        {
            int n = Mathf.Min(refs.Count, est.Count);
            if (n == 0) return 9999f;
            var errs = new List<float>(n);
            for (int i = 0; i < n; i++) errs.Add(Vector2.Distance(refs[i], est[i]));
            errs.Sort();
            int keep = Mathf.Max(1, n - Mathf.RoundToInt(n * Mathf.Clamp01(rejectFraction)));
            float s2 = 0f; for (int i = 0; i < keep; i++) s2 += errs[i] * errs[i];
            return Mathf.Sqrt(s2 / keep);
        }

        public static AlignmentResult Align(List<TimedTrajectoryPoint> reference, List<TimedTrajectoryPoint> tracked, AlignOptions opt)
        {
            var best = new AlignmentResult { TimeOffset = 0f, Offset = Vector2.zero, Scale = 1f, AngleRad = 0f, RmsError = float.PositiveInfinity, AlignedTracked = new List<TimedTrajectoryPoint>() };
            if (reference == null || tracked == null || reference.Count == 0 || tracked.Count == 0) return best;
            float window = opt.window <= 0f ? 1.0f : opt.window;
            float step = opt.dt <= 0f ? 0.02f : opt.dt;
            for (float shift = -window; shift <= window + 1e-6f; shift += step)
            {
                var refs = new List<Vector2>(); var trks = new List<Vector2>();
                foreach (var rp in reference) if (TrySample(tracked, rp.Time + shift, out var tp)) { refs.Add(rp.Position); trks.Add(tp); }
                if (refs.Count < 10) continue;

                Vector2 mr = Mean(refs); Vector2 mt = Mean(trks);
                float sxx = 0f, syy = 0f, sxy = 0f, syx = 0f, tnorm = 0f;
                for (int i = 0; i < refs.Count; i++)
                {
                    var rc = refs[i] - mr; var tc = trks[i] - mt;
                    sxx += tc.x * rc.x; sxy += tc.x * rc.y; syx += tc.y * rc.x; syy += tc.y * rc.y;
                    tnorm += tc.x * tc.x + tc.y * tc.y;
                }
                float a = sxx + syy; float b = sxy - syx; float angle = Mathf.Atan2(b, a); float cos = Mathf.Cos(angle), sin = Mathf.Sin(angle);
                float trace = cos * (sxx + syy) + sin * (sxy - syx);
                float scale = opt.rigidOnly ? 1f : ((tnorm > 1e-6f) ? (trace / tnorm) : 1f);
                Vector2 offset = mr - scale * new Vector2(cos * mt.x - sin * mt.y, sin * mt.x + cos * mt.y);

                var est = new List<Vector2>(trks.Count);
                for (int i = 0; i < trks.Count; i++)
                {
                    var tpt = trks[i]; var rot = new Vector2(cos * tpt.x - sin * tpt.y, sin * tpt.x + cos * tpt.y); est.Add(scale * rot + offset);
                }
                float rms = TrimmedRms(refs, est, Mathf.Clamp01(opt.rejectFraction));
                if (rms < best.RmsError)
                {
                    best.TimeOffset = shift; best.Offset = offset; best.Scale = scale; best.AngleRad = angle; best.RmsError = rms;
                }
            }
            // Build aligned
            float c = Mathf.Cos(best.AngleRad), s = Mathf.Sin(best.AngleRad);
            var aligned = new List<TimedTrajectoryPoint>(tracked.Count);
            foreach (var tp in tracked)
            {
                var rot = new Vector2(c * tp.Position.x - s * tp.Position.y, s * tp.Position.x + c * tp.Position.y);
                var p = best.Scale * rot + best.Offset;
                aligned.Add(new TimedTrajectoryPoint { Position = p, Time = tp.Time + best.TimeOffset, IsCushion = tp.IsCushion });
            }
            best.AlignedTracked = aligned; return best;
        }
        // Align tracked to reference by estimating time shift (±window) and XY translation to minimize RMS.
        public static AlignmentResult AlignTimeAndOffset(List<TimedTrajectoryPoint> reference, List<TimedTrajectoryPoint> tracked, float window = 1.0f, float dt = 0.02f)
        {
            var best = new AlignmentResult { TimeOffset = 0f, Offset = Vector2.zero, Scale = 1f, AngleRad = 0f, RmsError = float.PositiveInfinity, AlignedTracked = new List<TimedTrajectoryPoint>() };
            if (reference == null || tracked == null || reference.Count == 0 || tracked.Count == 0)
                return best;

            for (float shift = -window; shift <= window + 1e-6f; shift += dt)
            {
                // Compute mean translation over overlapping times
                var refs = new List<Vector2>();
                var trks = new List<Vector2>();
                foreach (var rp in reference)
                {
                    if (TrySample(tracked, rp.Time + shift, out var tp)) { refs.Add(rp.Position); trks.Add(tp); }
                }
                if (refs.Count < 10) continue;

                Vector2 meanRef = Mean(refs); Vector2 meanTrk = Mean(trks);
                Vector2 offset = meanRef - meanTrk;

                // Compute RMS with this shift+offset
                float s2 = 0f; int n = refs.Count;
                for (int i = 0; i < n; i++)
                {
                    float d = Vector2.Distance(refs[i], trks[i] + offset);
                    s2 += d * d;
                }
                float rms = Mathf.Sqrt(s2 / n);
                if (rms < best.RmsError)
                {
                    best.TimeOffset = shift; best.Offset = offset; best.Scale = 1f; best.AngleRad = 0f; best.RmsError = rms;
                }
            }

            // Build aligned copy with best shift/offset
            var aligned = new List<TimedTrajectoryPoint>(tracked.Count);
            foreach (var tp in tracked)
            {
                aligned.Add(new TimedTrajectoryPoint { Position = tp.Position + best.Offset, Time = tp.Time + best.TimeOffset, IsCushion = tp.IsCushion });
            }
            best.AlignedTracked = aligned;
            return best;
        }

        // Full similarity alignment: time shift + uniform scale + rotation + translation
        public static AlignmentResult AlignSimilarityTime(List<TimedTrajectoryPoint> reference, List<TimedTrajectoryPoint> tracked, float window = 1.0f, float dt = 0.02f)
        {
            var best = new AlignmentResult { TimeOffset = 0f, Offset = Vector2.zero, Scale = 1f, AngleRad = 0f, RmsError = float.PositiveInfinity, AlignedTracked = new List<TimedTrajectoryPoint>() };
            if (reference == null || tracked == null || reference.Count == 0 || tracked.Count == 0)
                return best;

            for (float shift = -window; shift <= window + 1e-6f; shift += dt)
            {
                var refs = new List<Vector2>();
                var trks = new List<Vector2>();
                foreach (var rp in reference)
                {
                    if (TrySample(tracked, rp.Time + shift, out var tp)) { refs.Add(rp.Position); trks.Add(tp); }
                }
                if (refs.Count < 10) continue;

                // Procrustes in 2D (similarity)
                Vector2 mr = Mean(refs); Vector2 mt = Mean(trks);
                float sxx = 0f, syy = 0f, sxy = 0f, syx = 0f; // covariance terms H = Σ (t_c)(r_c)^T
                float tnorm = 0f;
                for (int i = 0; i < refs.Count; i++)
                {
                    var rc = refs[i] - mr; var tc = trks[i] - mt;
                    sxx += tc.x * rc.x; sxy += tc.x * rc.y;
                    syx += tc.y * rc.x; syy += tc.y * rc.y;
                    tnorm += tc.x * tc.x + tc.y * tc.y;
                }
                // Rotation angle
                float a = sxx + syy; float b = sxy - syx;
                float angle = Mathf.Atan2(b, a);
                float cos = Mathf.Cos(angle), sin = Mathf.Sin(angle);
                // Scale
                float trace = cos * (sxx + syy) + sin * (sxy - syx); // = trace(R H)
                float scale = (tnorm > 1e-6f) ? (trace / tnorm) : 1f;
                // Translation
                Vector2 offset = mr - scale * new Vector2(cos * mt.x - sin * mt.y, sin * mt.x + cos * mt.y);

                // RMS error
                float s2 = 0f; int n = refs.Count;
                for (int i = 0; i < n; i++)
                {
                    var tpt = trks[i];
                    var rot = new Vector2(cos * tpt.x - sin * tpt.y, sin * tpt.x + cos * tpt.y);
                    var p = scale * rot + offset;
                    float d = Vector2.Distance(refs[i], p);
                    s2 += d * d;
                }
                float rms = Mathf.Sqrt(s2 / n);
                if (rms < best.RmsError)
                {
                    best.TimeOffset = shift; best.Offset = offset; best.Scale = scale; best.AngleRad = angle; best.RmsError = rms;
                }
            }

            // Build aligned copy
            float c = Mathf.Cos(best.AngleRad), s = Mathf.Sin(best.AngleRad);
            var aligned = new List<TimedTrajectoryPoint>(tracked.Count);
            foreach (var tp in tracked)
            {
                var rot = new Vector2(c * tp.Position.x - s * tp.Position.y, s * tp.Position.x + c * tp.Position.y);
                var p = best.Scale * rot + best.Offset;
                aligned.Add(new TimedTrajectoryPoint { Position = p, Time = tp.Time + best.TimeOffset, IsCushion = tp.IsCushion });
            }
            best.AlignedTracked = aligned;
            return best;
        }

        private static bool TrySample(List<TimedTrajectoryPoint> path, float t, out Vector2 pos)
        {
            pos = Vector2.zero; if (path == null || path.Count == 0) return false;
            int j = 1; while (j < path.Count && path[j].Time < t) j++;
            int j0 = Mathf.Clamp(j - 1, 0, path.Count - 1); int j1 = Mathf.Clamp(j, 0, path.Count - 1);
            float t0 = path[j0].Time; float t1 = Mathf.Max(t0 + 1e-5f, path[j1].Time);
            float u = Mathf.Clamp01((t - t0) / (t1 - t0));
            pos = Vector2.Lerp(path[j0].Position, path[j1].Position, u);
            return true;
        }

        private static Vector2 Mean(List<Vector2> pts)
        {
            Vector2 m = Vector2.zero; if (pts.Count == 0) return m; foreach (var p in pts) m += p; return m / pts.Count;
        }
    }
}
