using UnityEngine;

namespace SnowFight
{
    public class BossAttackPattern_JumpAndFall : BossAttackPattern
    {
        [Header("Pattern1 - Jump And Fall")]
        [SerializeField] private float riseHeight = 10f;

        public override void ResetPatternState()
        {
            bossAI.ResetJumpAndFall();
            IsFinished = false;
        }

        public override void UpdatePattern()
        {
            if (player == null) return;

            if (bossAI.IsAscending)
            {
                bossAI.Update_Ascending(this.riseHeight);
            }
            else if (bossAI.IsFalling)
            {
                IsFinished = bossAI.Update_Falling();
            }
        }
    }
}
