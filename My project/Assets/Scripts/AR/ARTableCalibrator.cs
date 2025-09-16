using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

namespace BilliardMasterAi.AR
{
    public class ARTableCalibrator : MonoBehaviour
    {
        [Header("AR Components")]
        [SerializeField] private ARRaycastManager raycastManager;
        [SerializeField] private ARPlaneManager planeManager;
        [SerializeField] private Camera arCamera;
        
        [Header("Table Setup")]
        [SerializeField] private GameObject tablePrefab;
        [SerializeField] private Transform tableRoot;
        [SerializeField] private float tableWidth = 2.84f;  // 캐롬대 표준 크기
        [SerializeField] private float tableHeight = 1.42f;
        
        [Header("Calibration")]
        [SerializeField] private Transform[] cornerMarkers = new Transform[4];
        [SerializeField] private LineRenderer tableOutline;
        
        private bool isCalibrated = false;
        private Vector3[] tableCorners = new Vector3[4];
        private int currentCornerIndex = 0;
        
        public bool IsCalibrated => isCalibrated;
        public Transform TableRoot => tableRoot;
        public Vector3[] TableCorners => tableCorners;
        
        public System.Action OnTableCalibrated;
        
        void Start()
        {
            if (tableOutline == null)
            {
                CreateTableOutline();
            }
        }
        
        void Update()
        {
            if (!isCalibrated && Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    PlaceCornerMarker(touch.position);
                }
            }
        }
        
        private void PlaceCornerMarker(Vector2 screenPosition)
        {
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            
            if (raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                ARRaycastHit hit = hits[0];
                Vector3 worldPosition = hit.pose.position;
                
                // 코너 마커 배치
                if (currentCornerIndex < 4)
                {
                    tableCorners[currentCornerIndex] = worldPosition;
                    
                    if (cornerMarkers[currentCornerIndex] != null)
                    {
                        cornerMarkers[currentCornerIndex].position = worldPosition;
                        cornerMarkers[currentCornerIndex].gameObject.SetActive(true);
                    }
                    
                    currentCornerIndex++;
                    
                    UpdateTableOutline();
                    
                    if (currentCornerIndex >= 4)
                    {
                        CompleteCalibration();
                    }
                }
            }
        }
        
        private void CreateTableOutline()
        {
            GameObject outlineObj = new GameObject("TableOutline");
            outlineObj.transform.SetParent(transform);
            
            tableOutline = outlineObj.AddComponent<LineRenderer>();
            tableOutline.material = Resources.Load<Material>("Materials/TableOutline");
            tableOutline.color = Color.green;
            tableOutline.width = 0.02f;
            tableOutline.positionCount = 5; // 사각형 + 닫힌 선
            tableOutline.useWorldSpace = true;
        }
        
        private void UpdateTableOutline()
        {
            if (tableOutline == null || currentCornerIndex < 2) return;
            
            tableOutline.positionCount = currentCornerIndex + 1;
            
            for (int i = 0; i < currentCornerIndex; i++)
            {
                tableOutline.SetPosition(i, tableCorners[i] + Vector3.up * 0.01f);
            }
            
            // 현재까지의 마지막 점을 첫 번째 점과 연결 (미완성 시)
            if (currentCornerIndex > 2)
            {
                tableOutline.SetPosition(currentCornerIndex, tableCorners[0] + Vector3.up * 0.01f);
            }
        }
        
        private void CompleteCalibration()
        {
            isCalibrated = true;
            
            // 테이블 중심점 계산
            Vector3 center = Vector3.zero;
            foreach (Vector3 corner in tableCorners)
            {
                center += corner;
            }
            center /= 4;
            
            // 테이블 루트 위치 설정
            if (tableRoot == null)
            {
                GameObject tableRootObj = new GameObject("TableRoot");
                tableRoot = tableRootObj.transform;
            }
            
            tableRoot.position = center;
            
            // 테이블 방향 계산 (첫 번째와 두 번째 코너 벡터 기준)
            Vector3 forward = (tableCorners[1] - tableCorners[0]).normalized;
            Vector3 right = (tableCorners[3] - tableCorners[0]).normalized;
            Vector3 up = Vector3.Cross(forward, right).normalized;
            
            tableRoot.rotation = Quaternion.LookRotation(forward, up);
            
            // 테이블 크기 조정
            float actualWidth = Vector3.Distance(tableCorners[0], tableCorners[1]);
            float actualHeight = Vector3.Distance(tableCorners[1], tableCorners[2]);
            
            Vector3 scale = new Vector3(
                actualWidth / tableWidth,
                1f,
                actualHeight / tableHeight
            );
            tableRoot.localScale = scale;
            
            // 최종 테이블 아웃라인 업데이트
            UpdateFinalTableOutline();
            
            Debug.Log("테이블 캘리브레이션 완료!");
            OnTableCalibrated?.Invoke();
        }
        
        private void UpdateFinalTableOutline()
        {
            tableOutline.positionCount = 5;
            for (int i = 0; i < 4; i++)
            {
                tableOutline.SetPosition(i, tableCorners[i] + Vector3.up * 0.01f);
            }
            tableOutline.SetPosition(4, tableCorners[0] + Vector3.up * 0.01f); // 닫힌 선
            
            tableOutline.color = Color.blue; // 완료 시 파란색으로 변경
        }
        
        public void ResetCalibration()
        {
            isCalibrated = false;
            currentCornerIndex = 0;
            
            foreach (Transform marker in cornerMarkers)
            {
                if (marker != null)
                    marker.gameObject.SetActive(false);
            }
            
            if (tableOutline != null)
            {
                tableOutline.positionCount = 0;
                tableOutline.color = Color.green;
            }
        }
        
        // 화면 좌표를 테이블 좌표로 변환
        public Vector3 ScreenToTablePosition(Vector2 screenPos)
        {
            if (!isCalibrated) return Vector3.zero;
            
            Ray ray = arCamera.ScreenPointToRay(screenPos);
            Plane tablePlane = new Plane(tableRoot.up, tableRoot.position);
            
            if (tablePlane.Raycast(ray, out float distance))
            {
                Vector3 worldPos = ray.GetPoint(distance);
                return tableRoot.InverseTransformPoint(worldPos);
            }
            
            return Vector3.zero;
        }
        
        // 테이블 좌표를 월드 좌표로 변환
        public Vector3 TableToWorldPosition(Vector3 tablePos)
        {
            if (!isCalibrated) return Vector3.zero;
            return tableRoot.TransformPoint(tablePos);
        }
    }
}