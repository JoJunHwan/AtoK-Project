using UnityEngine;

namespace SnowFight
{
    [RequireComponent(typeof(Character))]
    public class AIController : CharacterEntityController
    {
        public enum AIState { Patrol, Chase, Attack }

        [Header("References")]
        public Transform player;
        public Character character;
        public Move_Enemy moveAbility;
        public ThrowSnowball throwAbility;

        [Header("Patrol")]
        public Vector3 moveToTarget;
        public bool hasWaypoints = true;
        public Transform[] waypoints;
        public float waypointReachRadius = 0.5f;
        public float patrolSpeed = 3.5f;
        public bool loopPatrol = true;

        [Header("Random Patrol")]
        public float randomPatrolRadius = 5f;
        public float randomPatrolInterval = 3f;
        private Vector3 randomTarget;
        private float randomTimer;

        [Header("Chase")]
        public float detectionRadius = 12f;
        public float loseSightRadius = 16f;
        public float chaseSpeed = 5.5f;
        public float faceTurnSpeed = 12f;

        [Header("Attack")]
        public float attackRange = 10f;
        public float attackCooldown = 1.2f;
        public bool useCurve = false;
        public Transform aimPivot;
        public Vector3 origin;
        public Vector3 dir;

        [Header("Debug")]
        public AIState currentState = AIState.Patrol;
        private int currentWaypointIndex = 0;
        private float cooldownTimer = 0f;

        public override void AwakeEntity()
        {
            EnsureCharacter();
            EnsureMoveAbility();
            EnsureThrowAbility();
            character.AwakeByCharacterEntityController();
            
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        public override void StartEntity()
        {
            character.StartByCharacterEntityController();
        }
        
        public override void UpdateEntity()
        {
            Update_QAHelper();
            
            UpdateCooldown();
            UpdateState();
            
            Tick_ExecuteState();
            
            character.UpdateByLCharacterEntityController();
        }

#region Control AIState
        protected void UpdateCooldown()
        {
            if (cooldownTimer <= 0f) return;
            cooldownTimer -= Time.deltaTime;
            
            if (cooldownTimer < 0f) cooldownTimer = 0f;
        }

        private void UpdateState()
        {
            if (player == null) { currentState = AIState.Patrol; return; }
            if (currentState == AIState.Patrol) { TryEnterChase(); return; }
            if (currentState == AIState.Chase) { TryEnterAttackOrPatrol(); return; }
            if (currentState == AIState.Attack) { TryExitAttack(); return; }
        }

        private void TryEnterChase()
        {
            float d = PlanarDistance(transform.position, player.position);
            if (d <= detectionRadius) currentState = AIState.Chase;
        }

        private void TryEnterAttackOrPatrol()
        {
            float d = PlanarDistance(transform.position, player.position);
            if (d <= attackRange) { currentState = AIState.Attack; return; }
            if (d > loseSightRadius) currentState = AIState.Patrol;
        }

        private void TryExitAttack()
        {
            float d = PlanarDistance(transform.position, player.position);
            if (d > loseSightRadius) { currentState = AIState.Patrol; return; }
            if (d > attackRange) currentState = AIState.Chase;
        }

        private void Tick_ExecuteState()
        {
            if (currentState == AIState.Patrol) { AI_Patrol(); return; }
            if (currentState == AIState.Chase) { AI_Chase(); return; }
            if (currentState == AIState.Attack) { AI_Attack(); return; }
        }
        

#endregion
        
#region Patrol
// ===== Patrol =====
        private void AI_Patrol()
        {
            if (hasWaypoints) Patrol_Waypoint();
            else Patrol_Random();
        }

        private void Patrol_Waypoint()
        {
            if (!HasWaypoints()) return;

            Transform target = CurrentWaypoint();
            if (target == null) { AdvanceWaypoint(); return; }

            MoveTowards(target.position, patrolSpeed);
            RotateTowards(target.position);

            if (Reached(target.position, waypointReachRadius))
                AdvanceWaypoint();
        }

        // Call By Update
        private void Patrol_Random()
        {
            //randomTimer마다 이동 갱신
            randomTimer -= Time.deltaTime;
            if (randomTimer <= 0f)
            {
                randomTarget = GetRandomPatrolPoint();
                randomTimer = randomPatrolInterval;
            }

            MoveTowards(randomTarget, patrolSpeed);
            RotateTowards(randomTarget);
        }

        private Vector3 GetRandomPatrolPoint()
        {
            Vector2 offset = Random.insideUnitCircle * randomPatrolRadius;
            Vector3 point = transform.position + new Vector3(offset.x, 0f, offset.y);
            return point;
        }

        private bool HasWaypoints()
        {
            if (waypoints == null) return false;
            return waypoints.Length > 0;
        }

        private Transform CurrentWaypoint()
        {
            return waypoints[currentWaypointIndex];
        }

        private bool Reached(Vector3 worldPos, float radius)
        {
            float d = PlanarDistance(transform.position, worldPos);
            return d <= radius;
        }

        private void AdvanceWaypoint()
        {
            int last = waypoints.Length - 1;
            int next = currentWaypointIndex + 1;
            if (next > last)
            {
                if (loopPatrol) currentWaypointIndex = 0;
                else currentWaypointIndex = last;
                return;
            }
            currentWaypointIndex = next;
        }
#endregion

#region Chase
// ===== Chase =====
        private void AI_Chase()
        {
            if (player == null) return;
            MoveTowards(player.position, chaseSpeed);
            RotateTowards(player.position);
        }
        

#endregion

#region Attack
// ===== Attack =====
        private void AI_Attack()
        {
            if (!CanAttackNow()) { FaceTargetSoft(player.position); return; }

            this.MoveStop();

            origin = AttackOrigin();
            dir = AttackDirection(origin);

            //SelectShotType();
            ExecuteThrow();
            cooldownTimer = attackCooldown;

            FaceTargetSoft(player.position);
        }

        private bool CanAttackNow()
        {
            if (player == null) return false;
            if (throwAbility == null) return false;
            return cooldownTimer <= 0f;
        }

        private Vector3 AttackOrigin()
        {
            if (aimPivot != null) return aimPivot.position;
            return transform.position;
        }

        private Vector3 AttackDirection(Vector3 origin)
        {
            Vector3 toTarget = player.position - origin;
            if (toTarget.sqrMagnitude <= 0.000001f) toTarget = transform.forward;
            return toTarget.normalized;
        }

        private void ExecuteThrow()
        {
            if (throwAbility != null) throwAbility.Execute();
        }
        

#endregion

#region Movement
// ===== Movement / Facing =====
        private void MoveStop()
        {
            if (moveAbility != null) moveAbility.HandleInput_AI(0, 0);
        }
        private void MoveTowards(Vector3 worldPos, float speed)
        {
            // 어디로 가야하는지 방향벡터 구하기
            moveToTarget = worldPos - this.transform.position;
            if (moveAbility != null) moveAbility.HandleInput_AI(moveToTarget.x, moveToTarget.z);
            //if (moveAbility != null) character.ExecuteAbility(moveAbility);
        }

        private void RotateTowards(Vector3 worldPos)
        {
            Vector3 dir = PlanarDirection(transform.position, worldPos);
            if (dir.sqrMagnitude <= 0.0001f) return;
            Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, faceTurnSpeed * Time.deltaTime);
        }

        private void FaceTargetSoft(Vector3 worldPos)
        {
            RotateTowards(worldPos);
        }

        // ===== Math Utils (XZ 평면) =====
        private Vector3 PlanarDirection(Vector3 from, Vector3 to)
        {
            Vector3 delta = to - from;
            delta.y = 0f;
            float m = delta.magnitude;
            if (m > 0f) delta = delta / m;
            return delta;
        }

        private float PlanarDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
        

#endregion
        
#region Gizmos
// ===== Gizmos =====
        private void OnDrawGizmosSelected()
        {
            DrawRangeGizmo(Color.green, waypointReachRadius);
            DrawRangeGizmo(Color.yellow, detectionRadius);
            DrawRangeGizmo(Color.red, attackRange);
            DrawRangeGizmo(new Color(1f, 0.5f, 0f), loseSightRadius);
        }

        private void DrawRangeGizmo(Color c, float r)
        {
            Gizmos.color = c;
            Gizmos.DrawWireSphere(transform.position, r);
        }
        

#endregion

#region Ensure Valid
        // ===== Ensure refs =====
        protected void EnsureCharacter()
        {
            if (character == null) character = GetComponent<Character>();
        }

        protected void EnsureMoveAbility()
        {
            if (moveAbility == null) moveAbility = GetComponent<Move_Enemy>();
        }

        protected void EnsureThrowAbility()
        {
            if (throwAbility == null) throwAbility = GetComponent<ThrowSnowball_Enemy>();
        }
#endregion

#region QA Helper

        protected void Update_QAHelper()
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                character.QA_KillCharacter();
            }
        }

#endregion
    }
}
