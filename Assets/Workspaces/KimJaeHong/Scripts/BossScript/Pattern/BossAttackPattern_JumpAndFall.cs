using UnityEngine;

namespace SnowFight
{
    public class BossAttackPattern_JumpAndFall : BossAttackPattern
    {
        [Header("Pattern1 - Jump And Fall")]
        [SerializeField] private float riseHeight = 10f;
        [SerializeField] private float gravity = 9.8f;

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
            MoveUpwards();
            if (IsBelowTargetHeight()) return;

            MoveHorizontallyToPlayer();
            if (HasReachedPlayerXZ() == false) return;

            isAscending = false;
            isFalling = true;
        }

        private void UpdateFalling()
        {
            MoveDownwards();
            if (IsAboveGround()) return;

            SnapToGround();
            IsFinished = true;
        }

        // BossCharacter transform 간섭
        private void MoveUpwards()
        {
            transform.position += Vector3.up * speed * Time.deltaTime;
        }

        // BossCharacter transform 간섭
        private void MoveDownwards()
        {
            transform.position -= Vector3.up * gravity * Time.deltaTime;
        }

        private bool IsBelowTargetHeight()
        {
            float targetY = groundY + riseHeight;
            return transform.position.y < targetY;
        }

        // BossCharacter transform 간섭
        private void MoveHorizontallyToPlayer()
        {
            Vector3 target =
                new Vector3(player.position.x, transform.position.y, player.position.z);

            transform.position =
                Vector3.MoveTowards(transform.position, target,
                    speed * 3f * Time.deltaTime);
        }

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
