using System;
using System.Collections.Generic;
using UnityEngine;
using BilliardMasterAi.Physics;

namespace BilliardMasterAi.Calibration
{
    // Simple random search fitter to minimize RMS error between actual and simulated cue path.
    public static class CalibrationFitter
    {
        public struct FitResult { public CalibrationProfile profile; public float rms; public int iterations; }

        public static FitResult Fit(Vector2 cue, Vector2 obj1, Vector2 obj2, float angleDeg, float speed, float spinY, List<TimedTrajectoryPoint> actual, int iterations = 200)
        {
            var best = new CalibrationProfile { muK = AdvancedParams.MuK, muR = AdvancedParams.MuR, muContact = AdvancedParams.MuContact, muCushion = AdvancedParams.MuCushion, restitutionBall = AdvancedParams.RestitutionBall, restitutionCushion = AdvancedParams.RestitutionCushionBase };
            best.Apply();
            float bestCost = Cost(cue, obj1, obj2, angleDeg, speed, spinY, actual);

            var rnd = new System.Random(1234);
            float T0 = 0.05f; // initial temperature (accept worse solutions)
            for (int it = 0; it < iterations; it++)
            {
                var cand = ScriptableObject.CreateInstance<CalibrationProfile>();
                // Random perturb around current AdvancedParams
                float cooling = Mathf.Lerp(1f, 0.1f, it / Mathf.Max(1f, (float)iterations));
                cand.muK = Clamp(AdvancedParams.MuK + Rand(rnd, -0.05f, 0.05f) * cooling, 0.05f, 0.35f);
                cand.muR = Clamp(AdvancedParams.MuR + Rand(rnd, -0.005f, 0.005f) * cooling, 0.002f, 0.03f);
                cand.muContact = Clamp(AdvancedParams.MuContact + Rand(rnd, -0.05f, 0.05f) * cooling, 0.05f, 0.4f);
                cand.muCushion = Clamp(AdvancedParams.MuCushion + Rand(rnd, -0.05f, 0.05f) * cooling, 0.05f, 0.5f);
                cand.restitutionBall = Clamp(AdvancedParams.RestitutionBall + Rand(rnd, -0.05f, 0.05f) * cooling, 0.80f, 0.99f);
                cand.restitutionCushion = Clamp(AdvancedParams.RestitutionCushionBase + Rand(rnd, -0.05f, 0.05f) * cooling, 0.80f, 0.99f);
                cand.Apply();

                float c = Cost(cue, obj1, obj2, angleDeg, speed, spinY, actual);
                float d = c - bestCost;
                if (d < 0f || System.Math.Exp(-d / Mathf.Max(1e-6f, T0 * cooling)) > rnd.NextDouble())
                {
                    best = cand; bestCost = c;
                }
            }

            best.Apply();
            return new FitResult { profile = best, rms = bestCost, iterations = iterations };
        }

        private static float Cost(Vector2 cue, Vector2 obj1, Vector2 obj2, float angleDeg, float speed, float spinY, List<TimedTrajectoryPoint> actual)
        {
            var sim = PhysicsFacade.SimulateCue(cue, obj1, obj2, angleDeg, speed, spinY);
            // Compare to actual (ignore time; resample by count)
            var a = ToVec(actual);
            return Rms(Resample(a, 200), Resample(sim, 200));
        }

        private static List<Vector2> ToVec(List<TimedTrajectoryPoint> pts)
        {
            var list = new List<Vector2>(pts.Count); foreach (var p in pts) list.Add(p.Position); return list;
        }

        private static float Rms(List<Vector2> A, List<Vector2> B)
        {
            int N = Mathf.Min(A.Count, B.Count); if (N == 0) return 9999f;
            float s=0f; for (int i=0;i<N;i++){ float d = Vector2.Distance(A[i], B[i]); s += d*d; }
            return Mathf.Sqrt(s/N);
        }

        private static List<Vector2> Resample(List<Vector2> pts, int N)
        {
            if (pts == null || pts.Count == 0) return new List<Vector2>(new Vector2[N]);
            float total=0f; var acc = new float[pts.Count]; acc[0]=0f;
            for (int i=1;i<pts.Count;i++){ total += Vector2.Distance(pts[i-1], pts[i]); acc[i]=total; }
            var outPts = new List<Vector2>(N);
            for(int i=0;i<N;i++)
            {
                float t = total * i / Mathf.Max(1, N-1);
                int j=1; while(j<pts.Count && acc[j] < t) j++;
                int j0 = Mathf.Clamp(j-1,0,pts.Count-1); int j1 = Mathf.Clamp(j,0,pts.Count-1);
                float seg = Mathf.Max(1e-6f, acc[j1]-acc[j0]); float u = Mathf.Clamp01((t-acc[j0])/seg);
                outPts.Add(Vector2.Lerp(pts[j0], pts[j1], u));
            }
            return outPts;
        }

        private static float Rand(System.Random r, float a, float b) => a + (float)r.NextDouble() * (b - a);
        private static float Clamp(float v, float a, float b) => Mathf.Clamp(v, a, b);

        public static FitResult FitCmaEs(Vector2 cue, Vector2 obj1, Vector2 obj2, float angleDeg, float speed, float spinY, List<TimedTrajectoryPoint> actual, int iterations = 200)
        {
            float[] min = { 0.05f, 0.002f, 0.05f, 0.05f, 0.80f, 0.80f };
            float[] max = { 0.35f, 0.030f, 0.40f, 0.50f, 0.99f, 0.99f };
            float[] x0 = { AdvancedParams.MuK, AdvancedParams.MuR, AdvancedParams.MuContact, AdvancedParams.MuCushion, AdvancedParams.RestitutionBall, AdvancedParams.RestitutionCushionBase };
            var opt = new CmaEs.Options { iterations = iterations, lambda = 12, sigma0 = 0.2f };
            Func<float[], float> cost = (x)=>
            {
                AdvancedParams.MuK = x[0]; AdvancedParams.MuR = x[1]; AdvancedParams.MuContact = x[2]; AdvancedParams.MuCushion = x[3]; AdvancedParams.RestitutionBall = x[4]; AdvancedParams.RestitutionCushionBase = x[5];
                return Cost(cue, obj1, obj2, angleDeg, speed, spinY, actual);
            };
            var best = CmaEs.Optimize(cost, x0, min, max, opt);
            AdvancedParams.MuK = best[0]; AdvancedParams.MuR = best[1]; AdvancedParams.MuContact = best[2]; AdvancedParams.MuCushion = best[3]; AdvancedParams.RestitutionBall = best[4]; AdvancedParams.RestitutionCushionBase = best[5];
            var profile = ScriptableObject.CreateInstance<CalibrationProfile>();
            profile.muK = best[0]; profile.muR = best[1]; profile.muContact = best[2]; profile.muCushion = best[3]; profile.restitutionBall = best[4]; profile.restitutionCushion = best[5];
            float rms = Cost(cue, obj1, obj2, angleDeg, speed, spinY, actual);
            return new FitResult { profile = profile, rms = rms, iterations = iterations };
        }
    }
}
