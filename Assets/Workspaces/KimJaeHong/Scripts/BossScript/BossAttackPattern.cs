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
    }
}