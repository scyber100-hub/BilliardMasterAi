using System;
using System.Collections.Generic;
using UnityEngine;
using BilliardMasterAi.Physics;

namespace BilliardMasterAi.Recommendation
{
    public struct ShotParams
    {
        public float AngleDeg;   // 0..360
        public float Speed;      // m/s initial
        public float SpinZ;      // rad/s (+top / -back)
        public Vector2 TipOffset; // impact offset on cue-ball (meters), for future use
    }

    public struct ShotPlanResult
    {
        public ShotParams Parameters;
        public List<TrajectoryPoint> Path;
        public int CushionCount;
        public float Score; // higher is better
    }

    public static class ShotPlanner
    {
        // Simple search: sweep angles/speeds/spins, simulate, score cushion>=3 and proximity to target ball.
        public static ShotPlanResult PlanShot(Vector2 cuePos, Vector2 targetPos, Vector2 otherPos)
        {
            var best = new ShotPlanResult { Score = float.NegativeInfinity };

            float[] angleSteps = Linspace(0f, 360f, 72);   // 5-degree steps
            float[] speedSteps = { 1.5f, 2.0f, 2.5f, 3.0f, 3.5f }; // initial speeds
            float[] spinSteps = { -20f, -10f, 0f, 10f, 20f }; // rad/s

            foreach (var ang in angleSteps)
            foreach (var spd in speedSteps)
            foreach (var spn in spinSteps)
            {
                var path = Physics.PhysicsFacade.SimulateCue(cuePos, targetPos, otherPos, ang, spd, spn);
                int cushions = 0;
                for (int i = 0; i < path.Count; i++) if (path[i].IsCushion) cushions++;

                float prox = DistanceToPath(targetPos, path);
                float proxOther = DistanceToPath(otherPos, path);

                // score: prefer >=3 cushions, penalize close to other ball, reward proximity to target after cushion count satisfied
                float score = (cushions >= 3 ? 100f : 0f) + Mathf.Max(0f, 2.0f - prox) * 10f - Mathf.Max(0f, 1.0f - proxOther) * 5f - cushions * 0.5f;

                if (score > best.Score)
                {
                    best = new ShotPlanResult
                    {
                        Parameters = new ShotParams { AngleDeg = ang, Speed = spd, SpinZ = spn, TipOffset = Vector2.zero },
                        Path = path,
                        CushionCount = cushions,
                        Score = score
                    };
                }
            }

            return best;
        }

        // Return top-K candidates by score
        public static List<ShotPlanResult> PlanTopShots(Vector2 cuePos, Vector2 targetPos, Vector2 otherPos, int k = 2)
        {
            var top = new List<ShotPlanResult>(k);

            float[] angleSteps = Linspace(0f, 360f, 72);
            float[] speedSteps = { 1.5f, 2.0f, 2.5f, 3.0f, 3.5f };
            float[] spinSteps = { -20f, -10f, 0f, 10f, 20f };

            foreach (var ang in angleSteps)
            foreach (var spd in speedSteps)
            foreach (var spn in spinSteps)
            {
                var path = Physics.PhysicsFacade.SimulateCue(cuePos, targetPos, otherPos, ang, spd, spn);
                int cushions = 0; for (int i = 0; i < path.Count; i++) if (path[i].IsCushion) cushions++;
                float prox = DistanceToPath(targetPos, path);
                float proxOther = DistanceToPath(otherPos, path);
                float score = (cushions >= 3 ? 100f : 0f) + Mathf.Max(0f, 2.0f - prox) * 10f - Mathf.Max(0f, 1.0f - proxOther) * 5f - cushions * 0.5f;

                var cand = new ShotPlanResult
                {
                    Parameters = new ShotParams { AngleDeg = ang, Speed = spd, SpinZ = spn, TipOffset = Vector2.zero },
                    Path = path,
                    CushionCount = cushions,
                    Score = score
                };

                InsertTopK(top, cand, k);
            }

            return top;
        }

        private static void InsertTopK(List<ShotPlanResult> list, ShotPlanResult item, int k)
        {
            int idx = list.FindIndex(x => item.Score > x.Score);
            if (idx < 0)
            {
                if (list.Count < k) list.Add(item);
            }
            else
            {
                list.Insert(idx, item);
                if (list.Count > k) list.RemoveAt(list.Count - 1);
            }
        }

        private static float[] Linspace(float start, float end, int count)
        {
            var arr = new float[count];
            float step = (end - start) / count;
            for (int i = 0; i < count; i++) arr[i] = start + step * i;
            return arr;
        }

        private static Vector2 AngleToDir(float deg)
        {
            float r = deg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(r), Mathf.Sin(r)).normalized;
        }

        // Min distance from a point to the simulated polyline
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
