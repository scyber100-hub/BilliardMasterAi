using System.Collections.Generic;
using UnityEngine;
using BilliardMasterAi.Physics;

namespace BilliardMasterAi.Rules
{
    public struct RuleVerdict
    {
        public bool Success;
        public string Reason;
        public int Cushions;
        public string FirstObject;
    }

    public static class RuleEngine
    {
        private static bool TryContactAt(List<TimedTrajectoryPoint> cue, List<TimedTrajectoryPoint> obj, float tol, out float tContact, out Vector2 pContact)
        {
            tContact = -1f; pContact = Vector2.zero; if (cue == null || obj == null || cue.Count==0 || obj.Count==0) return false;
            for (int i=0;i<cue.Count;i++)
            {
                var c = cue[i];
                // sample object at cue time
                Vector2 o; if (!SampleAt(obj, c.Time, out o)) continue;
                if (Vector2.Distance(c.Position, o) <= tol) { tContact = c.Time; pContact = c.Position; return true; }
            }
            return false;
        }

        private static bool SampleAt(List<TimedTrajectoryPoint> path, float t, out Vector2 pos)
        {
            pos = Vector2.zero; if (path==null || path.Count==0) return false;
            int j=1; while (j<path.Count && path[j].Time < t) j++;
            int j0 = Mathf.Clamp(j-1,0,path.Count-1), j1 = Mathf.Clamp(j,0,path.Count-1);
            float t0 = path[j0].Time, t1 = Mathf.Max(t0+1e-6f, path[j1].Time); float u = Mathf.Clamp01((t - t0)/(t1 - t0));
            pos = Vector2.Lerp(path[j0].Position, path[j1].Position, u); return true;
        }

        public static RuleVerdict Evaluate(RuleConfig cfg, List<TimedTrajectoryPoint> cue, List<TimedTrajectoryPoint> obj1, List<TimedTrajectoryPoint> obj2)
        {
            if (cfg == null) cfg = ScriptableObject.CreateInstance<RuleConfig>();
            var d = CaromScorer.EvaluateThreeCushion(cue, obj1, obj2);
            bool ok = d.Success && d.CushionBeforeSecond >= Mathf.Max(1, cfg.requiredCushions);
            return new RuleVerdict { Success = ok, Cushions = d.CushionBeforeSecond, FirstObject = d.FirstObject, Reason = ok ? ">= required cushions before second contact" : d.Reason };
        }

        // Detailed evaluation with simple foul/kiss detection and event logging.
        public static (RuleVerdict verdict, BilliardMasterAi.Replay.ShotEventLogger log) EvaluateDetailed(RuleConfig cfg, List<TimedTrajectoryPoint> cue, List<TimedTrajectoryPoint> obj1, List<TimedTrajectoryPoint> obj2)
        {
            var log = new BilliardMasterAi.Replay.ShotEventLogger();
            if (cue != null && cue.Count > 0) log.Log(BilliardMasterAi.Replay.ShotEventType.Start, cue[0].Time, cue[0].Position, "start");

            // event-based contacts
            float tol = cfg != null ? cfg.contactTolerance : CaromConstants.BallRadius*2.05f;
            float t1=-1f, t2=-1f; Vector2 p1=Vector2.zero, p2=Vector2.zero; string firstObj="none";
            bool hit1 = TryContactAt(cue, obj1, tol, out t1, out p1);
            bool hit2 = TryContactAt(cue, obj2, tol, out t2, out p2);
            if (hit1 && (!hit2 || t1 <= t2)) firstObj = "obj1"; else if (hit2) firstObj="obj2";
            if (hit1) log.Log(BilliardMasterAi.Replay.ShotEventType.BallContact, t1, p1, "obj1");
            if (hit2) log.Log(BilliardMasterAi.Replay.ShotEventType.BallContact, t2, p2, "obj2");

            // log cushion events
            if (cue != null)
            {
                foreach (var p in cue) if (p.IsCushion) log.Log(BilliardMasterAi.Replay.ShotEventType.Cushion, p.Time, p.Position, "cushion");
            }
            // cushions between contacts
            int cushionsBetween = 0; if (cue != null && hit1 && hit2){ float tmin = Mathf.Min(t1,t2), tmax = Mathf.Max(t1,t2); foreach (var p in cue) if (p.IsCushion && p.Time > tmin && p.Time < tmax) cushionsBetween++; }
            // kiss/foul
            bool kiss = false; string reason = "";
            if (hit1 && hit2)
            {
                float dt = Mathf.Abs(t2 - t1);
                float thresh = cfg? cfg.kissDtThreshold : 0.1f;
                if (dt < thresh && cushionsBetween < Mathf.Max(1, cfg?.requiredCushions ?? 3)) { kiss = true; reason = "kiss/early second contact"; }
                // check minimal impact speed/angle at first contact (finite difference)
                float tFirst = Mathf.Min(t1,t2);
                Vector2 vPrev, vNext; if (EstimateVelocity(cue, tFirst, 0.01f, out vPrev, out vNext))
                {
                    float speed = vNext.magnitude;
                    float cosInc = Mathf.Abs(Vector2.Dot(vNext.normalized, (p1 - p2).normalized)); // approximate incidence
                    if (cfg != null && speed < cfg.minImpactSpeed) { kiss = true; reason = "foul: low impact speed"; }
                    if (cfg != null && cosInc < cfg.minIncidenceAngleCos) { kiss = true; reason = "foul: shallow incidence"; }
                }
            }
            // verdict
            bool ok2 = !kiss && hit1 && hit2 && cushionsBetween >= Mathf.Max(1, (cfg?.requiredCushions ?? 3));
            var verdict = new RuleVerdict { Success = ok2, Cushions = cushionsBetween, FirstObject = firstObj, Reason = ok2 ? ">= required cushions before second contact" : (reason==""?"insufficient cushions":reason) };
            if (cue != null && cue.Count > 0) log.Log(BilliardMasterAi.Replay.ShotEventType.Stop, cue[cue.Count-1].Time, cue[cue.Count-1].Position, "stop");
            return (verdict, log);
        }

        private static bool EstimateVelocity(List<TimedTrajectoryPoint> path, float t, float h, out Vector2 vPrev, out Vector2 vNext)
        {
            vPrev = Vector2.zero; vNext = Vector2.zero; if (path==null||path.Count==0) return false;
            Vector2 pA, pB; if (!SampleAt(path, t-h, out pA)) pA = path[0].Position; if (!SampleAt(path, t+h, out pB)) pB = path[path.Count-1].Position; float dt = 2*h; if (dt<=1e-6f) return false; vNext = (pB - pA)/dt; vPrev = vNext; return true;
        }
    }
}
