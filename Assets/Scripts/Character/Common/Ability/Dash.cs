using System;
using UnityEngine;

namespace SnowFight
{
    public class Dash : Ability
    {
        [Header("Dash Settings")]
        [SerializeField] private float dashDurationSeconds = 0.2f;
        [SerializeField] private float dashDistanceUnits = 5f;
        [SerializeField] private float dashCooldownSeconds = 1.0f;

        private CharacterController cachedController;
        private Move cachedMoveAbility;

        private bool isDashing;
        private float dashEndTime;
        private float dashSpeedPerSec;
        private float dashNextAvailableTime;
        private Vector3 dashDirection;

        [Header("Dash Particle")]
        [SerializeField] private ParticleSystem dashTrailParticle;
        [SerializeField] private ParticleController particleController;
        
        
        // 추가: UI용
        public event Action OnDashStarted;
        public event Action OnDashCooldownFinished;
        private bool cooldownFinishEventFired;
        private float cachedCooldownRatio;

        public override void Init()
        {
            base.Init();
            CacheComponentsOnce();
            PrecomputeSpeed();
            cachedCooldownRatio = 1f;
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
            UpdateCooldownProgress();
        }

        public override void TryExecute()
        {
            if (CanExecute() == false) return;
            BeginDash(GetDesiredDirection());
        }

        public override bool CanExecute()
        {
            if (isDashing) return false;
            if (Time.time < dashNextAvailableTime) return false;
            if (cachedController == null) return false;
            if (dashDurationSeconds <= 0f) return false;
            if (dashDistanceUnits <= 0f) return false;
            return true;
        }

        public override void Execute()
        {
            if (isDashing == false) return;
            if (Time.time >= dashEndTime)
            {
                EndDash();
                return;
            }
            StepDash();
        }

        public void TriggerDash(Vector3 desiredDirection)
        {
            if (CanExecute() == false) return;
            if (desiredDirection.sqrMagnitude <= 0f) desiredDirection = transform.forward;
            desiredDirection = NormalizeOnXZ(desiredDirection);
            BeginDash(desiredDirection);
        }

        public float GetCooldownRatio()
        {
            return cachedCooldownRatio;
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
            dashNextAvailableTime = dashEndTime + dashCooldownSeconds;
            cooldownFinishEventFired = false;
            cachedCooldownRatio = 0f;
            
            PlayDashParticle(); //대시 파티클 play
            
            if (OnDashStarted != null) OnDashStarted.Invoke();
        }

        private void StepDash()
        {
            Vector3 velocity = dashDirection * dashSpeedPerSec;
            cachedController.Move(velocity * Time.deltaTime);
        }

        private void EndDash()
        {
            isDashing = false;
            cachedCooldownRatio = 0f;
            
            StopDashParticle(); //파티클 재생 중지
        }

        private void UpdateCooldownProgress()
        {
            if (isDashing == true) return;

            if (Time.time < dashEndTime)
            {
                cachedCooldownRatio = 0f;
                return;
            }

            if (Time.time >= dashNextAvailableTime)
            {
                cachedCooldownRatio = 1f;
                TryFireCooldownFinished();
                return;
            }

            float elapsed = Time.time - dashEndTime;
            float ratio = 0f;
            if (dashCooldownSeconds > 0f)
            {
                ratio = elapsed / dashCooldownSeconds;
            }

            if (ratio < 0f)
            {
                ratio = 0f;
            }
            else if (ratio > 1f)
            {
                ratio = 1f;
            }

            cachedCooldownRatio = ratio;
            if (cachedCooldownRatio >= 1f)
            {
                TryFireCooldownFinished();
            }
        }

        private void TryFireCooldownFinished()
        {
            if (cooldownFinishEventFired == true) return;
            cooldownFinishEventFired = true;
            if (OnDashCooldownFinished != null) OnDashCooldownFinished.Invoke();
        }

        private Vector3 NormalizeOnXZ(Vector3 v)
        {
            v.y = 0f;
            if (v.sqrMagnitude > 0f) return v.normalized;
            return Vector3.zero;
        }


        private void PlayDashParticle()
        {
            if (particleController != null)
            {
                particleController.PlayTrail(dashTrailParticle); 
            }
        }

        private void StopDashParticle()
        {
            if (particleController != null)
            {
                particleController.StopTrail();
            }
        }
    }
    
    
}
