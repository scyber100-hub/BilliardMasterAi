using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using System.Collections;
using BilliardMasterAi.AR;
using BilliardMasterAi.UI;

namespace BilliardMasterAi.Perception
{
    [System.Serializable]
    public class BallDetectionConfig
    {
        public int downscaleWidth = 320;
        public int downscaleHeight = 180;
        public float minConfidence = 0.2f;
        public float maxConfidence = 1.0f;
        public bool useGPU = true;
        public int maxFramesPerSecond = 5;
    }
    
    [System.Serializable]
    public class BallDetectionResult
    {
        public BallColor color;
        public Vector2 screenPosition;
        public Vector3 tablePosition;
        public float confidence;
        public Rect boundingBox;
        
        public BallDetectionResult(BallColor ballColor, Vector2 screenPos, float conf)
        {
            color = ballColor;
            screenPosition = screenPos;
            confidence = conf;
            tablePosition = Vector3.zero;
        }
    }
    
    public class BallRecognitionController : MonoBehaviour
    {
        [Header("AR Components")]
        [SerializeField] private ARCameraManager arCamera;
        [SerializeField] private ARTableCalibrator tableCalibrator;
        
        [Header("Ball Transforms")]
        [SerializeField] private Transform whiteBall;
        [SerializeField] private Transform yellowBall;
        [SerializeField] private Transform redBall;
        
        [Header("Detection Settings")]
        [SerializeField] private BallDetectionConfig config = new BallDetectionConfig();
        [SerializeField] public bool autoDetect = false;
        [SerializeField] public bool placeTransforms = false;
        [SerializeField] public float detectInterval = 0.2f;
        [SerializeField] public bool yellowIsCueBall = true;
        
        [Header("Color Detection")]
        [SerializeField] private Color whiteColorTarget = Color.white;
        [SerializeField] private Color yellowColorTarget = Color.yellow;
        [SerializeField] private Color redColorTarget = Color.red;
        [SerializeField] private float colorTolerance = 0.3f;
        [SerializeField] private int minBlobSize = 50;
        
        private Dictionary<BallColor, BallDetectionResult> lastDetectionResults;
        private Texture2D frameTexture;
        private bool isDetecting = false;
        private float lastDetectTime = 0f;
        private RenderTexture tempRenderTexture;
        
        public bool IsDetecting => isDetecting;
        public Dictionary<BallColor, BallDetectionResult> LastResults => lastDetectionResults;
        
        public System.Action<Dictionary<BallColor, BallDetectionResult>> OnBallsDetected;
        
        void Start()
        {
            lastDetectionResults = new Dictionary<BallColor, BallDetectionResult>();
            InitializeComponents();
            
            if (autoDetect)
            {
                InvokeRepeating(nameof(AutoDetectBalls), detectInterval, detectInterval);
            }
        }
        
        void Update()
        {
            if (autoDetect && Time.time - lastDetectTime >= detectInterval && !isDetecting)
            {
                lastDetectTime = Time.time;
                DetectBalls();
            }
        }
        
        private void InitializeComponents()
        {
            if (arCamera == null)
                arCamera = FindObjectOfType<ARCameraManager>();
            
            if (tableCalibrator == null)
                tableCalibrator = FindObjectOfType<ARTableCalibrator>();
                
            // 프레임 텍스처 초기화
            frameTexture = new Texture2D(config.downscaleWidth, config.downscaleHeight, TextureFormat.RGB24, false);
            tempRenderTexture = new RenderTexture(config.downscaleWidth, config.downscaleHeight, 0);
        }
        
        public void DetectBalls()
        {
            if (isDetecting || arCamera == null) return;
            
            StartCoroutine(DetectBallsCoroutine());
        }
        
        private IEnumerator DetectBallsCoroutine()
        {
            isDetecting = true;
            
            try
            {
                // AR 카메라에서 프레임 캡처
                yield return StartCoroutine(CaptureFrame());
                
                // 색상 기반 공 검출
                Dictionary<BallColor, BallDetectionResult> results = ProcessFrame(frameTexture);
                
                // 결과 저장 및 Transform 업데이트
                lastDetectionResults = results;
                
                if (placeTransforms)
                {
                    UpdateBallTransforms(results);
                }
                
                OnBallsDetected?.Invoke(results);
                
                Debug.Log($"Ball detection completed. Found {results.Count} balls.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Ball detection error: {e.Message}");
            }
            finally
            {
                isDetecting = false;
            }
        }
        
        private IEnumerator CaptureFrame()
        {
            if (arCamera.subsystem == null || !arCamera.subsystem.running)
            {
                Debug.LogWarning("AR Camera subsystem not available");
                yield break;
            }
            
            // AR Foundation에서 프레임 텍스처 가져오기
            var cameraTexture = arCamera.subsystem.currentConfiguration?.framerate > 0 ? 
                                arCamera.subsystem.currentConfiguration?.cameraImage : null;
            
            // 대안: 메인 카메라에서 렌더텍스처 생성
            Camera mainCamera = Camera.main ?? arCamera.GetComponent<Camera>();
            
            if (mainCamera != null)
            {
                RenderTexture currentRT = RenderTexture.active;
                RenderTexture.active = tempRenderTexture;
                
                mainCamera.targetTexture = tempRenderTexture;
                mainCamera.Render();
                
                frameTexture.ReadPixels(new Rect(0, 0, config.downscaleWidth, config.downscaleHeight), 0, 0);
                frameTexture.Apply();
                
                mainCamera.targetTexture = null;
                RenderTexture.active = currentRT;
            }
            
            yield return null;
        }
        
        private Dictionary<BallColor, BallDetectionResult> ProcessFrame(Texture2D texture)
        {
            Dictionary<BallColor, BallDetectionResult> results = new Dictionary<BallColor, BallDetectionResult>();
            
            if (texture == null) return results;
            
            Color[] pixels = texture.GetPixels();
            int width = texture.width;
            int height = texture.height;
            
            // 각 색상별로 공 검출
            BallDetectionResult whiteResult = DetectBallByColor(pixels, width, height, whiteColorTarget, BallColor.White);
            if (whiteResult != null) results[BallColor.White] = whiteResult;
            
            BallDetectionResult yellowResult = DetectBallByColor(pixels, width, height, yellowColorTarget, BallColor.Yellow);
            if (yellowResult != null) results[BallColor.Yellow] = yellowResult;
            
            BallDetectionResult redResult = DetectBallByColor(pixels, width, height, redColorTarget, BallColor.Red);
            if (redResult != null) results[BallColor.Red] = redResult;
            
            // 테이블 좌표로 변환
            foreach (var result in results.Values)
            {
                if (tableCalibrator != null && tableCalibrator.IsCalibrated)
                {
                    Vector2 normalizedScreen = new Vector2(
                        result.screenPosition.x / width,
                        result.screenPosition.y / height
                    );
                    
                    Vector2 screenPos = new Vector2(
                        normalizedScreen.x * Screen.width,
                        normalizedScreen.y * Screen.height
                    );
                    
                    result.tablePosition = tableCalibrator.ScreenToTablePosition(screenPos);
                }
            }
            
            return results;
        }
        
        private BallDetectionResult DetectBallByColor(Color[] pixels, int width, int height, Color targetColor, BallColor ballColor)
        {
            List<Vector2> matchingPixels = new List<Vector2>();
            
            // 색상 매칭 픽셀 찾기
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    Color pixel = pixels[index];
                    
                    if (IsColorMatch(pixel, targetColor, colorTolerance))
                    {
                        matchingPixels.Add(new Vector2(x, y));
                    }
                }
            }
            
            if (matchingPixels.Count < minBlobSize)
                return null;
            
            // 중심점 계산
            Vector2 center = Vector2.zero;
            foreach (Vector2 pixel in matchingPixels)
            {
                center += pixel;
            }
            center /= matchingPixels.Count;
            
            // 신뢰도 계산 (클러스터 밀도 기반)
            float confidence = CalculateConfidence(matchingPixels, center);
            
            if (confidence < config.minConfidence)
                return null;
            
            return new BallDetectionResult(ballColor, center, confidence);
        }
        
        private bool IsColorMatch(Color pixel, Color target, float tolerance)
        {
            // HSV 색공간에서 비교 (더 정확한 색상 매칭)
            Color.RGBToHSV(pixel, out float pixelH, out float pixelS, out float pixelV);
            Color.RGBToHSV(target, out float targetH, out float targetS, out float targetV);
            
            float hDiff = Mathf.Min(Mathf.Abs(pixelH - targetH), 1f - Mathf.Abs(pixelH - targetH));
            float sDiff = Mathf.Abs(pixelS - targetS);
            float vDiff = Mathf.Abs(pixelV - targetV);
            
            return hDiff < tolerance && sDiff < tolerance && vDiff < tolerance;
        }
        
        private float CalculateConfidence(List<Vector2> pixels, Vector2 center)
        {
            if (pixels.Count == 0) return 0f;
            
            // 중심점으로부터의 평균 거리 계산
            float totalDistance = 0f;
            foreach (Vector2 pixel in pixels)
            {
                totalDistance += Vector2.Distance(pixel, center);
            }
            
            float avgDistance = totalDistance / pixels.Count;
            
            // 거리가 짧을수록 (응집도가 높을수록) 신뢰도 증가
            float confidence = Mathf.Clamp01(1f - avgDistance / 50f);
            
            // 픽셀 개수도 신뢰도에 반영
            float sizeBonus = Mathf.Clamp01(pixels.Count / 200f);
            
            return Mathf.Clamp01(confidence * 0.7f + sizeBonus * 0.3f);
        }
        
        private void UpdateBallTransforms(Dictionary<BallColor, BallDetectionResult> results)
        {
            foreach (var result in results)
            {
                Transform ballTransform = GetBallTransform(result.Key);
                if (ballTransform != null && tableCalibrator != null)
                {
                    Vector3 worldPos = tableCalibrator.TableToWorldPosition(result.Value.tablePosition);
                    ballTransform.position = worldPos;
                }
            }
        }
        
        private Transform GetBallTransform(BallColor color)
        {
            switch (color)
            {
                case BallColor.White: return whiteBall;
                case BallColor.Yellow: return yellowBall;
                case BallColor.Red: return redBall;
                default: return null;
            }
        }
        
        public Vector3 GetBallPosition(BallColor color)
        {
            if (lastDetectionResults.ContainsKey(color))
            {
                return lastDetectionResults[color].tablePosition;
            }
            return Vector3.zero;
        }
        
        public float GetBallConfidence(BallColor color)
        {
            if (lastDetectionResults.ContainsKey(color))
            {
                return lastDetectionResults[color].confidence;
            }
            return 0f;
        }
        
        public float GetAverageConfidence()
        {
            if (lastDetectionResults.Count == 0) return 0f;
            
            float total = 0f;
            foreach (var result in lastDetectionResults.Values)
            {
                total += result.confidence;
            }
            return total / lastDetectionResults.Count;
        }
        
        private void AutoDetectBalls()
        {
            if (autoDetect && !isDetecting && tableCalibrator != null && tableCalibrator.IsCalibrated)
            {
                DetectBalls();
            }
        }
        
        public void SetAutoDetect(bool enabled)
        {
            autoDetect = enabled;
            
            if (enabled)
            {
                InvokeRepeating(nameof(AutoDetectBalls), detectInterval, detectInterval);
            }
            else
            {
                CancelInvoke(nameof(AutoDetectBalls));
            }
        }
        
        void OnDestroy()
        {
            if (frameTexture != null)
            {
                DestroyImmediate(frameTexture);
            }
            
            if (tempRenderTexture != null)
            {
                tempRenderTexture.Release();
                DestroyImmediate(tempRenderTexture);
            }
        }
    }
}