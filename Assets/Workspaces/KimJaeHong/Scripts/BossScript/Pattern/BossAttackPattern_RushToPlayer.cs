using UnityEngine;

namespace SnowFight
{
    public class BossAttackPattern_RushToPlayer : BossAttackPattern
    {
        [Header("Pattern3 - Rush To Player")]
        [SerializeField] private float moveTime = 2f;
        bool isExecuted = false;
        
        public override void ResetPatternState()
        {
            //차라리 Movetime은 BossAI에서 계산하고, 이거는 몇초로 할지만 정하는 게 나을 듯..?
            base.bossAI.ResetMoveTimer();
            base.bossAI.ResetMoveTargetPos();
            IsFinished = false;
        }

        public override void UpdatePattern()
        {
            if (player == null)
            {
                IsFinished = true;
                return;
            }

            IsFinished = bossAI.Update_RushToPlayer(moveTime);
        }

        public void Execute()
        {
            isExecuted = true;
        }
    }
}