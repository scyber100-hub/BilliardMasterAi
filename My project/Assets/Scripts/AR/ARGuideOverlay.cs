using UnityEngine;
using System.Collections.Generic;
using BilliardMasterAi.Recommendation;
using BilliardMasterAi.Physics;

namespace BilliardMasterAi.AR
{
    public class ARGuideOverlay : MonoBehaviour
    {
        [Header("Guide Rendering")]
        [SerializeField] private LineRenderer trajectoryLine;
        [SerializeField] private LineRenderer cushionMarkers;
        [SerializeField] private Transform[] cushionIcons;
        
        [Header("Visual Settings")]
        [SerializeField] private Material trajectoryMaterial;
        [SerializeField] private Material cushionMaterial;
        [SerializeField] private Color trajectoryColor = Color.cyan;
        [SerializeField] private Color cushionColor = Color.yellow;
        [SerializeField] private float lineWidth = 0.02f;
        [SerializeField] private float cushionIconSize = 0.1f;
        
        [Header("Animation")]
        [SerializeField] private bool animateTrajectory = true;
        [SerializeField] private float animationSpeed = 2f;
        [SerializeField] private AnimationCurve trajectoryOpacity = AnimationCurve.EaseInOut(0, 1, 1, 0.3f);
        
        private ShotPlan currentPlan;
        private ARTableCalibrator tableCalibrator;
        private bool isVisible = false;
        private float animationTime = 0f;
        
        public bool IsVisible => isVisible;
        public ShotPlan CurrentPlan => currentPlan;
        
        void Start()
        {
            tableCalibrator = FindObjectOfType<ARTableCalibrator>();
            InitializeComponents();
        }
        
        void Update()
        {
            if (isVisible && animateTrajectory)
            {
                AnimateTrajectoryDisplay();
            }
        }
        
        private void InitializeComponents()
        {
            // 궤적 라인 렌더러 설정
            if (trajectoryLine == null)
            {
                GameObject trajObj = new GameObject("TrajectoryLine");
                trajObj.transform.SetParent(transform);
                trajectoryLine = trajObj.AddComponent<LineRenderer>();
            }
            
            SetupLineRenderer(trajectoryLine, trajectoryColor, trajectoryMaterial);
            
            // 쿠션 마커 라인 렌더러 설정
            if (cushionMarkers == null)
            {
                GameObject cushObj = new GameObject("CushionMarkers");
                cushObj.transform.SetParent(transform);
                cushionMarkers = cushObj.AddComponent<LineRenderer>();
            }
            
            SetupLineRenderer(cushionMarkers, cushionColor, cushionMaterial);
            
            // 쿠션 아이콘 생성
            CreateCushionIcons();
            
            // 초기에는 비활성화
            SetVisibility(false);
        }
        
        private void SetupLineRenderer(LineRenderer line, Color color, Material material)
        {
            line.material = material != null ? material : CreateDefaultMaterial(color);
            line.color = color;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.useWorldSpace = true;
            line.positionCount = 0;
        }
        
        private Material CreateDefaultMaterial(Color color)
        {
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = color;
            return mat;
        }
        
        private void CreateCushionIcons()
        {
            cushionIcons = new Transform[5]; // 최대 5개 쿠션
            
            for (int i = 0; i < cushionIcons.Length; i++)
            {
                GameObject icon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                icon.name = $"CushionIcon_{i}";
                icon.transform.SetParent(transform);
                icon.transform.localScale = Vector3.one * cushionIconSize;
                
                // 머티리얼 설정
                Renderer renderer = icon.GetComponent<Renderer>();
                renderer.material = CreateDefaultMaterial(cushionColor);
                
                cushionIcons[i] = icon.transform;
                icon.SetActive(false);
            }
        }
        
        // 샷 플랜을 표시
        public void ShowPlan(ShotPlan plan)
        {
            if (plan == null || tableCalibrator == null || !tableCalibrator.IsCalibrated)
            {
                Debug.LogWarning("Cannot show plan: Invalid plan or table not calibrated");
                return;
            }
            
            currentPlan = plan;
            isVisible = true;
            animationTime = 0f;
            
            DisplayTrajectory(plan.trajectory);
            DisplayCushionMarkers(plan.cushions);
            
            SetVisibility(true);
        }
        
        // 가이드 숨기기
        public void HidePlan()
        {
            currentPlan = null;
            isVisible = false;
            animationTime = 0f;
            
            SetVisibility(false);
        }
        
        private void DisplayTrajectory(List<Vector3> trajectory)
        {
            if (trajectory == null || trajectory.Count < 2)
            {
                trajectoryLine.positionCount = 0;
                return;
            }
            
            // 테이블 좌표를 월드 좌표로 변환
            Vector3[] worldPositions = new Vector3[trajectory.Count];
            for (int i = 0; i < trajectory.Count; i++)
            {
                worldPositions[i] = tableCalibrator.TableToWorldPosition(trajectory[i]);
                worldPositions[i] += Vector3.up * 0.02f; // 테이블 위로 살짝 띄움
            }
            
            trajectoryLine.positionCount = worldPositions.Length;
            trajectoryLine.SetPositions(worldPositions);
        }
        
        private void DisplayCushionMarkers(List<CushionCollision> cushions)
        {
            // 모든 쿠션 아이콘 비활성화
            foreach (Transform icon in cushionIcons)
            {
                icon.gameObject.SetActive(false);
            }
            
            if (cushions == null || cushions.Count == 0) return;
            
            // 쿠션 충돌 지점에 아이콘 배치
            for (int i = 0; i < Mathf.Min(cushions.Count, cushionIcons.Length); i++)
            {
                Vector3 worldPos = tableCalibrator.TableToWorldPosition(cushions[i].position);
                worldPos += Vector3.up * 0.05f; // 높이 조정
                
                cushionIcons[i].position = worldPos;
                cushionIcons[i].gameObject.SetActive(true);
                
                // 쿠션 순서에 따라 크기 조정 (첫 번째가 가장 큼)
                float scale = cushionIconSize * (1.0f - i * 0.1f);
                cushionIcons[i].localScale = Vector3.one * scale;
            }
        }
        
        private void AnimateTrajectoryDisplay()
        {
            animationTime += Time.deltaTime * animationSpeed;
            
            // 궤적 라인 애니메이션
            Color baseColor = trajectoryColor;
            float alpha = trajectoryOpacity.Evaluate(animationTime % 1f);
            baseColor.a = alpha;
            trajectoryLine.color = baseColor;
            
            // 쿠션 아이콘 깜빡임 효과
            float blinkPhase = Mathf.Sin(animationTime * 3f) * 0.5f + 0.5f;
            foreach (Transform icon in cushionIcons)
            {
                if (icon.gameObject.activeInHierarchy)
                {
                    Renderer renderer = icon.GetComponent<Renderer>();
                    Color iconColor = cushionColor;
                    iconColor.a = 0.5f + blinkPhase * 0.5f;
                    renderer.material.color = iconColor;
                }
            }
        }
        
        private void SetVisibility(bool visible)
        {
            trajectoryLine.gameObject.SetActive(visible);
            cushionMarkers.gameObject.SetActive(visible);
            
            foreach (Transform icon in cushionIcons)
            {
                if (!visible)
                    icon.gameObject.SetActive(false);
            }
        }
        
        // 실시간 업데이트용 - 새로운 공 위치로 가이드 갱신
        public void UpdateGuideForNewBallPositions(Vector3 cuePos, Vector3 targetPos, Vector3 obstaclePos = default)
        {
            ShotPlanner planner = FindObjectOfType<ShotPlanner>();
            if (planner != null)
            {
                var recommendations = planner.GetTopRecommendations(cuePos, targetPos, obstaclePos);
                if (recommendations.Count > 0)
                {
                    ShowPlan(recommendations[0]); // 최고 추천안 표시
                }
            }
        }
        
        // 가이드 색상 변경 (성공률에 따라)
        public void SetGuideColorBySuccessRate(float successRate)
        {
            Color color = Color.Lerp(Color.red, Color.green, successRate);
            trajectoryLine.color = color;
            
            foreach (Transform icon in cushionIcons)
            {
                if (icon.gameObject.activeInHierarchy)
                {
                    icon.GetComponent<Renderer>().material.color = color;
                }
            }
        }
        
        // 특정 구간만 하이라이트 (쿠션 간 구간별로)
        public void HighlightTrajectorySegment(int segmentIndex)
        {
            if (currentPlan == null || currentPlan.trajectory.Count < 2) return;
            
            // 구현: 특정 구간의 라인을 다른 색상으로 표시
            // (복잡한 구현이므로 기본 버전만 제공)
        }
        
        void OnDestroy()
        {
            // 동적으로 생성된 머티리얼 정리
            if (trajectoryLine != null && trajectoryLine.material != null)
            {
                DestroyImmediate(trajectoryLine.material);
            }
        }
    }
}