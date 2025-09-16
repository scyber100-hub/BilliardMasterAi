using System.Collections.Generic;
using UnityEngine;
using BilliardMasterAi.Physics;

namespace BilliardMasterAi.Analytics
{
    // Placeholder for a data-driven success probability model. Collects samples and computes simple logistic fit later.
    public static class SuccessModel
    {
        public struct Sample
        {
            public float cushions; public float length; public float spin; public float proxTarget; public float proxOther; public int label; // 1/0
        }
        private static readonly List<Sample> _samples = new();

        public static void AddSample(List<TimedTrajectoryPoint> cue, Vector2 target, Vector2 other, bool success)
        {
            float len = 0f; for (int i=1;i<cue.Count;i++) len += Vector2.Distance(cue[i-1].Position, cue[i].Position);
            int cushions = 0; foreach (var p in cue) if (p.IsCushion) cushions++;
            float proxT = MinDistance(cue, target);
            float proxO = MinDistance(cue, other);
            _samples.Add(new Sample{ cushions=cushions, length=len, spin=0f, proxTarget=proxT, proxOther=proxO, label = success ? 1 : 0});
        }

        private static float MinDistance(List<TimedTrajectoryPoint> path, Vector2 p)
        {
            float d = float.PositiveInfinity; for (int i=1;i<path.Count;i++){ var a=path[i-1].Position; var b=path[i].Position; d = Mathf.Min(d, DistancePointToSegment(p,a,b)); } return d;
        }
        private static float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b-a; var ap = p-a; float t = Mathf.Clamp01(Vector2.Dot(ap,ab)/Mathf.Max(1e-6f,ab.sqrMagnitude)); var q = a + t*ab; return Vector2.Distance(p,q);
        }

        public static IReadOnlyList<Sample> GetSamples() => _samples;

        public static (float auc, float[] fpr, float[] tpr) RocCurve(int bins=50)
        {
            if (_samples.Count == 0) return (0f, new float[0], new float[0]);
            var scores = new System.Collections.Generic.List<(float s, int y)>();
            foreach (var s in _samples)
            {
                float p = Mathf.Clamp01((s.cushions/3f) - Mathf.Clamp01(s.proxTarget/1.0f));
                scores.Add((p, s.label));
            }
            scores.Sort((a,b)=> b.s.CompareTo(a.s));
            int P=0,N=0; foreach (var r in scores){ if (r.y==1) P++; else N++; }
            var fpr=new float[bins+1]; var tpr=new float[bins+1];
            for (int i=0;i<=bins;i++)
            {
                float thr = i/(float)bins;
                int tp=0, fp=0; foreach (var r in scores){ bool pred = r.s>=thr; if (pred && r.y==1) tp++; else if (pred && r.y==0) fp++; }
                tpr[i] = P>0? tp/(float)P : 0f; fpr[i] = N>0? fp/(float)N : 0f;
            }
            float auc=0f; for (int i=1;i<=bins;i++){ float dx = Mathf.Abs(fpr[i]-fpr[i-1]); float yavg = 0.5f*(tpr[i]+tpr[i-1]); auc += dx*yavg; }
            return (auc, fpr, tpr);
        }

        public static (float mean, float std) FeatureStats(System.Func<Sample,float> selector)
        {
            if (_samples.Count==0) return (0f,0f);
            float sum=0f; foreach (var s in _samples) sum+=selector(s); float mean=sum/_samples.Count; float var=0f; foreach (var s in _samples){ float d=selector(s)-mean; var+=d*d; } var/=Mathf.Max(1,_samples.Count-1); return (mean, Mathf.Sqrt(var));
        }

        public static Analytics.LogisticModel TrainLogistic(int epochs=300, float lr=0.1f)
        {
            if (_samples.Count == 0) return new LogisticModel(5);
            var X = new List<float[]>(); var y = new List<int>();
            foreach (var s in _samples)
            {
                X.Add(new[]{ s.cushions, s.length, s.spin, s.proxTarget, s.proxOther }); y.Add(s.label);
            }
            var model = new LogisticModel(5);
            model.Fit(X, y, epochs, lr);
            return model;
        }
    }
}
