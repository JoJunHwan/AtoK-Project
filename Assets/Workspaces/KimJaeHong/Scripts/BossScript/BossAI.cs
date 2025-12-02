using UnityEngine;

namespace SnowFight
{
    public class BossAI : MonoBehaviour
    {
        [Header("플레이어/능력")]
        public Transform player;
        private ThrowSnowball_Boss throwAbility;

        [Header("공격 패턴")]
        public BossAttackPattern[] patterns;
        public float pauseDuration = 1f;

        private int currentPatternIndex;
        private float pauseTimer;

        void Start()
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
                pattern.InitializePattern(player, throwAbility);
            }

            currentPatternIndex = 0;
            StartCurrentPattern();
        }

        void Update()
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
    }
}
