using UnityEngine;

namespace SnowFight
{
    public class BossAttackPattern_ThrowSnowballs : BossAttackPattern
    {
        [Header("Pattern4 - Throw Snowballs")]
        [SerializeField] private int snowballBurstCount = 5;
        [SerializeField] private float throwInterval = 0.3f;
        
        private float patternTimer;
        private int currentThrowCount;

        public override void ResetPatternState()
        {
            patternTimer = 0f;
            currentThrowCount = 0;
            IsFinished = false;
        }

        public override void UpdatePattern()
        {
            if (player == null)
            {
                IsFinished = true;
                return;
            }

            if (throwAbility == null)
            {
                IsFinished = true;
                return;
            }

            patternTimer += Time.deltaTime;

            if (currentThrowCount < snowballBurstCount)
            {
                TryThrowSnowball();
            }
            else
            {
                IsFinished = true;
            }
        }

        private void TryThrowSnowball()
        {
            if (currentThrowCount == 0)
            {
                ExecuteThrow();
                return;
            }

            if (patternTimer >= throwInterval)
            {
                ExecuteThrow();
            }
        }

        private void ExecuteThrow()
        {
            //LookAtPlayerXZ();
            bossAI.LookAtPlayerXZ();
            base.throwAbility.ThrowFromBoss();
            currentThrowCount++;
            patternTimer = 0f;
        }
    }
}