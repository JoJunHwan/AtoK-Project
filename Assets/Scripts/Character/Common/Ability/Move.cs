using UnityEngine;

namespace SnowFight
{
    /// <summary>
    /// 이동 + 넉백 합성(캐릭컨 기반)
    /// </summary>
    public class Move : Ability
    {
        [Header("Current Input")]
        [SerializeField] protected float curMoveX = 0f;
        [SerializeField] protected float curMoveZ = 0f;

        [Header("Physics")]
        [SerializeField] private float gravity = -9.81f;

        [Header("Movement")]
        public float moveSpeed = 5f;
        public Vector3 moveDirection;

        [Header("Knockback")]
        [SerializeField] private float kbDamping = 12f;     // 감쇠 속도
        [SerializeField] private float kbMinSpeed = 0.1f;   // 멈춤 임계값
        [SerializeField] private float kbUpGravityScale = 1f;
        [SerializeField] private float rotateStopTime = 0.1f;
        private Vector3 knockbackVel;
        private float knockbackEndTime;
        private float rotateResumeTime;

        public override void HandleInput()
        {
            curMoveX = 0f; curMoveZ = 0f;
            if (ownerCharacter.inputState_MoveLeft == InputState.Held)  curMoveX -= 1f;
            if (ownerCharacter.inputState_MoveRight == InputState.Held) curMoveX += 1f;
            if (ownerCharacter.inputState_MoveUp == InputState.Held)    curMoveZ += 1f;
            if (ownerCharacter.inputState_MoveDown == InputState.Held)  curMoveZ -= 1f;
        }

        public override void Tick()
        {
            UpdateMovement(curMoveX, curMoveZ);
        }

        private void UpdateMovement(float x, float z)
        {
            try
            {
                Vector3 world = GetWorldMoveDirection(x, z);
                ApplyMoveAbility(world);
                RotateTowards(world);
            }
            catch (System.Exception e)
            {
                Debug.LogError("UpdateMovement 오류 발생! GameObject: " + gameObject.name);
                Debug.LogException(e);
            }
        }

        private void ApplyMoveAbility(Vector3 world)
        {
            moveDirection.x = world.x;
            moveDirection.z = world.z;
            this.Execute();
        }

        public override void Execute()
        {
            if (ownerCharacter == null) return;
            Vector3 horizontal = new Vector3(moveDirection.x, 0f, moveDirection.z);
            if (horizontal.sqrMagnitude > 1f) horizontal = horizontal.normalized;
            UpdateKnockback(); // 넉백 갱신(감쇠/중력/종료)
            MoveCharacter(horizontal);
        }

        /// <summary>이동 실제 수행(입력 + 넉백 + 중력)</summary>
        public void MoveCharacter(Vector3 worldDir)
        {
            ApplyGravity();
            Vector3 velocity = worldDir * moveSpeed;
            velocity += knockbackVel;
            velocity.y = moveDirection.y + knockbackVel.y;
            ownerCharacter.characterController.Move(velocity * Time.deltaTime);
        }

        private void RotateTowards(Vector3 world)
        {
            if (Time.time < rotateResumeTime) return;
            if (world.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(world, Vector3.up),
                    20f * Time.deltaTime
                );
            }
        }

        // 카메라 기준 방향 변환
        private Vector3 GetWorldMoveDirection(float x, float z)
        {
            Vector3 f = GetCameraForward();
            Vector3 r = GetCameraRight();
            return f * z + r * x;
        }

        private Vector3 GetCameraForward()
        {
            if (Camera.main == null) return Vector3.forward;
            Vector3 f = Camera.main.transform.forward; f.y = 0f;
            if (f.sqrMagnitude > 0f) return f.normalized;
            return Vector3.forward;
        }

        private Vector3 GetCameraRight()
        {
            if (Camera.main == null) return Vector3.right;
            Vector3 r = Camera.main.transform.right; r.y = 0f;
            if (r.sqrMagnitude > 0f) return r.normalized;
            return Vector3.right;
        }

        protected void SetCurrentMove(float _curMoveX, float _curMoveZ)
        {
            curMoveX = _curMoveX;
            curMoveZ = _curMoveZ;
        }

        #region Physics
        private void ApplyGravity()
        {
            if (ownerCharacter.characterController.isGrounded)
            {
                if (moveDirection.y < 0f) moveDirection.y = 0f;
                if (knockbackVel.y < 0f) knockbackVel.y = 0f;
            }
            else
            {
                moveDirection.y += gravity * Time.deltaTime;
                knockbackVel.y += gravity * kbUpGravityScale * Time.deltaTime;
            }
        }
        #endregion

        #region Knockback
        /// <summary>
        /// 넉백 적용. dir은 월드 기준(정규화 안되어도 됨)
        /// </summary>
        public void ApplyKnockback(Vector3 dir, float power, float duration, float upwardBoost = 0f)
        {
            Vector3 n = dir;
            if (n.sqrMagnitude > 0f) n = n.normalized;
            knockbackVel = n * power;
            knockbackVel.y += upwardBoost;
            knockbackEndTime = Time.time + duration;
            rotateResumeTime = Time.time + rotateStopTime;
        }

        /// <summary>저항 시 호출: 아무 것도 하지 않음</summary>
        public void TryApplyKnockback(Vector3 dir, float power, float duration, float upwardBoost, bool resisted)
        {
            if (resisted) return; // 저항 중이면 무시
            ApplyKnockback(dir, power, duration, upwardBoost); // 넉백 적용
        }

        private void UpdateKnockback()
        {
            if (!IsKnockbackActive()) { DampenToStop(); return; } // 넉백이 끝났으면 감속 후 정지
            DampenHorizontal(); // 수평 방향 감속
            ClampTinyToZero();  // 매우 작은 값은 0으로 클램프
        }

        private bool IsKnockbackActive()
        {
            if (Time.time <= knockbackEndTime) return true; // 지속 시간 내면 활성 상태
            return knockbackVel.sqrMagnitude > kbMinSpeed * kbMinSpeed; // 남은 속도가 일정 이상이면 계속 유지
        }

        private void DampenHorizontal()
        {
            // 수평 넉백 속도를 부드럽게 0으로 감속
            Vector3 flat = new Vector3(knockbackVel.x, 0f, knockbackVel.z);
            Vector3 to = Vector3.MoveTowards(flat, Vector3.zero, kbDamping * Time.deltaTime);
            knockbackVel.x = to.x; 
            knockbackVel.z = to.z;
        }

        private void DampenToStop()
        {
            // 남은 전체 속도가 작으면 완전 정지, 아니면 점점 감속
            float s = knockbackVel.magnitude;
            if (s <= kbMinSpeed) { knockbackVel = Vector3.zero; return; }
            Vector3 to = Vector3.MoveTowards(knockbackVel, Vector3.zero, kbDamping * Time.deltaTime);
            knockbackVel = to;
        }

        private void ClampTinyToZero()
        {
            // 부동소수점 오차 방지: 거의 0에 가까운 속도는 0으로 처리
            if (Mathf.Abs(knockbackVel.x) < 0.0001f) knockbackVel.x = 0f;
            if (Mathf.Abs(knockbackVel.z) < 0.0001f) knockbackVel.z = 0f;
        }

        #endregion
    }
}
