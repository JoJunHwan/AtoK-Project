using UnityEngine;

namespace SnowFight
{
    public abstract class BossAttackPattern : MonoBehaviour
    {
        [Header("공통 이동/위치 설정")]
        //하위 패턴에서 사용되는 것들 (Transform과 관련됨)
        [SerializeField] protected float speed = 10f;
        [SerializeField] protected float groundY = 0f;
        [SerializeField] protected float gravity = 9.8f;
        [SerializeField] protected float moveTimer;
        protected Vector3 moveTargetPos;

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

        protected void SnapToGround()
        {
            Vector3 pos = transform.position;
            transform.position = new Vector3(pos.x, groundY, pos.z);
        }

#region Migrate to BossAI
        // BossCharacter transform 간섭
        protected void MoveUpwards()
        {
            //transform.position += Vector3.up * speed * Time.deltaTime;
            bossAI.MoveUpwards();
        }
        
        protected void MoveDownwards()
        {
            //transform.position -= Vector3.up * gravity * Time.deltaTime;
            bossAI.MoveDownwards();
        }
        
        protected void MoveTowardsPlayer()
        {
            transform.position =
                Vector3.MoveTowards(transform.position, moveTargetPos,
                    speed * Time.deltaTime);
        }
        
        protected void MoveHorizontallyToPlayer()
        {
            Vector3 target =
                new Vector3(player.position.x, transform.position.y, player.position.z);

            transform.position =
                Vector3.MoveTowards(transform.position, target,
                    speed * 3f * Time.deltaTime);
        }
        
        protected void UpdateMoveState()
        {
            moveTargetPos =
                new Vector3(player.position.x, transform.position.y, player.position.z);

            moveTimer += Time.deltaTime;
        }
#endregion
        
    }
}