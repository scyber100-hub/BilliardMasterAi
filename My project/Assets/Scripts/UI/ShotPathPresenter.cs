using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using BilliardMasterAi.Recommendation;

namespace BilliardMasterAi.UI
{
    public class ShotPathPresenter : MonoBehaviour
    {
        [Header("Path Rendering")]
        [SerializeField] private LineRenderer pathLine;
        [SerializeField] private LineRenderer cushionMarkers;
        
        [Header("UI Display")]
        [SerializeField] private TextMeshProUGUI strengthText;
        [SerializeField] private TextMeshProUGUI spinText;
        [SerializeField] private TextMeshProUGUI difficultyText;
        [SerializeField] private TextMeshProUGUI successProbabilityText;
        [SerializeField] private TextMeshProUGUI cushionCountText;
        
        [Header("Visual Settings")]
        [SerializeField] private Color pathColor = Color.cyan;
        [SerializeField] private Color cushionColor = Color.yellow;
        [SerializeField] private float lineWidth = 0.05f;
        [SerializeField] private Material pathMaterial;
        [SerializeField] private Material cushionMaterial;
        
        [Header("Animation")]
        [SerializeField] private bool animatePath = true;
        [SerializeField] private float animationDuration = 2f;
        [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        private ShotPlan currentPlan;
        private bool isDisplaying = false;
        private float animationTime = 0f;
        
        public ShotPlan CurrentPlan => currentPlan;
        public bool IsDisplaying => isDisplaying;
        
        void Start()
        {
            InitializeComponents();
        }
        
        void Update()
        {
            if (isDisplaying && animatePath)
            {
                UpdateAnimation();
            }
        }
        
        private void InitializeComponents()
        {
            // Path Line Renderer 설정
            if (pathLine == null)
            {
                GameObject pathObj = new GameObject("PathLine");
                pathObj.transform.SetParent(transform);
                pathLine = pathObj.AddComponent<LineRenderer>();
            }
            
            SetupLineRenderer(pathLine, pathColor, pathMaterial);
            
            // Cushion Markers Line Renderer 설정
            if (cushionMarkers == null)
            {
                GameObject cushionObj = new GameObject("CushionMarkers");
                cushionObj.transform.SetParent(transform);
                cushionMarkers = cushionObj.AddComponent<LineRenderer>();
            }
            
            SetupLineRenderer(cushionMarkers, cushionColor, cushionMaterial);
            cushionMarkers.useWorldSpace = false; // UI용으로 로컬 좌표 사용
            
            // 초기 상태 설정
            ClearPath();
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
        
        public void DisplayPath(ShotPlan plan)
        {
            if (plan == null)
            {
                ClearPath();
                return;
            }
            
            currentPlan = plan;
            isDisplaying = true;
            animationTime = 0f;
            
            // 궤적 표시
            DisplayTrajectory(plan.trajectory);
            
            // 쿠션 마커 표시
            DisplayCushionMarkers(plan.cushions);
            
            // UI 텍스트 업데이트
            UpdateParameterDisplay(plan);
            
            Debug.Log($"Displaying shot plan: Speed={plan.speed:F1}, Spin={plan.spinZ:F2}, Success={plan.successProbability:F2}");
        }
        
        private void DisplayTrajectory(List<Vector3> trajectory)
        {
            if (trajectory == null || trajectory.Count < 2)
            {
                pathLine.positionCount = 0;
                return;
            }
            
            // 화면 좌표계로 변환하여 표시 (3D 월드가 아닌 UI 오버레이로)
            Vector3[] uiPositions = ConvertToUIPositions(trajectory);
            
            pathLine.positionCount = uiPositions.Length;
            pathLine.SetPositions(uiPositions);
        }
        
        private void DisplayCushionMarkers(List<CushionCollision> cushions)
        {
            if (cushions == null || cushions.Count == 0)
            {
                cushionMarkers.positionCount = 0;
                return;
            }
            
            // 쿠션 지점들을 마커로 표시
            List<Vector3> markerPositions = new List<Vector3>();
            
            foreach (var cushion in cushions)
            {
                Vector3 uiPos = ConvertTableToUIPosition(cushion.position);
                markerPositions.Add(uiPos);
                
                // 각 쿠션에서 작은 십자가 모양 추가
                AddCushionCrossMarker(markerPositions, uiPos);
            }
            
            cushionMarkers.positionCount = markerPositions.Count;
            cushionMarkers.SetPositions(markerPositions.ToArray());
        }
        
        private void AddCushionCrossMarker(List<Vector3> positions, Vector3 center)
        {
            float size = 0.02f;
            
            // 십자가 모양의 선분 추가
            positions.Add(center + Vector3.left * size);
            positions.Add(center + Vector3.right * size);
            positions.Add(center);
            positions.Add(center + Vector3.up * size);
            positions.Add(center + Vector3.down * size);
            positions.Add(center);
        }
        
        private Vector3[] ConvertToUIPositions(List<Vector3> tablePositions)
        {
            Vector3[] uiPositions = new Vector3[tablePositions.Count];
            
            for (int i = 0; i < tablePositions.Count; i++)
            {
                uiPositions[i] = ConvertTableToUIPosition(tablePositions[i]);
            }
            
            return uiPositions;
        }
        
        private Vector3 ConvertTableToUIPosition(Vector3 tablePos)
        {
            // 테이블 좌표를 UI 화면 좌표로 변환
            // 테이블 크기 (2.84m x 1.42m)를 화면 영역에 맞게 스케일링
            
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null) return tablePos;
            
            Rect rect = rectTransform.rect;
            
            // 테이블 좌표를 정규화 (-1 ~ 1)
            float normalizedX = tablePos.x / 1.42f; // 테이블 반폭
            float normalizedZ = tablePos.z / 0.71f; // 테이블 반높이
            
            // UI 좌표로 변환
            float uiX = normalizedX * (rect.width * 0.5f);
            float uiY = normalizedZ * (rect.height * 0.5f);
            
            return new Vector3(uiX, uiY, 0f);
        }
        
        private void UpdateParameterDisplay(ShotPlan plan)
        {
            // 강도 표시 (0-100%)
            if (strengthText != null)
            {
                float strengthPercent = ((plan.speed - 1.5f) / (4.0f - 1.5f)) * 100f;
                strengthText.text = $"강도: {strengthPercent:F0}%";
                strengthText.color = GetColorByValue(strengthPercent / 100f, Color.green, Color.red);
            }
            
            // 스핀 표시
            if (spinText != null)
            {
                string spinDirection = plan.spinZ > 0 ? "우회전" : plan.spinZ < 0 ? "좌회전" : "직진";
                float spinPercent = Mathf.Abs(plan.spinZ) * 100f;
                spinText.text = $"스핀: {spinDirection} {spinPercent:F0}%";
                spinText.color = GetColorByValue(spinPercent / 100f, Color.white, Color.yellow);
            }
            
            // 난이도 표시
            if (difficultyText != null)
            {
                string difficultyLevel = GetDifficultyLevel(plan.difficulty);
                difficultyText.text = $"난이도: {difficultyLevel}";
                difficultyText.color = GetColorByValue(plan.difficulty, Color.green, Color.red);
            }
            
            // 성공 확률 표시
            if (successProbabilityText != null)
            {
                successProbabilityText.text = $"성공률: {plan.successProbability * 100f:F0}%";
                successProbabilityText.color = GetColorByValue(plan.successProbability, Color.red, Color.green);
            }
            
            // 쿠션 개수 표시
            if (cushionCountText != null)
            {
                cushionCountText.text = $"쿠션: {plan.cushionCount}개";
                cushionCountText.color = plan.cushionCount >= 3 && plan.cushionCount <= 4 ? Color.green : Color.orange;
            }
        }
        
        private string GetDifficultyLevel(float difficulty)
        {
            if (difficulty < 0.3f) return "쉬움";
            if (difficulty < 0.6f) return "보통";
            if (difficulty < 0.8f) return "어려움";
            return "매우 어려움";
        }
        
        private Color GetColorByValue(float value, Color lowColor, Color highColor)
        {
            return Color.Lerp(lowColor, highColor, value);
        }
        
        private void UpdateAnimation()
        {
            animationTime += Time.deltaTime / animationDuration;
            
            if (animationTime > 1f)
            {
                animationTime = 0f; // 반복 애니메이션
            }
            
            float progress = animationCurve.Evaluate(animationTime);
            
            // 궤적 라인 애니메이션 (점진적 표시)
            if (currentPlan != null && currentPlan.trajectory.Count > 1)
            {
                int visiblePoints = Mathf.RoundToInt(progress * currentPlan.trajectory.Count);
                visiblePoints = Mathf.Clamp(visiblePoints, 2, currentPlan.trajectory.Count);
                
                Vector3[] animatedPositions = new Vector3[visiblePoints];
                Vector3[] allPositions = ConvertToUIPositions(currentPlan.trajectory);
                
                for (int i = 0; i < visiblePoints; i++)
                {
                    animatedPositions[i] = allPositions[i];
                }
                
                pathLine.positionCount = visiblePoints;
                pathLine.SetPositions(animatedPositions);
            }
            
            // 색상 애니메이션
            Color animatedColor = pathColor;
            animatedColor.a = 0.5f + 0.5f * Mathf.Sin(animationTime * Mathf.PI * 2f);
            pathLine.color = animatedColor;
        }
        
        public void ClearPath()
        {
            currentPlan = null;
            isDisplaying = false;
            animationTime = 0f;
            
            if (pathLine != null)
                pathLine.positionCount = 0;
            
            if (cushionMarkers != null)
                cushionMarkers.positionCount = 0;
            
            // UI 텍스트 초기화
            if (strengthText != null) strengthText.text = "강도: --";
            if (spinText != null) spinText.text = "스핀: --";
            if (difficultyText != null) difficultyText.text = "난이도: --";
            if (successProbabilityText != null) successProbabilityText.text = "성공률: --%";
            if (cushionCountText != null) cushionCountText.text = "쿠션: --";
        }
        
        public void SetPathColor(Color color)
        {
            pathColor = color;
            if (pathLine != null)
                pathLine.color = color;
        }
        
        public void SetAnimationEnabled(bool enabled)
        {
            animatePath = enabled;
            
            if (!enabled && pathLine != null)
            {
                pathLine.color = pathColor;
            }
        }
        
        // 특정 구간 하이라이트
        public void HighlightSegment(int startIndex, int endIndex)
        {
            if (currentPlan == null || currentPlan.trajectory.Count == 0) return;
            
            // 구현: 특정 구간만 다른 색상으로 표시
            // (복잡한 구현이므로 기본 버전만 제공)
        }
        
        void OnDestroy()
        {
            // 동적으로 생성된 머티리얼 정리
            if (pathLine != null && pathLine.material != null)
            {
                DestroyImmediate(pathLine.material);
            }
            
            if (cushionMarkers != null && cushionMarkers.material != null)
            {
                DestroyImmediate(cushionMarkers.material);
            }
        }
    }
}