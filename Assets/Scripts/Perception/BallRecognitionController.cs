using System.Collections.Generic;
using UnityEngine;
using BilliardMasterAi.Perception;
using Unity.Collections;

#if UNITY_XR_ARFOUNDATION
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#endif

namespace BilliardMasterAi.Perception
{
    public struct RecognizedBall
    {
        public BallColor color;
        public Vector2 screenPos;   // pixels
        public Vector2 tableLocal;  // meters (X,Z on table local)
        public float confidence;
    }

    public class BallRecognitionController : MonoBehaviour
    {
        public Transform tableRoot;
        public Camera arCamera;
        public BallDetectionConfig config = new BallDetectionConfig();
        public bool showDebugLogs = true;
        [Header("Auto Detect")]
        public bool autoDetect = true;
        public float detectInterval = 0.2f; // seconds (5 Hz)
        public float minConfidence = 0.15f;
        public bool placeTransforms = true;
        public bool autoCalibrateGreen = false;
        public Transform whiteBall;
        public Transform yellowBall;
        public Transform redBall;
        public bool yellowIsCueBall = true; // otherwise white is cue

#if UNITY_XR_ARFOUNDATION
        [SerializeField] private ARCameraManager _cameraManager;
#else
        private WebCamTexture _webCam;
#endif

        public List<RecognizedBall> lastResults = new();
        private float _timer;

        void Start()
        {
            if (arCamera == null) arCamera = Camera.main;
#if !UNITY_XR_ARFOUNDATION
            // Fallback camera for editor
            if (_webCam == null && WebCamTexture.devices.Length > 0)
            {
                _webCam = new WebCamTexture(640, 360, 15);
                _webCam.Play();
            }
#endif
        }

        void Update()
        {
            if (!autoDetect) return;
            _timer += Time.deltaTime;
            if (_timer >= detectInterval)
            {
                _timer = 0f;
                CaptureAndDetect();
                if (placeTransforms) ApplyToTransforms();
            }
        }

        public void CaptureAndDetect()
        {
#if UNITY_XR_ARFOUNDATION
            if (_cameraManager == null) _cameraManager = FindObjectOfType<ARCameraManager>();
            if (_cameraManager == null || !_cameraManager.TryAcquireLatestCpuImage(out XRCpuImage cpuImage))
            {
                if (showDebugLogs) Debug.LogWarning("No ARCameraManager or CPU image unavailable.");
                return;
            }

            var tex = ConvertToTexture2D(cpuImage);
            cpuImage.Dispose();
            var pixels = tex.GetPixels32();
            ProcessPixels(pixels, tex.width, tex.height);
#else
            if (_webCam == null || !_webCam.isPlaying)
            {
                if (showDebugLogs) Debug.LogWarning("WebCam not available.");
                return;
            }
            var pixels = _webCam.GetPixels32();
            ProcessPixels(pixels, _webCam.width, _webCam.height);
#endif
        }

        private void ProcessPixels(Color32[] pixels, int width, int height)
        {
            if (autoCalibrateGreen)
            {
                config = ColorBallDetector.AutoCalibrateGreenFelt(pixels, width, height, config);
            }
            var dets = ColorBallDetector.DetectFromBuffer(pixels, width, height, config);
            lastResults.Clear();
            foreach (var d in dets)
            {
                Vector2 sp = new Vector2(d.normalizedPos.x * Screen.width, d.normalizedPos.y * Screen.height);
                if (TableCoordinateMapper.ScreenToTableLocal2D(sp, arCamera, tableRoot, out var local))
                {
                    if (d.confidence >= minConfidence)
                    {
                        lastResults.Add(new RecognizedBall
                        {
                            color = d.color,
                            screenPos = sp,
                            tableLocal = local,
                            confidence = d.confidence
                        });
                    }
                }
            }

            if (showDebugLogs)
            {
                foreach (var r in lastResults)
                    Debug.Log($"Detected {r.color} at table {r.tableLocal}, conf={r.confidence:0.00}");
            }
        }

        public void ApplyTransformsNow()
        {
            ApplyToTransforms();
        }

        private void ApplyToTransforms()
        {
            foreach (var r in lastResults)
            {
                Transform t = null;
                switch (r.color)
                {
                    case BallColor.White: t = whiteBall; break;
                    case BallColor.Yellow: t = yellowBall; break;
                    case BallColor.Red: t = redBall; break;
                }
                if (t == null) continue;
                var world = tableRoot ? tableRoot.TransformPoint(new Vector3(r.tableLocal.x, 0f, r.tableLocal.y)) : new Vector3(r.tableLocal.x, 0f, r.tableLocal.y);
                t.position = world + Vector3.up * BilliardMasterAi.Physics.CaromConstants.BallRadius;
            }
        }

#if UNITY_XR_ARFOUNDATION
        private Texture2D ConvertToTexture2D(XRCpuImage image)
        {
            var format = TextureFormat.RGBA32;
            var tex = new Texture2D(image.width, image.height, format, false);
            var conversionParams = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, image.width, image.height),
                outputDimensions = new Vector2Int(image.width, image.height),
                outputFormat = TextureFormat.RGBA32,
                transformation = XRCpuImage.Transformation.MirrorY
            };

            int size = image.GetConvertedDataSize(conversionParams);
            var buffer = new NativeArray<byte>(size, Unity.Collections.Allocator.Temp);
            image.Convert(conversionParams, buffer);
            tex.LoadRawTextureData(buffer);
            tex.Apply(false);
            buffer.Dispose();
            return tex;
        }
#endif
    }
}
