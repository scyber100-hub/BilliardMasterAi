using System.Collections.Generic;
using UnityEngine;
using BilliardMasterAi.Physics;
using BilliardMasterAi.Analysis;
using BilliardMasterAi.UI;

namespace BilliardMasterAi.Replay
{
    public class ShotReplayController : MonoBehaviour
    {
        [Header("References")]
        public Transform tableRoot;
        public TimedPathRenderer idealRenderer;
        public TimedPathRenderer actualRenderer;
        public Transform idealMarker;
        public Transform actualMarker;

        [Header("Playback")]
        public float playbackSpeed = 1f;

        private List<TimedTrajectoryPoint> _ideal = new();
        private List<TimedTrajectoryPoint> _actual = new();
        private float _t; private bool _playing;

        public void SetIdealFromPlan(BallState start, float maxTime = 8f, float dt = 0.01f)
        {
            _ideal = TrajectorySimulator.SimulateTimed(start, maxTime, dt, 12);
            idealRenderer?.Draw(_ideal);
        }

        public void SetActual(List<TimedTrajectoryPoint> recorded)
        {
            _actual = recorded ?? new List<TimedTrajectoryPoint>();
            actualRenderer?.Draw(_actual);
        }

        public void Play()
        {
            _t = 0f; _playing = true;
        }

        public void Pause() => _playing = false;

        public void Seek(float timeSeconds)
        {
            _t = Mathf.Max(0f, timeSeconds);
            _playing = false;
            UpdateMarkers(_ideal, idealMarker, _t);
            UpdateMarkers(_actual, actualMarker, _t);
        }

        public float Duration
        {
            get
            {
                float ti = (_ideal != null && _ideal.Count > 0) ? _ideal[_ideal.Count - 1].Time : 0f;
                float ta = (_actual != null && _actual.Count > 0) ? _actual[_actual.Count - 1].Time : 0f;
                return Mathf.Max(ti, ta);
            }
        }

        public List<TimedTrajectoryPoint> GetIdeal() => _ideal;
        public List<TimedTrajectoryPoint> GetActual() => _actual;

        public void Update()
        {
            if (!_playing) return;
            _t += Time.deltaTime * Mathf.Max(0.1f, playbackSpeed);
            UpdateMarkers(_ideal, idealMarker, _t);
            UpdateMarkers(_actual, actualMarker, _t);
        }

        private void UpdateMarkers(List<TimedTrajectoryPoint> path, Transform marker, float t)
        {
            if (marker == null || path == null || path.Count == 0) return;
            // find segment by time
            int j = 1; while (j < path.Count && path[j].Time < t) j++;
            int j0 = Mathf.Clamp(j - 1, 0, path.Count - 1);
            int j1 = Mathf.Clamp(j, 0, path.Count - 1);
            float t0 = path[j0].Time; float t1 = Mathf.Max(t0 + 1e-4f, path[j1].Time);
            float u = Mathf.Clamp01((t - t0) / (t1 - t0));
            Vector2 p = Vector2.Lerp(path[j0].Position, path[j1].Position, u);
            Vector3 world = tableRoot ? tableRoot.TransformPoint(new Vector3(p.x, 0f, p.y)) : new Vector3(p.x, 0f, p.y);
            marker.position = world + Vector3.up * 0.01f;
        }

        public ErrorReport ComputeError()
        {
            var ideal2D = new List<Vector2>(_ideal.Count); foreach (var tp in _ideal) ideal2D.Add(tp.Position);
            var actual2D = new List<Vector2>(_actual.Count); foreach (var tp in _actual) actual2D.Add(tp.Position);
            return TrajectoryComparer.Compare(ideal2D, actual2D);
        }
    }
}
