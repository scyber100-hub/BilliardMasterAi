using System.Collections.Generic;
using UnityEngine;
using BilliardMasterAi.Physics;
using BilliardMasterAi.Recommendation;

namespace BilliardMasterAi.Analytics
{
    public static class SensitivityMap
    {
        public struct Cell { public float angleDeg; public float speed; public float spin; public float score; }

        public static List<Cell> EvaluateGrid(Vector2 cue, Vector2 target, Vector2 other, float angleMin, float angleMax, int angleN, float speedMin, float speedMax, int speedN, float spinMin, float spinMax, int spinN)
        {
            var cells = new List<Cell>(angleN*speedN*spinN);
            for (int ia=0; ia<angleN; ia++)
            {
                float ang = Mathf.Lerp(angleMin, angleMax, ia/(float)Mathf.Max(1,angleN-1));
                for (int ispd=0; ispd<speedN; ispd++)
                {
                    float spd = Mathf.Lerp(speedMin, speedMax, ispd/(float)Mathf.Max(1,speedN-1));
                    for (int isp=0; isp<spinN; isp++)
                    {
                        float sp = Mathf.Lerp(spinMin, spinMax, isp/(float)Mathf.Max(1,spinN-1));
                        var path = PhysicsFacade.SimulateCue(cue, target, other, ang, spd, sp);
                        int cushions=0; foreach (var p in path) if (p.IsCushion) cushions++;
                        float prox = DistanceToPath(target, path);
                        float score = (cushions>=3?1f:0f) - Mathf.Clamp01(prox/1.0f);
                        cells.Add(new Cell{ angleDeg=ang, speed=spd, spin=sp, score=score });
                    }
                }
            }
            return cells;
        }

        private static float DistanceToPath(Vector2 p, List<TrajectoryPoint> path)
        {
            float d = float.PositiveInfinity; for (int i=1;i<path.Count;i++){ var a=path[i-1].Position; var b=path[i].Position; d = Mathf.Min(d, DistancePointToSegment(p,a,b)); } return d;
        }
        private static float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab=b-a; var ap=p-a; float t = Mathf.Clamp01(Vector2.Dot(ap,ab)/Mathf.Max(1e-6f,ab.sqrMagnitude)); var q = a + t*ab; return Vector2.Distance(p,q);
        }
    }
}

