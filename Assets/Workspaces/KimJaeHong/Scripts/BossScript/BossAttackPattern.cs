using UnityEngine;

namespace SnowFight
{
    public abstract class BossAttackPattern : MonoBehaviour
    {
        [Header("DI - By BossAI")]
        protected BossAI bossAI;
        protected Transform player;
        protected ThrowSnowball_Boss throwAbility;
        
        [Header("패턴 활성화")]
        public bool isEnabled = true;

        public bool IsFinished { get; protected set; }

        public virtual void InitializePattern(BossAI _bossAI ,Transform _playerTransform,ThrowSnowball_Boss _bossThrowAbility)
        {
            this.bossAI =  _bossAI;
            this.player = _playerTransform;
            this.throwAbility = _bossThrowAbility;
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

        /// <summary>
        /// 바닥과 닿게 하는 함수
        /// </summary>
        protected void SnapToGround()
        {
            transform.position = new Vector3(transform.position.x, 
                                            bossAI.GroundY, 
                                            transform.position.z);
        }
    }
}