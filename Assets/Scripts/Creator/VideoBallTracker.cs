using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using BilliardMasterAi.Perception;
using BilliardMasterAi.Physics;

namespace BilliardMasterAi.Creator
{
    public class VideoBallTracker : MonoBehaviour
    {
        public VideoPlayer player;
        public VideoTableMapper mapper;
        public BallDetectionConfig config = new BallDetectionConfig();
        public int sampleFps = 10; // tracking fps
        [Range(0f,1f)] public float smoothingAlpha = 0.4f; // EMA smoothing for trajectories
        public bool useKalman = true;
        public bool logDebug = true;

        [Header("Output")] 
        public Dictionary<BallColor, List<TimedTrajectoryPoint>> trajectories = new();

        private RenderTexture _rt;
        private Texture2D _readTex;
        private readonly System.Collections.Generic.Dictionary<BallColor, Kalman2D> _filters = new();
        public double lastStartSec { get; private set; }
        public double lastEndSec { get; private set; }

        void OnDisable()
        {
            if (_rt != null) { if (player) player.targetTexture = null; _rt.Release(); _rt = null; }
            if (_readTex != null) { Destroy(_readTex); _readTex = null; }
        }

        public void Clear()
        {
            trajectories.Clear();
        }

        public void StartTrack(double startSec, double endSec)
        {
            StopAllCoroutines();
            StartCoroutine(TrackRoutine(startSec, endSec));
        }

        private IEnumerator TrackRoutine(double startSec, double endSec)
        {
            if (player == null || mapper == null) yield break;
            PrepareRenderTarget();
            Clear();
            lastStartSec = startSec; lastEndSec = endSec;

            double duration = Mathf.Max(0.001f, (float)(endSec - startSec));
            double dt = 1.0 / Mathf.Max(1, sampleFps);
            for (double t = startSec; t <= endSec; t += dt)
            {
                player.time = t;
                player.StepForward(); // ensure frame update where supported
                yield return new WaitForEndOfFrame();

                var pixels = ReadPixels();
                if (pixels == null) continue;
                var dets = ColorBallDetector.DetectFromBuffer(pixels, _readTex.width, _readTex.height, config);
                foreach (var d in dets)
                {
                    var local = mapper.ImageToTable(d.normalizedPos);
                    // smoothing per color
                    if (!trajectories.TryGetValue(d.color, out var list)) { list = new List<TimedTrajectoryPoint>(); trajectories[d.color] = list; }
                    Vector2 pos = local;
                    if (list.Count > 0)
                    {
                        var prev = list[list.Count - 1].Position;
                        pos = Vector2.Lerp(prev, local, Mathf.Clamp01(smoothingAlpha));
                    }
                    if (useKalman)
                    {
                        if (!_filters.TryGetValue(d.color, out var kf)) { kf = new Kalman2D(); _filters[d.color] = kf; }
                        // simple RANSAC-like rejection using residual vs predicted
                        Vector2 predicted = kf.Update(pos, 0f); // get state without advancing (approx)
                        float residual = Vector2.Distance(predicted, pos);
                        float thresh = 0.06f; // 6cm
                        if (residual < thresh) pos = kf.Update(pos, (float)dt);
                        else pos = predicted; // ignore outlier measurement
                    }
                    var pt = new TimedTrajectoryPoint { Position = pos, IsCushion = NearCushion(pos), Time = (float)(t - startSec) };
                    list.Add(pt);
                }
            }

            if (logDebug)
            {
                foreach (var kv in trajectories)
                    Debug.Log($"Tracked {kv.Key}: {kv.Value.Count} points");
            }
        }

        private void PrepareRenderTarget()
        {
            if (player.targetTexture == null)
            {
                int w = 640, h = 360;
                _rt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32);
                player.targetTexture = _rt;
            }
            else _rt = player.targetTexture;

            if (_readTex == null || _readTex.width != _rt.width || _readTex.height != _rt.height)
                _readTex = new Texture2D(_rt.width, _rt.height, TextureFormat.RGBA32, false);
        }

        private Color32[] ReadPixels()
        {
            if (_rt == null) return null;
            var prev = RenderTexture.active;
            RenderTexture.active = _rt;
            _readTex.ReadPixels(new Rect(0, 0, _rt.width, _rt.height), 0, 0);
            _readTex.Apply(false);
            RenderTexture.active = prev;
            return _readTex.GetPixels32();
        }

        private bool NearCushion(Vector2 local)
        {
            float halfW = CaromConstants.TableWidth * 0.5f - CaromConstants.BallRadius;
            float halfH = CaromConstants.TableHeight * 0.5f - CaromConstants.BallRadius;
            float eps = 0.02f;
            return Mathf.Abs(local.x) > halfW - eps || Mathf.Abs(local.y) > halfH - eps;
        }
    }
}
