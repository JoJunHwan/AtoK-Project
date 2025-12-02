using UnityEngine;

namespace SnowFight
{
    public class BossAttackPattern_RushToPlayer : BossAttackPattern
    {
        [Header("Pattern3 - Rush To Player")]
        [SerializeField] private float moveTime = 2f;

        private float moveTimer;
        private Vector3 moveTargetPos;

        public override void ResetPatternState()
        {
            moveTimer = 0f;
            moveTargetPos = transform.position;
            IsFinished = false;
        }

        public override void UpdatePattern()
        {
            if (player == null)
            {
                IsFinished = true;
                return;
            }

            UpdateMoveState();
            MoveTowardsPlayer();
            LookAtPlayerXZ();

            if (moveTimer >= moveTime)
            {
                IsFinished = true;
            }
        }

        private void UpdateMoveState()
        {
            moveTargetPos =
                new Vector3(player.position.x, transform.position.y, player.position.z);

            moveTimer += Time.deltaTime;
        }

        // BossCharacter transform 간섭
        private void MoveTowardsPlayer()
        {
            transform.position =
                Vector3.MoveTowards(transform.position, moveTargetPos,
                    speed * Time.deltaTime);
        }
    }
}