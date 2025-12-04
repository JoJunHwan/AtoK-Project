using UnityEngine;

namespace SnowFight
{
    public class BossAI : AIController
    {
        [Header("Player, Ability")]
        //public Transform player;//공통O
        private ThrowSnowball_Boss throwAbility;//공통O

        [Header("Attack Pattern")]
        public BossAttackPattern[] patterns;
        public float pauseDuration = 1f;
        private int currentPatternIndex;
        private float pauseTimer;
        
        [Header("Movement setting")]
        [SerializeField] private float speed = 20f;
        [SerializeField] private float gravity = 9.8f;
        public float GroundY { get; } = 0f;
        public float MoveTimer { get; private set; }
        public Vector3 MoveTargetPos { get; private set; }
        public bool IsAscending { get; private set; }
        public bool IsFalling { get; private set; }

        public override void AwakeEntity()
        {
            base.EnsureCharacter();
            base.EnsureMoveAbility();
            EnsureThrowAbility_Boss();
            character.AwakeByCharacterEntityController();
            
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        public override void StartEntity()
        {
            InitThrowAbility();
            InitPatterns();
        }

        void InitThrowAbility()
        {
            throwAbility = GetComponentInChildren<ThrowSnowball_Boss>();
            Debug.Assert(throwAbility != null,
                "**ThrowSnowball_Boss** 컴포넌트가 필요합니다.");
        }

        void InitPatterns()
        {
            if (patterns == null) return;

            for (int i = 0; i < patterns.Length; i++)
            {
                BossAttackPattern pattern = patterns[i];
                if (pattern == null) continue;
                pattern.InitializePattern(this, player, throwAbility);
            }

            currentPatternIndex = 0;
            StartCurrentPattern();
        }

        public override void UpdateEntity()
        {
            if (patterns == null) return;
            if (patterns.Length == 0) return;

            if (UpdatePause()) return;

            BossAttackPattern currentPattern = GetCurrentPattern();
            if (currentPattern == null) return;
            if (currentPattern.isEnabled == false) return;

            currentPattern.UpdatePattern();

            if (currentPattern.IsFinished)
            {
                StartPauseAndSelectNext();
            }
        }

        bool UpdatePause()
        {
            if (pauseTimer <= 0f) return false;

            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f)
            {
                StartCurrentPattern();
            }

            return pauseTimer > 0f;
        }
        
        // 이 밑으로는 병합을 위해 추가된 코드들
        private void EnsureThrowAbility_Boss()
        {
            if (throwAbility == null) throwAbility = GetComponent<ThrowSnowball_Boss>();
        }

#region Control Pattern
        BossAttackPattern GetCurrentPattern()
        {
            if (patterns == null) return null;
            if (patterns.Length == 0) return null;

            if (currentPatternIndex < 0) return null;
            if (currentPatternIndex >= patterns.Length) return null;

            return patterns[currentPatternIndex];
        }

        void StartCurrentPattern()
        {
            BossAttackPattern current = GetCurrentPattern();
            if (current == null) return;
            if (current.isEnabled == false) return;

            current.ResetPatternState();
        }

        void StartPauseAndSelectNext()
        {
            SelectNextPatternIndex();
            pauseTimer = pauseDuration;
        }

        void SelectNextPatternIndex()
        {
            if (patterns == null) return;
            if (patterns.Length == 0) return;

            int length = patterns.Length;
            int index = currentPatternIndex;

            for (int i = 0; i < length; i++)
            {
                index++;
                if (index >= length)
                {
                    index = 0;
                }

                BossAttackPattern candidate = patterns[index];
                if (candidate == null) continue;
                if (candidate.isEnabled == false) continue;

                currentPatternIndex = index;
                return;
            }
        }
#endregion

#region Migrate from BossAI
    // BossCharacter transform 간섭
        //Call By Update
        private void MoveUpwards()
        {
            transform.position += Vector3.up * speed * Time.deltaTime;
        }
        
        private void MoveDownwards()
        {
            transform.position -= Vector3.up * gravity * Time.deltaTime;
        }
        
        private void MoveTowardsPlayer()
        {
            transform.position =
                Vector3.MoveTowards(transform.position, MoveTargetPos,
                    speed * Time.deltaTime);
        }
        
        private void MoveHorizontallyToPlayer()
        {
            Vector3 target =
                new Vector3(player.position.x, transform.position.y, player.position.z);

            transform.position =
                Vector3.MoveTowards(transform.position, target,
                    speed * 3f * Time.deltaTime);
        }
        
        //이거 Private이 되어야 함
        public void ResetMoveTimer()
        {
            MoveTimer = 0f; 
        }
        
        public void ResetJumpAndFall()
        {
            IsAscending = true;
            IsFalling = false;
        }

        public void ResetMoveTargetPos()
        {
            MoveTargetPos = transform.position;
        }
        
        public void LookAtPlayerXZ()
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
        public void SnapToGround()
        {
            transform.position = new Vector3(transform.position.x, 
                GroundY, 
                transform.position.z);
        }
        
        private void Update_MoveTimer()
        {
            MoveTargetPos =
                new Vector3(player.position.x, transform.position.y, player.position.z);

            MoveTimer += Time.deltaTime;
        }

        //이것도 가능한 Private이 되도록..?
        public bool Update_RushToPlayer(float moveTime)
        {
            bool IsFinished = false;

            //이것들은 여기서 분리가 되어서.
            // Private으로 움직여져야 함
            Update_MoveTimer();
            MoveTowardsPlayer();
            LookAtPlayerXZ();

            if (MoveTimer >= moveTime)
            {
                IsFinished = true;
            }

            return IsFinished;
        }
        
        public void Update_Ascending(float _riseHeight)
        {
            MoveUpwards();
            if (IsBelowTargetHeight(_riseHeight)) return;

            MoveHorizontallyToPlayer();
            if (HasReachedPlayerXZ() == false) return;

            IsAscending = false;
            IsFalling = true;
        }
        
        public bool Update_Falling()
        {
            bool IsFinished;
            
            MoveDownwards();
            if (IsAboveGround()) return false;

            SnapToGround();
            IsFinished = true;
            
            return IsFinished;
        }
        
        // transform 읽기만
        public bool IsBelowTargetHeight(float riseHeight)
        {
            float targetY = GroundY + riseHeight;
            return transform.position.y < targetY;
        }
        
        // transform 읽기만
        public bool HasReachedPlayerXZ()
        {
            Vector3 currentXZ =
                new Vector3(transform.position.x, 0f, transform.position.z);
            Vector3 playerXZ =
                new Vector3(player.position.x, 0f, player.position.z);

            float distance = Vector3.Distance(currentXZ, playerXZ);
            return distance < 0.01f;
        }
        
        public bool IsAboveGround()
        {
            return transform.position.y > GroundY;
        }
#endregion
        
    }
}
