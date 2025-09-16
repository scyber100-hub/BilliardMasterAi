using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using BilliardMasterAi.AR;
using BilliardMasterAi.Recommendation;
using BilliardMasterAi.Perception;

namespace BilliardMasterAi.UI
{
    public class StudentTrainingScreenController : MonoBehaviour
    {
        [Header("AR Components")]
        [SerializeField] private ARTableCalibrator tableCalibrator;
        [SerializeField] private BallRecognitionController recognition;
        [SerializeField] private ARGuideOverlay overlay;
        
        [Header("Ball Transforms")]
        [SerializeField] private Transform cueBall;
        [SerializeField] private Transform targetBall;
        [SerializeField] private Transform obstacleBall;
        
        [Header("UI Elements")]
        [SerializeField] private Button detectAndSimulateButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Toggle showARGuideToggle;
        [SerializeField] private Toggle showScreenGuideToggle;
        
        [Header("Path Display")]
        [SerializeField] private ShotPathPresenter pathA;
        [SerializeField] private ShotPathPresenter pathB;
        
        [Header("Status Display")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI ballPositionsText;
        [SerializeField] private Slider confidenceSlider;
        
        [Header("Training Parameters")]
        [SerializeField] private bool autoDetect = true;
        [SerializeField] private float autoDetectInterval = 2f;
        
        private ShotPlanner shotPlanner;
        private List<ShotPlan> currentRecommendations;
        private bool isProcessing = false;
        private float lastAutoDetectTime = 0f;
        
        public System.Action<List<ShotPlan>> OnRecommendationsGenerated;
        
        void Start()
        {
            InitializeComponents();
            SetupUI();
            
            if (autoDetect)
            {
                InvokeRepeating(nameof(AutoDetectAndUpdate), autoDetectInterval, autoDetectInterval);
            }
        }
        
        void Update()
        {
            UpdateBallPositionDisplay();
            
            if (recognition != null && recognition.IsDetecting)
            {
                UpdateConfidenceDisplay();
            }
        }
        
        private void InitializeComponents()
        {
            // 컴포넌트 자동 검색
            if (tableCalibrator == null)
                tableCalibrator = FindObjectOfType<ARTableCalibrator>();
            
            if (recognition == null)
                recognition = FindObjectOfType<BallRecognitionController>();
            
            if (overlay == null)
                overlay = FindObjectOfType<ARGuideOverlay>();
            
            shotPlanner = FindObjectOfType<ShotPlanner>();
            
            // 테이블 캘리브레이션 완료 이벤트 구독
            if (tableCalibrator != null)
            {
                tableCalibrator.OnTableCalibrated += OnTableCalibrated;
            }
        }
        
        private void SetupUI()
        {
            // 버튼 이벤트 설정
            if (detectAndSimulateButton != null)
            {
                detectAndSimulateButton.onClick.AddListener(DetectAndSimulate);
            }
            
            if (resetButton != null)
            {
                resetButton.onClick.AddListener(ResetTraining);
            }
            
            // 토글 이벤트 설정
            if (showARGuideToggle != null)
            {
                showARGuideToggle.onValueChanged.AddListener(OnARGuideToggle);
                showARGuideToggle.isOn = true;
            }
            
            if (showScreenGuideToggle != null)
            {
                showScreenGuideToggle.onValueChanged.AddListener(OnScreenGuideToggle);
                showScreenGuideToggle.isOn = true;
            }
            
            // 초기 상태 설정
            UpdateStatusText("테이블 캘리브레이션을 완료해주세요.");
            SetUIInteractable(false);
        }
        
        private void OnTableCalibrated()
        {
            UpdateStatusText("테이블 캘리브레이션 완료! 공 인식을 시작할 수 있습니다.");
            SetUIInteractable(true);
            
            // 자동 인식 설정
            if (recognition != null)
            {
                recognition.autoDetect = true;
                recognition.placeTransforms = true;
            }
        }
        
        public void DetectAndSimulate()
        {
            if (isProcessing) return;
            
            StartCoroutine(DetectAndSimulateCoroutine());
        }
        
        private System.Collections.IEnumerator DetectAndSimulateCoroutine()
        {
            isProcessing = true;
            UpdateStatusText("공 위치 인식 중...");
            
            // 공 인식 실행
            if (recognition != null)
            {
                recognition.DetectBalls();
                
                // 인식 완료까지 대기
                float timeout = 5f;
                float elapsed = 0f;
                
                while (recognition.IsDetecting && elapsed < timeout)
                {
                    yield return new WaitForSeconds(0.1f);
                    elapsed += 0.1f;
                }
                
                if (elapsed >= timeout)
                {
                    UpdateStatusText("공 인식 시간 초과. 다시 시도해주세요.");
                    isProcessing = false;
                    yield break;
                }
            }
            
            // 공 위치 업데이트
            UpdateBallPositions();
            
            // 샷 추천 계산
            yield return StartCoroutine(GenerateRecommendations());
            
            isProcessing = false;
        }
        
        private void UpdateBallPositions()
        {
            if (recognition == null || tableCalibrator == null) return;
            
            // BallRecognitionController에서 인식된 위치 가져오기
            Vector3 whiteBallPos = recognition.GetBallPosition(BallColor.White);
            Vector3 yellowBallPos = recognition.GetBallPosition(BallColor.Yellow);
            Vector3 redBallPos = recognition.GetBallPosition(BallColor.Red);
            
            // Transform 업데이트
            if (cueBall != null && whiteBallPos != Vector3.zero)
            {
                cueBall.position = tableCalibrator.TableToWorldPosition(whiteBallPos);
            }
            
            if (targetBall != null && redBallPos != Vector3.zero)
            {
                targetBall.position = tableCalibrator.TableToWorldPosition(redBallPos);
            }
            
            if (obstacleBall != null && yellowBallPos != Vector3.zero)
            {
                obstacleBall.position = tableCalibrator.TableToWorldPosition(yellowBallPos);
            }
        }
        
        private System.Collections.IEnumerator GenerateRecommendations()
        {
            UpdateStatusText("최적 경로 계산 중...");
            
            if (shotPlanner == null || cueBall == null || targetBall == null)
            {
                UpdateStatusText("샷 플래닝 컴포넌트가 누락되었습니다.");
                yield break;
            }
            
            // 테이블 좌표로 변환
            Vector3 cuePos = tableCalibrator.ScreenToTablePosition(cueBall.position);
            Vector3 targetPos = tableCalibrator.ScreenToTablePosition(targetBall.position);
            Vector3 obstaclePos = obstacleBall != null ? 
                tableCalibrator.ScreenToTablePosition(obstacleBall.position) : Vector3.zero;
            
            // 비동기적으로 추천 계산 (프레임 드랍 방지)
            yield return new WaitForEndOfFrame();
            
            currentRecommendations = shotPlanner.GetTopRecommendations(cuePos, targetPos, obstaclePos);
            
            if (currentRecommendations != null && currentRecommendations.Count > 0)
            {
                DisplayRecommendations();
                UpdateStatusText($"{currentRecommendations.Count}개의 추천 경로를 찾았습니다.");
                
                OnRecommendationsGenerated?.Invoke(currentRecommendations);
            }
            else
            {
                UpdateStatusText("유효한 3쿠션 경로를 찾을 수 없습니다.");
            }
        }
        
        private void DisplayRecommendations()
        {
            if (currentRecommendations == null || currentRecommendations.Count == 0) return;
            
            // AR 가이드 표시
            if (showARGuideToggle.isOn && overlay != null)
            {
                overlay.ShowPlan(currentRecommendations[0]);
                overlay.SetGuideColorBySuccessRate(currentRecommendations[0].successProbability);
            }
            
            // 화면 가이드 표시
            if (showScreenGuideToggle.isOn)
            {
                if (pathA != null && currentRecommendations.Count > 0)
                {
                    pathA.DisplayPath(currentRecommendations[0]);
                }
                
                if (pathB != null && currentRecommendations.Count > 1)
                {
                    pathB.DisplayPath(currentRecommendations[1]);
                }
            }
        }
        
        private void AutoDetectAndUpdate()
        {
            if (!autoDetect || isProcessing || Time.time - lastAutoDetectTime < autoDetectInterval)
                return;
            
            if (tableCalibrator != null && tableCalibrator.IsCalibrated)
            {
                lastAutoDetectTime = Time.time;
                DetectAndSimulate();
            }
        }
        
        private void OnARGuideToggle(bool isOn)
        {
            if (overlay != null)
            {
                if (isOn && currentRecommendations != null && currentRecommendations.Count > 0)
                {
                    overlay.ShowPlan(currentRecommendations[0]);
                }
                else
                {
                    overlay.HidePlan();
                }
            }
        }
        
        private void OnScreenGuideToggle(bool isOn)
        {
            if (pathA != null) pathA.gameObject.SetActive(isOn);
            if (pathB != null) pathB.gameObject.SetActive(isOn);
            
            if (isOn && currentRecommendations != null)
            {
                DisplayRecommendations();
            }
        }
        
        private void ResetTraining()
        {
            currentRecommendations = null;
            
            if (overlay != null) overlay.HidePlan();
            if (pathA != null) pathA.ClearPath();
            if (pathB != null) pathB.ClearPath();
            
            UpdateStatusText("훈련이 초기화되었습니다.");
        }
        
        private void UpdateStatusText(string message)
        {
            if (statusText != null)
            {
                statusText.text = $"[{System.DateTime.Now:HH:mm:ss}] {message}";
            }
            
            Debug.Log($"StudentTraining: {message}");
        }
        
        private void UpdateBallPositionDisplay()
        {
            if (ballPositionsText == null) return;
            
            string text = "공 위치:\n";
            
            if (cueBall != null)
                text += $"수구: {cueBall.position:F2}\n";
            
            if (targetBall != null)
                text += $"목적구: {targetBall.position:F2}\n";
            
            if (obstacleBall != null)
                text += $"장애구: {obstacleBall.position:F2}";
            
            ballPositionsText.text = text;
        }
        
        private void UpdateConfidenceDisplay()
        {
            if (confidenceSlider != null && recognition != null)
            {
                confidenceSlider.value = recognition.GetAverageConfidence();
            }
        }
        
        private void SetUIInteractable(bool interactable)
        {
            if (detectAndSimulateButton != null)
                detectAndSimulateButton.interactable = interactable;
            
            if (resetButton != null)
                resetButton.interactable = interactable;
        }
        
        void OnDestroy()
        {
            if (tableCalibrator != null)
            {
                tableCalibrator.OnTableCalibrated -= OnTableCalibrated;
            }
        }
    }
    
    // 공 색상 열거형
    public enum BallColor
    {
        White,
        Yellow,
        Red
    }
}