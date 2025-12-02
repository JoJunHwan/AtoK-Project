using UnityEngine;

namespace SnowFight
{
    public abstract class BossAttackPattern : MonoBehaviour
    {
        [Header("공통 이동/위치 설정")]
        public float speed = 10f;
        public float groundY = 0f;

        [Header("DI - By BossAI")]
        protected Transform player;
        protected ThrowSnowball_Boss throwAbility;

        [Header("패턴 활성화")]
        public bool isEnabled = true;

        public bool IsFinished { get; protected set; }

        public virtual void InitializePattern(Transform playerTransform,
            ThrowSnowball_Boss bossThrowAbility)
        {
            player = playerTransform;
            throwAbility = bossThrowAbility;
            ResetPatternState();
        }

        public abstract void ResetPatternState();
        public abstract void UpdatePattern();

        protected void LookAtPlayerXZ()
        {
            if (player == null) return;

            Vector3 lookDir = player.position - transform.position;
            lookDir.y = 0f;

            if (lookDir == Vector3.zero) return;

            transform.rotation = Quaternion.LookRotation(lookDir);
        }

        protected void SnapToGround()
        {
            Vector3 pos = transform.position;
            transform.position = new Vector3(pos.x, groundY, pos.z);
        }
    }
}