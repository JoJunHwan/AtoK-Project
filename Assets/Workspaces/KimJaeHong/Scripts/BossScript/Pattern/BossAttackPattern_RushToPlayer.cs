using UnityEngine;

namespace SnowFight
{
    public class BossAttackPattern_RushToPlayer : BossAttackPattern
    {
        [Header("Pattern3 - Rush To Player")]
        [SerializeField] private float moveTime = 2f;
        
        public override void ResetPatternState()
        {
            base.bossAI.MoveTimer = 0f;
            base.bossAI.MoveTargetPos = transform.position;
            IsFinished = false;
        }

        public override void UpdatePattern()
        {
            if (player == null)
            {
                IsFinished = true;
                return;
            }

            bossAI.UpdateMoveState();
            base.bossAI.MoveTowardsPlayer();
            LookAtPlayerXZ();

            if (base.bossAI.MoveTimer >= moveTime)
            {
                IsFinished = true;
            }
        }
    }
}