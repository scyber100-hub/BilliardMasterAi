using UnityEngine;
using System.Collections.Generic;

namespace BilliardMasterAi.Physics
{
    [System.Serializable]
    public class BallState
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 angularVelocity;
        public float radius = 0.028575f; // 당구공 반지름 (미터)
        
        public BallState(Vector3 pos)
        {
            position = pos;
            velocity = Vector3.zero;
            angularVelocity = Vector3.zero;
        }
        
        public BallState(Vector3 pos, Vector3 vel, Vector3 angVel)
        {
            position = pos;
            velocity = vel;
            angularVelocity = angVel;
        }
        
        public BallState Clone()
        {
            return new BallState(position, velocity, angularVelocity);
        }
    }
    
    [System.Serializable]
    public class CushionCollision
    {
        public Vector3 position;
        public Vector3 normal;
        public float time;
        public int cushionIndex;
        
        public CushionCollision(Vector3 pos, Vector3 norm, float t, int index)
        {
            position = pos;
            normal = norm;
            time = t;
            cushionIndex = index;
        }
    }
    
    [System.Serializable]
    public class PhysicsConfig
    {
        public float friction = 0.15f;           // 마찰계수
        public float rollingFriction = 0.01f;    // 굴림 마찰
        public float cushionRestitution = 0.8f;  // 쿠션 반발계수
        public float ballRestitution = 0.9f;     // 공-공 반발계수
        public float spinDecay = 0.95f;          // 스핀 감소율
        public float minVelocity = 0.01f;        // 최소 속도 (정지 판정)
        public float tableWidth = 2.84f;        // 테이블 폭
        public float tableHeight = 1.42f;       // 테이블 높이
        public float cushionThickness = 0.05f;  // 쿠션 두께
    }
    
    public class CaromPhysics : MonoBehaviour
    {
        [Header("Physics Configuration")]
        [SerializeField] private PhysicsConfig config = new PhysicsConfig();
        
        [Header("Table Boundaries")]
        [SerializeField] private Vector3[] cushionNormals = new Vector3[4];
        [SerializeField] private float[] cushionDistances = new float[4];
        
        public PhysicsConfig Config => config;
        
        void Start()
        {
            InitializeCushions();
        }
        
        private void InitializeCushions()
        {
            // 쿠션 설정 (시계방향: 상, 우, 하, 좌)
            cushionNormals[0] = Vector3.back;   // 상단 쿠션
            cushionNormals[1] = Vector3.left;   // 우측 쿠션  
            cushionNormals[2] = Vector3.forward; // 하단 쿠션
            cushionNormals[3] = Vector3.right;   // 좌측 쿠션
            
            float halfWidth = config.tableWidth * 0.5f;
            float halfHeight = config.tableHeight * 0.5f;
            
            cushionDistances[0] = halfHeight - config.cushionThickness;  // 상단
            cushionDistances[1] = halfWidth - config.cushionThickness;   // 우측
            cushionDistances[2] = halfHeight - config.cushionThickness;  // 하단
            cushionDistances[3] = halfWidth - config.cushionThickness;   // 좌측
        }
        
        // 단일 프레임 물리 시뮬레이션
        public BallState SimulateStep(BallState ball, float deltaTime)
        {
            BallState newState = ball.Clone();
            
            // 위치 업데이트
            newState.position += newState.velocity * deltaTime;
            
            // 쿠션 충돌 검사
            CushionCollision collision = CheckCushionCollision(ball.position, newState.position, ball.radius);
            if (collision != null)
            {
                newState = HandleCushionCollision(newState, collision);
            }
            
            // 마찰 적용
            ApplyFriction(newState, deltaTime);
            
            // 스핀 감소
            newState.angularVelocity *= Mathf.Pow(config.spinDecay, deltaTime);
            
            // 최소 속도 검사
            if (newState.velocity.magnitude < config.minVelocity)
            {
                newState.velocity = Vector3.zero;
                newState.angularVelocity = Vector3.zero;
            }
            
            return newState;
        }
        
        // 전체 궤적 시뮬레이션
        public List<Vector3> SimulateTrajectory(BallState initialState, float maxTime = 10f, float timeStep = 0.02f)
        {
            List<Vector3> trajectory = new List<Vector3>();
            BallState currentState = initialState.Clone();
            float currentTime = 0f;
            
            trajectory.Add(currentState.position);
            
            while (currentTime < maxTime && currentState.velocity.magnitude > config.minVelocity)
            {
                currentState = SimulateStep(currentState, timeStep);
                trajectory.Add(currentState.position);
                currentTime += timeStep;
            }
            
            return trajectory;
        }
        
        // 쿠션과의 충돌 시뮬레이션 (전체 경로)
        public List<CushionCollision> SimulateCushionCollisions(BallState initialState, float maxTime = 10f, float timeStep = 0.02f)
        {
            List<CushionCollision> collisions = new List<CushionCollision>();
            BallState currentState = initialState.Clone();
            float currentTime = 0f;
            
            while (currentTime < maxTime && currentState.velocity.magnitude > config.minVelocity)
            {
                Vector3 prevPos = currentState.position;
                currentState = SimulateStep(currentState, timeStep);
                
                CushionCollision collision = CheckCushionCollision(prevPos, currentState.position, currentState.radius);
                if (collision != null)
                {
                    collision.time = currentTime;
                    collisions.Add(collision);
                }
                
                currentTime += timeStep;
            }
            
            return collisions;
        }
        
        private CushionCollision CheckCushionCollision(Vector3 startPos, Vector3 endPos, float radius)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector3 normal = cushionNormals[i];
                float distance = cushionDistances[i];
                
                // 시작점과 끝점의 쿠션으로부터의 거리
                float startDist = GetDistanceFromCushion(startPos, i) - radius;
                float endDist = GetDistanceFromCushion(endPos, i) - radius;
                
                // 충돌 발생 검사 (부호가 바뀌면 충돌)
                if (startDist > 0 && endDist <= 0)
                {
                    // 충돌 지점 계산
                    float t = startDist / (startDist - endDist);
                    Vector3 collisionPoint = Vector3.Lerp(startPos, endPos, t);
                    
                    return new CushionCollision(collisionPoint, normal, t, i);
                }
            }
            
            return null;
        }
        
        private float GetDistanceFromCushion(Vector3 position, int cushionIndex)
        {
            switch (cushionIndex)
            {
                case 0: return cushionDistances[0] - position.z;  // 상단
                case 1: return cushionDistances[1] - position.x;  // 우측
                case 2: return position.z + cushionDistances[2];  // 하단
                case 3: return position.x + cushionDistances[3];  // 좌측
                default: return 0f;
            }
        }
        
        private BallState HandleCushionCollision(BallState ball, CushionCollision collision)
        {
            // 충돌 지점으로 위치 조정
            ball.position = collision.position;
            
            // 반사 속도 계산
            Vector3 normal = collision.normal;
            Vector3 incident = ball.velocity;
            
            // 법선 방향 속도 성분
            float normalVelocity = Vector3.Dot(incident, normal);
            
            // 반사 공식: v' = v - 2(v·n)n * restitution
            ball.velocity = incident - 2 * normalVelocity * normal * config.cushionRestitution;
            
            // 스핀 효과 (간단한 모델)
            Vector3 tangent = (incident - normalVelocity * normal).normalized;
            float spinEffect = Vector3.Dot(ball.angularVelocity, Vector3.Cross(normal, tangent));
            ball.velocity += tangent * spinEffect * 0.1f;
            
            return ball;
        }
        
        private void ApplyFriction(BallState ball, float deltaTime)
        {
            // 슬라이딩 마찰
            Vector3 frictionForce = -ball.velocity.normalized * config.friction * 9.81f; // g = 9.81
            ball.velocity += frictionForce * deltaTime;
            
            // 굴림 마찰 (속도가 낮을 때)
            if (ball.velocity.magnitude < 1.0f)
            {
                ball.velocity *= Mathf.Pow(1f - config.rollingFriction, deltaTime);
            }
        }
        
        // 스핀을 고려한 초기 속도 계산
        public Vector3 CalculateInitialVelocity(Vector3 direction, float speed, float spinZ = 0f)
        {
            Vector3 velocity = direction.normalized * speed;
            
            // 사이드 스핀 효과 (매그너스 효과 간단 모델)
            if (Mathf.Abs(spinZ) > 0.1f)
            {
                Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
                velocity += perpendicular * spinZ * 0.5f;
            }
            
            return velocity;
        }
        
        // 목표 지점까지의 필요 속도 계산
        public float CalculateRequiredSpeed(Vector3 start, Vector3 target)
        {
            float distance = Vector3.Distance(start, target);
            
            // 마찰을 고려한 필요 속도 (경험적 공식)
            float requiredSpeed = distance * config.friction * 2.0f;
            return Mathf.Clamp(requiredSpeed, 1.0f, 5.0f); // 1-5 m/s 범위
        }
        
        // 두 공 사이의 충돌 시뮬레이션
        public static void HandleBallCollision(BallState ball1, BallState ball2, float restitution = 0.9f)
        {
            Vector3 normal = (ball2.position - ball1.position).normalized;
            Vector3 relativeVelocity = ball1.velocity - ball2.velocity;
            float velocityAlongNormal = Vector3.Dot(relativeVelocity, normal);
            
            if (velocityAlongNormal > 0) return; // 공들이 이미 분리되고 있음
            
            float impulse = -(1 + restitution) * velocityAlongNormal / 2f; // 같은 질량 가정
            
            ball1.velocity += impulse * normal;
            ball2.velocity -= impulse * normal;
        }
    }
}