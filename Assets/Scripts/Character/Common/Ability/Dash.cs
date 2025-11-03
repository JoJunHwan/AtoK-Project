using UnityEngine;

namespace SnowFight
{
    /// <summary>
    /// n초 동안 지정 거리만큼 전/측/입력 방향으로 단순 대시
    /// - 인스펙터: dashDurationSeconds, dashDistanceUnits, dashCooldownSeconds
    /// - 입력: ownerCharacter.inputState_Dash == InputState.Pressed
    /// - 외부 트리거: TriggerDash(direction)
    /// </summary>
    public class Dash : Ability
    {
        [Header("Dash Settings")]
        [SerializeField] private float dashDurationSeconds = 0.2f;
        [SerializeField] private float dashDistanceUnits = 5f;
        [SerializeField] private float dashCooldownSeconds = 1.0f; // 추가: 쿨타임

        private CharacterController cachedController;
        private Move cachedMoveAbility;

        private bool isDashing;
        private float dashEndTime;
        private float dashSpeedPerSec;
        private float dashNextAvailableTime; // 쿨타임 끝나는 시점
        private Vector3 dashDirection;

        // ------------ Lifecycle ------------

        public override void Init()
        {
            base.Init();
            CacheComponentsOnce();
            PrecomputeSpeed();
        }

        public override void HandleInput()
        {
            if (ownerCharacter == null) return;
            if (ownerCharacter.inputState_Dash != InputState.Pressed) return;
            TryExecute();
        }

        public override void Tick()
        {
            Execute();
        }

        public override void TryExecute()
        {
            if (CanExecute() == false) return;
            BeginDash(GetDesiredDirection());
        }

        public override bool CanExecute()
        {
            if (isDashing) return false;
            if (Time.time < dashNextAvailableTime) return false; // 쿨타임 확인
            if (cachedController == null) return false;
            if (dashDurationSeconds <= 0f) return false;
            if (dashDistanceUnits <= 0f) return false;
            return true;
        }

        public override void Execute()
        {
            if (isDashing == false) return;
            if (Time.time >= dashEndTime) { EndDash(); return; }
            StepDash();
        }

        // ------------ Public API ------------

        public void TriggerDash(Vector3 desiredDirection)
        {
            if (CanExecute() == false) return;
            if (desiredDirection.sqrMagnitude <= 0f) desiredDirection = transform.forward;
            desiredDirection = NormalizeOnXZ(desiredDirection);
            BeginDash(desiredDirection);
        }

        // ------------ Core ------------

        private void CacheComponentsOnce()
        {
            cachedController = GetComponent<CharacterController>();
            cachedMoveAbility = GetComponent<Move>();
            if (cachedController == null)
            {
                Debug.LogError("Dash: CharacterController가 필요합니다.");
            }
        }

        private void PrecomputeSpeed()
        {
            if (dashDurationSeconds <= 0f) return;
            dashSpeedPerSec = dashDistanceUnits / dashDurationSeconds;
        }

        private Vector3 GetDesiredDirection()
        {
            Vector3 moveDir = GetMoveAbilityDirection();
            if (moveDir.sqrMagnitude > 0f) return moveDir;
            return transform.forward;
        }

        private Vector3 GetMoveAbilityDirection()
        {
            if (cachedMoveAbility == null) return Vector3.zero;
            Vector3 horizontal = new Vector3(cachedMoveAbility.moveDirection.x, 0f, cachedMoveAbility.moveDirection.z);
            if (horizontal.sqrMagnitude > 1f) horizontal = horizontal.normalized;
            return horizontal;
        }

        private void BeginDash(Vector3 desiredDirection)
        {
            dashDirection = NormalizeOnXZ(desiredDirection);
            isDashing = true;
            dashEndTime = Time.time + dashDurationSeconds;
            dashNextAvailableTime = dashEndTime + dashCooldownSeconds; // 쿨타임 시작
        }

        private void StepDash()
        {
            Vector3 velocity = dashDirection * dashSpeedPerSec;
            cachedController.Move(velocity * Time.deltaTime);
        }

        private void EndDash()
        {
            isDashing = false;
        }

        // ------------ Utils ------------

        private Vector3 NormalizeOnXZ(Vector3 v)
        {
            v.y = 0f;
            if (v.sqrMagnitude > 0f) return v.normalized;
            return Vector3.zero;
        }
    }
}
