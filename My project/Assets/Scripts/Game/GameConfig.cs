using UnityEngine;

namespace BilliardMasterAi.Game
{
    [System.Serializable]
    public class GameState
    {
        public bool isLeagueMode = false;
        public float timeLimit = 300f; // 5분 기본
        public float remainingTime = 0f;
        public bool timerActive = false;
        public int shotCount = 0;
        public int successfulShots = 0;
        
        public float SuccessRate => shotCount > 0 ? (float)successfulShots / shotCount : 0f;
    }
    
    [CreateAssetMenu(fileName = "GameConfig", menuName = "BilliardMasterAI/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Game Settings")]
        public GameState gameState = new GameState();
        
        [Header("AR Settings")]
        public bool enableARFoundation = true;
        public bool enablePlaneDetection = true;
        public bool enableImageTracking = false;
        
        [Header("Physics Settings")]
        public float tableWidth = 2.84f;
        public float tableHeight = 1.42f;
        public float ballRadius = 0.028575f;
        public float gravityScale = 1f;
        
        [Header("UI Settings")]
        public bool showDebugInfo = false;
        public bool showPerformanceStats = false;
        public float uiScale = 1f;
        
        [Header("Training Settings")]
        public int maxRecommendations = 3;
        public bool autoDetectBalls = true;
        public float detectionInterval = 0.5f;
        public bool showARGuides = true;
        
        [Header("Quality Settings")]
        public int targetFrameRate = 60;
        public bool adaptiveQuality = true;
        public int qualityLevel = 2; // 0=Low, 1=Medium, 2=High
        
        private void OnEnable()
        {
            // 게임 시작 시 설정 적용
            ApplySettings();
        }
        
        public void ApplySettings()
        {
            // 프레임레이트 설정
            Application.targetFrameRate = targetFrameRate;
            
            // 품질 설정
            QualitySettings.SetQualityLevel(qualityLevel);
            
            // 물리 설정
            Physics.gravity = new Vector3(0, -9.81f * gravityScale, 0);
            
            Debug.Log($"GameConfig applied: FrameRate={targetFrameRate}, Quality={qualityLevel}");
        }
        
        public void StartTimer()
        {
            gameState.remainingTime = gameState.timeLimit;
            gameState.timerActive = true;
        }
        
        public void StopTimer()
        {
            gameState.timerActive = false;
        }
        
        public void UpdateTimer(float deltaTime)
        {
            if (gameState.timerActive && gameState.remainingTime > 0)
            {
                gameState.remainingTime -= deltaTime;
                
                if (gameState.remainingTime <= 0)
                {
                    gameState.remainingTime = 0;
                    gameState.timerActive = false;
                    OnTimeUp();
                }
            }
        }
        
        public void RecordShot(bool successful)
        {
            gameState.shotCount++;
            if (successful)
            {
                gameState.successfulShots++;
            }
        }
        
        public void ResetGameState()
        {
            gameState.shotCount = 0;
            gameState.successfulShots = 0;
            gameState.remainingTime = 0;
            gameState.timerActive = false;
        }
        
        private void OnTimeUp()
        {
            Debug.Log("Time's up! Game session ended.");
            // 게임 종료 처리
        }
        
        // 설정 저장/로드
        public void SaveSettings()
        {
            PlayerPrefs.SetInt("QualityLevel", qualityLevel);
            PlayerPrefs.SetInt("TargetFrameRate", targetFrameRate);
            PlayerPrefs.SetFloat("UIScale", uiScale);
            PlayerPrefs.SetInt("ShowDebugInfo", showDebugInfo ? 1 : 0);
            PlayerPrefs.SetInt("AutoDetectBalls", autoDetectBalls ? 1 : 0);
            PlayerPrefs.SetFloat("DetectionInterval", detectionInterval);
            PlayerPrefs.Save();
        }
        
        public void LoadSettings()
        {
            qualityLevel = PlayerPrefs.GetInt("QualityLevel", 2);
            targetFrameRate = PlayerPrefs.GetInt("TargetFrameRate", 60);
            uiScale = PlayerPrefs.GetFloat("UIScale", 1f);
            showDebugInfo = PlayerPrefs.GetInt("ShowDebugInfo", 0) == 1;
            autoDetectBalls = PlayerPrefs.GetInt("AutoDetectBalls", 1) == 1;
            detectionInterval = PlayerPrefs.GetFloat("DetectionInterval", 0.5f);
            
            ApplySettings();
        }
    }
}