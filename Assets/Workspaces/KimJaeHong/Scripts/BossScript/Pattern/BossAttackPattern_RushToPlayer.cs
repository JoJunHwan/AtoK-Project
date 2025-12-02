using UnityEngine;

namespace SnowFight
{
    public class BossAttackPattern_RushToPlayer : BossAttackPattern
    {
        [Header("Pattern3 - Rush To Player")]
        [SerializeField] private float moveTime = 2f;
        
        public override void ResetPatternState()
        {
            base.moveTimer = 0f;
            base.moveTargetPos = transform.position;
            IsFinished = false;
        }

        public override void UpdatePattern()
        {
            if (player == null)
            {
                IsFinished = true;
                return;
            }

            base.UpdateMoveState();
            base.MoveTowardsPlayer();
            LookAtPlayerXZ();

            if (base.moveTimer >= moveTime)
            {
                IsFinished = true;
            }
        }
    }
}