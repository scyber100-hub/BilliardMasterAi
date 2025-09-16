using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using BilliardMasterAi.Physics;

namespace BilliardMasterAi.Recommendation
{
    [System.Serializable]
    public class ShotPlan
    {
        public Vector3 direction;        // 샷 방향
        public float speed;              // 샷 속도
        public float spinZ;              // 사이드 스핀 (-1 to 1)
        public List<Vector3> trajectory; // 예상 궤적
        public List<CushionCollision> cushions; // 쿠션 충돌 정보
        public float successProbability; // 성공 확률 (0-1)
        public float difficulty;         // 난이도 (0-1)
        public int cushionCount;         // 쿠션 개수
        public float pathLength;         // 경로 길이
        
        public ShotPlan()
        {
            trajectory = new List<Vector3>();
            cushions = new List<CushionCollision>();
        }
    }
    
    [System.Serializable]
    public class ShotParameters
    {
        public float minSpeed = 1.5f;
        public float maxSpeed = 4.0f;
        public float speedStep = 0.2f;
        public float minAngle = -45f;
        public float maxAngle = 45f;
        public float angleStep = 5f;
        public float minSpin = -0.8f;
        public float maxSpin = 0.8f;
        public float spinStep = 0.4f;
        public int maxCushions = 5;
        public float targetRadius = 0.1f; // 목표 도달 허용 반경
    }
    
    public class ShotPlanner : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private CaromPhysics physicsEngine;
        
        [Header("Shot Parameters")]
        [SerializeField] private ShotParameters parameters = new ShotParameters();
        
        [Header("Evaluation Weights")]
        [SerializeField] private float cushionWeight = 0.3f;      // 쿠션 개수 가중치
        [SerializeField] private float lengthWeight = 0.2f;       // 경로 길이 가중치
        [SerializeField] private float speedWeight = 0.2f;        // 속도 가중치
        [SerializeField] private float accuracyWeight = 0.3f;     // 정확도 가중치
        
        public ShotParameters Parameters => parameters;
        
        void Start()
        {
            if (physicsEngine == null)
                physicsEngine = GetComponent<CaromPhysics>();
        }
        
        // 메인 샷 플래닝 함수 - 3쿠션 당구용
        public List<ShotPlan> PlanShot(Vector3 cuePosition, Vector3 targetPosition, Vector3 obstaclePosition = default)
        {
            List<ShotPlan> allPlans = new List<ShotPlan>();
            
            // 가능한 모든 샷 조합 탐색
            for (float angle = parameters.minAngle; angle <= parameters.maxAngle; angle += parameters.angleStep)
            {
                for (float speed = parameters.minSpeed; speed <= parameters.maxSpeed; speed += parameters.speedStep)
                {
                    for (float spin = parameters.minSpin; spin <= parameters.maxSpin; spin += parameters.spinStep)
                    {
                        ShotPlan plan = SimulateShotPlan(cuePosition, targetPosition, angle, speed, spin);
                        
                        if (IsValidCaromShot(plan, cuePosition, targetPosition, obstaclePosition))
                        {
                            EvaluateShotPlan(plan, cuePosition, targetPosition);
                            allPlans.Add(plan);
                        }
                    }
                }
            }
            
            // 성공 확률 기준으로 정렬하여 상위 결과 반환
            return allPlans.OrderByDescending(p => p.successProbability).Take(10).ToList();
        }
        
        // 단일 샷 시뮬레이션
        private ShotPlan SimulateShotPlan(Vector3 start, Vector3 target, float angleDeg, float speed, float spin)
        {
            ShotPlan plan = new ShotPlan();
            
            // 기본 방향 계산
            Vector3 baseDirection = (target - start).normalized;
            
            // 각도 적용 (Y축 기준 회전)
            Quaternion rotation = Quaternion.AngleAxis(angleDeg, Vector3.up);
            plan.direction = rotation * baseDirection;
            
            plan.speed = speed;
            plan.spinZ = spin;
            
            // 초기 속도 계산 (스핀 고려)
            Vector3 initialVelocity = physicsEngine.CalculateInitialVelocity(plan.direction, speed, spin);
            
            // 물리 시뮬레이션
            BallState initialState = new BallState(start, initialVelocity, new Vector3(0, 0, spin * 10f));
            plan.trajectory = physicsEngine.SimulateTrajectory(initialState);
            plan.cushions = physicsEngine.SimulateCushionCollisions(initialState);
            
            // 기본 정보 계산
            plan.cushionCount = plan.cushions.Count;
            plan.pathLength = CalculatePathLength(plan.trajectory);
            
            return plan;
        }
        
        // 3쿠션 당구 유효성 검사
        private bool IsValidCaromShot(ShotPlan plan, Vector3 cuePos, Vector3 targetPos, Vector3 obstaclePos)
        {
            // 최소 3개 쿠션 필요
            if (plan.cushionCount < 3) return false;
            
            // 최대 쿠션 제한
            if (plan.cushionCount > parameters.maxCushions) return false;
            
            // 목표 지점 근처 도달 확인
            if (plan.trajectory.Count > 0)
            {
                Vector3 finalPos = plan.trajectory.Last();
                if (Vector3.Distance(finalPos, targetPos) > parameters.targetRadius)
                    return false;
            }
            
            // 장애물 회피 검사 (있는 경우)
            if (obstaclePos != default && IsPathBlockedByObstacle(plan.trajectory, obstaclePos, 0.15f))
                return false;
            
            return true;
        }
        
        // 샷 플랜 평가
        private void EvaluateShotPlan(ShotPlan plan, Vector3 cuePos, Vector3 targetPos)
        {
            float cushionScore = EvaluateCushionCount(plan.cushionCount);
            float lengthScore = EvaluatePathLength(plan.pathLength);
            float speedScore = EvaluateSpeed(plan.speed);
            float accuracyScore = EvaluateAccuracy(plan.trajectory, targetPos);
            
            plan.successProbability = 
                cushionScore * cushionWeight +
                lengthScore * lengthWeight +
                speedScore * speedWeight +
                accuracyScore * accuracyWeight;
            
            plan.difficulty = CalculateDifficulty(plan);
        }
        
        private float EvaluateCushionCount(int cushionCount)
        {
            // 3-4쿠션이 가장 좋음, 그 이상은 점수 감소
            if (cushionCount == 3) return 1.0f;
            if (cushionCount == 4) return 0.9f;
            if (cushionCount == 5) return 0.7f;
            return 0.5f;
        }
        
        private float EvaluatePathLength(float length)
        {
            // 적당한 길이가 좋음 (너무 길거나 짧으면 감점)
            float optimalLength = 4.0f; // 미터
            float deviation = Mathf.Abs(length - optimalLength) / optimalLength;
            return Mathf.Clamp01(1.0f - deviation);
        }
        
        private float EvaluateSpeed(float speed)
        {
            // 중간 속도가 가장 안정적
            float optimalSpeed = (parameters.minSpeed + parameters.maxSpeed) * 0.5f;
            float range = parameters.maxSpeed - parameters.minSpeed;
            float deviation = Mathf.Abs(speed - optimalSpeed) / range;
            return Mathf.Clamp01(1.0f - deviation);
        }
        
        private float EvaluateAccuracy(List<Vector3> trajectory, Vector3 target)
        {
            if (trajectory.Count == 0) return 0f;
            
            Vector3 finalPos = trajectory.Last();
            float distance = Vector3.Distance(finalPos, target);
            
            // 거리에 따른 정확도 점수
            return Mathf.Clamp01(1.0f - distance / parameters.targetRadius);
        }
        
        private float CalculateDifficulty(ShotPlan plan)
        {
            float difficulty = 0f;
            
            // 쿠션 개수에 따른 난이도
            difficulty += (plan.cushionCount - 3) * 0.2f;
            
            // 속도에 따른 난이도
            float speedRange = parameters.maxSpeed - parameters.minSpeed;
            float speedNormalized = (plan.speed - parameters.minSpeed) / speedRange;
            difficulty += speedNormalized * 0.3f;
            
            // 스핀에 따른 난이도
            difficulty += Mathf.Abs(plan.spinZ) * 0.3f;
            
            // 경로 길이에 따른 난이도
            float normalizedLength = plan.pathLength / 10.0f; // 10m를 최대로 가정
            difficulty += normalizedLength * 0.2f;
            
            return Mathf.Clamp01(difficulty);
        }
        
        private float CalculatePathLength(List<Vector3> trajectory)
        {
            if (trajectory.Count < 2) return 0f;
            
            float length = 0f;
            for (int i = 1; i < trajectory.Count; i++)
            {
                length += Vector3.Distance(trajectory[i-1], trajectory[i]);
            }
            return length;
        }
        
        private bool IsPathBlockedByObstacle(List<Vector3> trajectory, Vector3 obstacle, float blockRadius)
        {
            for (int i = 1; i < trajectory.Count; i++)
            {
                Vector3 segmentStart = trajectory[i-1];
                Vector3 segmentEnd = trajectory[i];
                
                float distToObstacle = DistancePointToLineSegment(obstacle, segmentStart, segmentEnd);
                if (distToObstacle < blockRadius)
                    return true;
            }
            return false;
        }
        
        private float DistancePointToLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 line = lineEnd - lineStart;
            float lineLength = line.magnitude;
            
            if (lineLength == 0f) return Vector3.Distance(point, lineStart);
            
            float t = Mathf.Clamp01(Vector3.Dot(point - lineStart, line) / (lineLength * lineLength));
            Vector3 projection = lineStart + t * line;
            
            return Vector3.Distance(point, projection);
        }
        
        // 빠른 추천용 - 상위 3개 샷만 반환
        public List<ShotPlan> GetTopRecommendations(Vector3 cuePos, Vector3 targetPos, Vector3 obstaclePos = default)
        {
            return PlanShot(cuePos, targetPos, obstaclePos).Take(3).ToList();
        }
        
        // 특정 샷 파라미터로 시뮬레이션
        public ShotPlan SimulateSpecificShot(Vector3 cuePos, Vector3 targetPos, float angleDeg, float speed, float spin)
        {
            ShotPlan plan = SimulateShotPlan(cuePos, targetPos, angleDeg, speed, spin);
            EvaluateShotPlan(plan, cuePos, targetPos);
            return plan;
        }
    }
}