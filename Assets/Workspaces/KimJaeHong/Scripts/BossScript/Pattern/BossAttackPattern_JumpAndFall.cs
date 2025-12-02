using UnityEngine;

namespace SnowFight
{
    public class BossAttackPattern_JumpAndFall : BossAttackPattern
    {
        [Header("Pattern1 - Jump And Fall")]
        [SerializeField] private float riseHeight = 10f;
        
        private bool isAscending;
        private bool isFalling;

        public override void ResetPatternState()
        {
            isAscending = true;
            isFalling = false;
            IsFinished = false;
        }

        public override void UpdatePattern()
        {
            if (player == null) return;

            if (isAscending)
            {
                UpdateAscending();
            }
            else if (isFalling)
            {
                UpdateFalling();
            }
        }

        private void UpdateAscending()
        {
            base.MoveUpwards();
            if (IsBelowTargetHeight()) return;

            base.MoveHorizontallyToPlayer();
            if (HasReachedPlayerXZ() == false) return;

            isAscending = false;
            isFalling = true;
        }

        private void UpdateFalling()
        {
            base.MoveDownwards();
            if (IsAboveGround()) return;

            SnapToGround();
            IsFinished = true;
        }

        // transform 읽기만
        protected bool IsBelowTargetHeight()
        {
            float targetY = groundY + riseHeight;
            return transform.position.y < targetY;
        }
        
        // transform 읽기만
        private bool HasReachedPlayerXZ()
        {
            Vector3 currentXZ =
                new Vector3(transform.position.x, 0f, transform.position.z);
            Vector3 playerXZ =
                new Vector3(player.position.x, 0f, player.position.z);

            float distance = Vector3.Distance(currentXZ, playerXZ);
            return distance < 0.01f;
        }

        private bool IsAboveGround()
        {
            return transform.position.y > groundY;
        }
    }
}
