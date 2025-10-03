using UnityEngine;

namespace SnowFight
{
    /// <summary>
    /// 가장 처음으로 행동을 결정. 이후 그 동작 값을 Character에게 전달
    /// 순찰-추적-공격을 수행하는 적 AI
    /// (지금은 각 상태들을 함수로 처리하나, 나중에는 클래스로 변경하기)
    /// </summary>
    [RequireComponent(typeof(Character))]
    public class EnemyAI : MonoBehaviour
    {
        public enum AIState
        {
            Patrol,
            Chase,
            Attack
        }

        [Header("References")]
        public Transform player;                            // 추적/공격 대상
        public Character character;                         // 이 적의 캐릭터 컴포넌트
        public Move moveAbility;                             // 이동 능력(캐릭터에도 있지만 캐시)
        public ThrowSnowball_Enemy throwAbility;                   // 투척 능력(방향 지정 발사 사용)

        [Header("Patrol")]
        public Transform[] waypoints;                       // 순찰 웨이포인트
        public float waypointReachRadius = 0.5f;            // 웨이포인트 도달 판정 반경
        public float patrolSpeed = 3.5f;                    // 순찰 속도
        public bool loopPatrol = true;                      // 순찰 루프 여부

        [Header("Chase")]
        public float detectionRadius = 12f;                 // 플레이어 감지 반경
        public float loseSightRadius = 16f;                 // 시야 상실 반경(이 범위 밖이면 순찰 복귀)
        public float chaseSpeed = 5.5f;                     // 추적 속도
        public float faceTurnSpeed = 12f;                   // 회전 속도

        [Header("Attack")]
        public float attackRange = 10f;                     // 공격 사거리(수평 기준 권장)
        public float attackCooldown = 1.2f;                 // 공격 쿨다운
        public bool useCurve = false;                       // 커브볼 사용 여부
        public Transform aimPivot;                          // 조준 기준 위치(없으면 적 본체), muzzle가 ThrowSnowball에 있으니 여기서는 방향계산만

        [Header("Debug")]
        public AIState currentState = AIState.Patrol;       // 현재 상태 디버그
        private int currentWaypointIndex = 0;               // 현재 웨이포인트 인덱스
        private float cooldownTimer = 0f;                   // 공격 쿨다운 타이머

        // 컴포넌트 초기화
        private void Awake()
        {
            if (character == null)
            {
                character = GetComponent<Character>();
            }
            if (moveAbility == null)
            {
                moveAbility = GetComponent<Move>();
            }
            if (throwAbility == null)
            {
                throwAbility = GetComponent<ThrowSnowball_Enemy>();
            }
        }

        // 매 프레임 상태 업데이트
        private void Update()
        {
            UpdateCooldown();
            UpdateState();
            TickState();
        }

        // 쿨다운 감소를 수행
        private void UpdateCooldown()
        {
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
                if (cooldownTimer < 0f)
                {
                    cooldownTimer = 0f;
                }
            }
        }

        // 상태 전이를 수행
        private void UpdateState()
        {
            if (player == null)
            {
                currentState = AIState.Patrol;
                return;
            }

            float dist = PlanarDistance(transform.position, player.position);
            if (currentState == AIState.Patrol)
            {
                if (dist <= detectionRadius)
                {
                    currentState = AIState.Chase;
                    return;
                }
            }
            else if (currentState == AIState.Chase)
            {
                if (dist <= attackRange)
                {
                    currentState = AIState.Attack;
                    return;
                }
                if (dist > loseSightRadius)
                {
                    currentState = AIState.Patrol;
                    return;
                }
            }
            else if (currentState == AIState.Attack)
            {
                if (dist > attackRange)
                {
                    currentState = AIState.Chase;
                    return;
                }
                if (dist > loseSightRadius)
                {
                    currentState = AIState.Patrol;
                    return;
                }
            }
        }

        // 상태별 로직을 수행
        private void TickState()
        {
            if (currentState == AIState.Patrol)
            {
                AI_Patrol();
                return;
            }
            if (currentState == AIState.Chase)
            {
                AI_Chase();
                return;
            }
            if (currentState == AIState.Attack)
            {
                AI_Attack();
                return;
            }
        }

        // 순찰 상태를 수행
        private void AI_Patrol()
        {
            if (waypoints == null)
            {
                return;
            }
            if (waypoints.Length == 0)
            {
                return;
            }

            Transform target = waypoints[currentWaypointIndex];
            if (target == null)
            {
                AdvanceWaypoint();
                return;
            }

            Vector3 dir = PlanarDirection(transform.position, target.position);
            //character.moveSpeed = patrolSpeed;
            //character.moveDirection = dir;

            if (moveAbility != null)
            {
                character.ExecuteAbility(moveAbility);
            }

            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, faceTurnSpeed * Time.deltaTime);
            }

            float d = PlanarDistance(transform.position, target.position);
            if (d <= waypointReachRadius)
            {
                AdvanceWaypoint();
            }
        }

        // 추적 상태를 수행
        private void AI_Chase()
        {
            if (player == null)
            {
                return;
            }

            Vector3 dir = PlanarDirection(transform.position, player.position);
            //character.moveSpeed = chaseSpeed;
            //character.moveDirection = dir;

            if (moveAbility != null)
            {
                character.ExecuteAbility(moveAbility);
            }

            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, faceTurnSpeed * Time.deltaTime);
            }
        }

        // 공격 상태를 수행
        private void AI_Attack()
        {
            if (player == null)
            {
                return;
            }
            if (throwAbility == null)
            {
                return;
            }
            if (cooldownTimer > 0f)
            {
                FaceTargetSoft(player.position);
                return;
            }

            Vector3 origin = transform.position;
            if (aimPivot != null)
            {
                origin = aimPivot.position;
            }

            Vector3 toTarget = player.position - origin;
            if (toTarget.sqrMagnitude <= 0.000001f)
            {
                toTarget = transform.forward;
            }

            Vector3 dir = toTarget.normalized;

            if (useCurve)
            {
                character.SelectCurve();
            }
            else
            {
                character.SelectStraight();
            }

            bool fired = throwAbility.TryFireInDirection(dir);
            if (fired)
            {
                cooldownTimer = attackCooldown;
            }

            FaceTargetSoft(player.position);
        }

        // 웨이포인트 인덱스를 진행
        private void AdvanceWaypoint()
        {
            int count = waypoints.Length;
            int next = currentWaypointIndex + 1;
            if (next >= count)
            {
                if (loopPatrol)
                {
                    currentWaypointIndex = 0;
                    return;
                }
                currentWaypointIndex = count - 1;
                return;
            }
            currentWaypointIndex = next;
        }

        // 대상 방향으로 천천히 회전
        private void FaceTargetSoft(Vector3 worldPos)
        {
            Vector3 dir = PlanarDirection(transform.position, worldPos);
            if (dir.sqrMagnitude <= 0.0001f)
            {
                return;
            }
            Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, faceTurnSpeed * Time.deltaTime);
        }

        // 수평( XZ ) 방향 벡터를 계산
        private Vector3 PlanarDirection(Vector3 from, Vector3 to)
        {
            Vector3 delta = to - from;
            delta.y = 0f;
            float m = delta.magnitude;
            if (m > 0f)
            {
                delta = delta / m;
            }
            return delta;
        }

        // 수평( XZ ) 거리 계산
        private float PlanarDistance(Vector3 a, Vector3 b)
        {
            Vector3 da = a;
            Vector3 db = b;
            da.y = 0f;
            db.y = 0f;
            float d = Vector3.Distance(da, db);
            return d;
        }
        
        // === Gizmos 디버그 표시 ===
        private void OnDrawGizmosSelected()
        {
            // 순찰 범위 (웨이포인트 도달 반경)
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, waypointReachRadius);

            // 추적 감지 반경
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            // 공격 사거리
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // 시야 상실 반경
            Gizmos.color = new Color(1f, 0.5f, 0f); // 주황색
            Gizmos.DrawWireSphere(transform.position, loseSightRadius);
        }
        
        /*
        private void UpdateKeyState(ref InputState state,    )
        {
            if (Input.GetKeyDown(key))
            {
                state = InputState.Pressed;
                return;
            }

            if (Input.GetKey(key))
            {
                state = InputState.Held;
                return;
            }

            if (Input.GetKeyUp(key))
            {
                state = InputState.Released;
                return;
            }

            state = InputState.None;
        }*/
    }
}
