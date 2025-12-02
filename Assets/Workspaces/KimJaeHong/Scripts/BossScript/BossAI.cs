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
        [SerializeField] public float speed = 20f;
        public float GroundY { get; private set; } = 0f;
        [SerializeField] private float gravity = 9.8f;
        public float MoveTimer { get; set; }
        public Vector3 MoveTargetPos { get; set; }

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
        public void MoveUpwards()
        {
            transform.position += Vector3.up * speed * Time.deltaTime;
        }
        
        public void MoveDownwards()
        {
            transform.position -= Vector3.up * gravity * Time.deltaTime;
        }
        
        public void MoveTowardsPlayer()
        {
            transform.position =
                Vector3.MoveTowards(transform.position, MoveTargetPos,
                    speed * Time.deltaTime);
        }
        
        public void MoveHorizontallyToPlayer()
        {
            Vector3 target =
                new Vector3(player.position.x, transform.position.y, player.position.z);

            transform.position =
                Vector3.MoveTowards(transform.position, target,
                    speed * 3f * Time.deltaTime);
        }
        
        public void UpdateMoveState()
        {
            MoveTargetPos =
                new Vector3(player.position.x, transform.position.y, player.position.z);

            MoveTimer += Time.deltaTime;
        }
#endregion
        
    }
}
