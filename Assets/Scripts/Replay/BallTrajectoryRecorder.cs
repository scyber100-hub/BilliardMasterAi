using System.Collections.Generic;
using UnityEngine;
using BilliardMasterAi.Physics;

namespace BilliardMasterAi.Replay
{
    public class BallTrajectoryRecorder : MonoBehaviour
    {
        public Transform tableRoot;
        public Transform cueBall; // track this transform
        public float sampleRate = 60f;
        public float maxDuration = 10f;

        private bool _recording;
        private float _t;
        private float _accum;
        private readonly List<TimedTrajectoryPoint> _samples = new();

        public void StartRecording()
        {
            _samples.Clear();
            _t = 0f; _accum = 0f; _recording = true;
            SampleOnce();
        }

        public List<TimedTrajectoryPoint> StopRecording()
        {
            _recording = false;
            return new List<TimedTrajectoryPoint>(_samples);
        }

        void Update()
        {
            if (!_recording) return;
            float dt = Time.deltaTime;
            _t += dt; _accum += dt;
            float step = 1f / Mathf.Max(1f, sampleRate);
            while (_accum >= step)
            {
                _accum -= step;
                SampleOnce();
            }
            if (_t >= maxDuration) _recording = false;
        }

        private void SampleOnce()
        {
            if (cueBall == null) return;
            Vector3 world = cueBall.position;
            Vector3 local = tableRoot ? tableRoot.InverseTransformPoint(world) : world;
            var p = new TimedTrajectoryPoint
            {
                Position = new Vector2(local.x, local.z),
                IsCushion = NearCushion(local),
                Time = _t
            };
            _samples.Add(p);
        }

        private bool NearCushion(Vector3 local)
        {
            float halfW = CaromConstants.TableWidth * 0.5f - CaromConstants.BallRadius;
            float halfH = CaromConstants.TableHeight * 0.5f - CaromConstants.BallRadius;
            float eps = 0.02f; // 2cm proximity
            return Mathf.Abs(local.x) > halfW - eps || Mathf.Abs(local.z) > halfH - eps;
        }
    }
}

