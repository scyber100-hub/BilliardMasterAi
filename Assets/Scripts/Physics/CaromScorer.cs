using System.Collections.Generic;
using UnityEngine;

namespace BilliardMasterAi.Physics
{
    public struct ScoreDetails
    {
        public bool Success;
        public int CushionBeforeSecond;
        public string FirstObject; // "obj1" or "obj2" or "none"
        public float FirstHitTime;
        public float SecondHitTime;
        public string Reason; // explanation
    }

    public static class CaromScorer
    {
        // Evaluate 3-cushion: cue hits one object ball first, accumulates >=3 cushions before contacting second object ball.
        // Inputs: cue path (timed), object ball positions (assumed static for evaluation simplicity).
        public static ScoreDetails EvaluateThreeCushion(List<TimedTrajectoryPoint> cuePath, Vector2 obj1, Vector2 obj2)
        {
            var result = new ScoreDetails { Success = false, CushionBeforeSecond = 0, FirstObject = "none", FirstHitTime = -1f, SecondHitTime = -1f, Reason = string.Empty };
            if (cuePath == null || cuePath.Count == 0) { result.Reason = "empty path"; return result; }
            float contactR = CaromConstants.BallRadius * 2.05f; // tolerance
            bool hit1 = false, hit2 = false; int cushions = 0; bool afterFirst = false;
            float tFirst = -1f, tSecond = -1f;
            for (int i = 0; i < cuePath.Count; i++)
            {
                var p = cuePath[i];
                if (afterFirst && p.IsCushion) cushions++;
                if (!hit1 && Vector2.Distance(p.Position, obj1) <= contactR)
                {
                    hit1 = true; afterFirst = true; tFirst = p.Time; if (result.FirstObject == "none") result.FirstObject = "obj1";
                }
                if (!hit2 && Vector2.Distance(p.Position, obj2) <= contactR)
                {
                    if (!afterFirst)
                    {
                        // hit obj2 first
                        afterFirst = true; result.FirstObject = "obj2"; tFirst = p.Time;
                    }
                    else
                    {
                        tSecond = p.Time; break;
                    }
                    hit2 = true;
                }
            }

            result.CushionBeforeSecond = cushions;
            result.FirstHitTime = tFirst; result.SecondHitTime = tSecond;
            if (tSecond >= 0f && cushions >= 3)
            {
                result.Success = true; result.Reason = ">=3 cushions before second object contact";
            }
            else
            {
                if (tSecond < 0f) result.Reason = "second object not contacted";
                else result.Reason = $"only {cushions} cushions before second contact";
            }
            return result;
        }

        // Overload: accept moving object ball trajectories and use time-aligned proximity for contact detection
        public static ScoreDetails EvaluateThreeCushion(List<TimedTrajectoryPoint> cuePath, List<TimedTrajectoryPoint> obj1Path, List<TimedTrajectoryPoint> obj2Path)
        {
            var result = new ScoreDetails { Success = false, CushionBeforeSecond = 0, FirstObject = "none", FirstHitTime = -1f, SecondHitTime = -1f, Reason = string.Empty };
            if (cuePath == null || cuePath.Count == 0) { result.Reason = "empty path"; return result; }
            float contactR = CaromConstants.BallRadius * 2.05f;
            bool hitObj1 = false, hitObj2 = false; int cushions = 0; bool afterFirst = false;
            float tFirst = -1f, tSecond = -1f;
            for (int i = 0; i < cuePath.Count; i++)
            {
                var p = cuePath[i];
                if (afterFirst && p.IsCushion) cushions++;
                var p1 = SampleAt(obj1Path, p.Time);
                var p2 = SampleAt(obj2Path, p.Time);
                if (!hitObj1 && (p1.HasValue && Vector2.Distance(p.Position, p1.Value) <= contactR))
                { hitObj1 = true; if (!afterFirst) { afterFirst = true; result.FirstObject = "obj1"; tFirst = p.Time; } }
                if (!hitObj2 && (p2.HasValue && Vector2.Distance(p.Position, p2.Value) <= contactR))
                { hitObj2 = true; if (!afterFirst) { afterFirst = true; result.FirstObject = "obj2"; tFirst = p.Time; } else { tSecond = p.Time; break; } }
            }
            result.CushionBeforeSecond = cushions; result.FirstHitTime = tFirst; result.SecondHitTime = tSecond;
            if (tSecond >= 0f && cushions >= 3) { result.Success = true; result.Reason = ">=3 cushions before second object contact"; }
            else { result.Reason = (tSecond < 0f) ? "second object not contacted" : $"only {cushions} cushions before second contact"; }
            return result;
        }

        private static Vector2? SampleAt(List<TimedTrajectoryPoint> path, float t)
        {
            if (path == null || path.Count == 0) return null;
            int j = 1; while (j < path.Count && path[j].Time < t) j++;
            int j0 = Mathf.Clamp(j - 1, 0, path.Count - 1);
            int j1 = Mathf.Clamp(j, 0, path.Count - 1);
            float t0 = path[j0].Time; float t1 = Mathf.Max(t0 + 1e-5f, path[j1].Time);
            float u = Mathf.Clamp01((t - t0) / (t1 - t0));
            return Vector2.Lerp(path[j0].Position, path[j1].Position, u);
        }
    }
}
