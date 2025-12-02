using UnityEngine;

namespace SnowFight
{
    public class BossAI : MonoBehaviour
    {
        [Header("플레이어/능력")]
        public Transform player;                 // 플레이어 Transform
        private ThrowSnowball_Boss throwAbility; // 눈덩이 던지기 능력

        [Header("Refactoring - BossAttackPattern")]
        public BossAttackPattern bossAttackPattern;

        void Start()
        {
            InitThrowAbility();
            InitBossAttackPattern();
        }

        void InitThrowAbility()
        {
            throwAbility = GetComponentInChildren<ThrowSnowball_Boss>();
            Debug.Assert(throwAbility != null,
                "**ThrowSnowball_Boss** 컴포넌트가 필요합니다.");
        }

        void InitBossAttackPattern()
        {
            if (bossAttackPattern == null)
            {
                Debug.LogError("BossAttackPattern 참조가 비어있습니다.");
                return;
            }

            bossAttackPattern.InitializePattern(player, throwAbility);
        }

        // 나중에 UpdateEntity() 구조로 옮기고 싶으면 이 함수만 바꿔 쓰면 됨
        void Update()
        {
            if (bossAttackPattern == null) return;
            bossAttackPattern.UpdatePattern();
        }
    }
}