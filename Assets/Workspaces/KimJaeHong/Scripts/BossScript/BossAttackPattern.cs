using UnityEngine;
using System.Collections.Generic;

namespace SnowFight
{
    public class BossAttackPattern : MonoBehaviour
    {
        [Header("보스 기본 설정")]
        public float speed = 10f;
        public float riseHeight = 10f;
        public float groundY = 0f;
        public float gravity = 9.8f;
        public float moveTime = 2f;
        public float pauseDuration = 1f;

        [Header("DI - By BossAI")]
        private Transform player;
        private ThrowSnowball_Boss throwAbility;
        public int snowballCount = 5;
        public float throwInterval = 0.3f;

        [Header("아이스 플랫폼")]
        public GameObject icePlatformPrefab;
        public float iceDuration = 10f;

        [Header("패턴 2 (눈덩이 소환/낙하) 설정")]
        public GameObject fallingSnowballPrefab;
        public float snowballSpawnHeight = 15f;
        public float snowballFallTime = 1f;
        public int fallingSnowballCount = 3;
        public float fallingSnowballInterval = 0.5f;

        [Header("패턴 종류 활성화")]
        // 0: P1, 1: P2, 2: P3, 3: P4
        public bool[] patternEnabled = new bool[4] { true, true, true, true };

        private List<int> activePatterns;
        private int currentPatternIndex;

        private float pauseTimer;

        // Pattern 1 상태
        private bool pattern1Ascending = true;
        private bool pattern1Falling = false;

        // Pattern 3 상태
        private float moveTimer;
        private Vector3 moveTargetPos;

        // Pattern 2/4 공용 타이머 및 카운트
        private float patternTimer;
        private int currentThrowCount;
        private int currentSpawnCount;
        private bool isWaitingForFall;

        public void InitializePattern(Transform playerTransform,
            ThrowSnowball_Boss bossThrowAbility)
        {
            player = playerTransform;
            throwAbility = bossThrowAbility;
            InitializePatternOrder();
        }

        void InitializePatternOrder()
        {
            activePatterns = new List<int>();

            for (int i = 0; i < patternEnabled.Length; i++)
            {
                if (patternEnabled[i])
                {
                    activePatterns.Add(i + 1); // 패턴 번호는 1부터
                }
            }

            if (activePatterns.Count == 0)
            {
                Debug.LogError("활성화된 패턴이 없습니다. BossAttackPattern 비활성화.");
                enabled = false;
                return;
            }

            currentPatternIndex = 0;
            ResetPatternState(activePatterns[currentPatternIndex]);
        }

        public void UpdatePattern()
        {
            if (player == null) return;
            if (HandlePause()) return;

            SelectPattern();
        }

        bool HandlePause()
        {
            if (pauseTimer <= 0f) return false;

            pauseTimer -= Time.deltaTime;
            return true;
        }

        void SelectPattern()
        {
            if (activePatterns == null) return;
            if (activePatterns.Count == 0) return;

            int pattern = activePatterns[currentPatternIndex];

            if (pattern == 1) Pattern1_JumpAndFall();
            else if (pattern == 2) Pattern2_FallingSnowball();
            else if (pattern == 3) Pattern3_RushToPlayer();
            else if (pattern == 4) Pattern4_ThrowSnowballs();
        }

        void TransitionToNextPattern()
        {
            pauseTimer = pauseDuration;

            currentPatternIndex++;
            if (currentPatternIndex >= activePatterns.Count)
            {
                currentPatternIndex = 0;
            }

            ResetPatternState(activePatterns[currentPatternIndex]);
        }

        void ResetPatternState(int patternNumber)
        {
            patternTimer = 0f;
            moveTimer = 0f;

            if (patternNumber == 1)
            {
                ResetPattern1State();
            }
            else if (patternNumber == 2)
            {
                ResetPattern2State();
            }
            else if (patternNumber == 4)
            {
                ResetPattern4State();
            }
        }

        void ResetPattern1State()
        {
            pattern1Ascending = true;
            pattern1Falling = false;
        }

        void ResetPattern2State()
        {
            currentSpawnCount = 0;
            isWaitingForFall = false;
        }

        void ResetPattern4State()
        {
            currentThrowCount = 0;
        }

        #region Pattern 1

        void Pattern1_JumpAndFall()
        {
            if (pattern1Ascending)
            {
                HandleAscending();
            }
            else if (pattern1Falling)
            {
                HandleFalling();
            }
        }

        void HandleAscending()
        {
            MoveUpwards();

            if (transform.position.y < groundY + riseHeight)
            {
                return;
            }

            MoveHorizontallyToPlayer();

            if (HasReachedPlayerXZ())
            {
                pattern1Ascending = false;
                pattern1Falling = true;
            }
        }

        void MoveUpwards()
        {
            transform.position += Vector3.up * speed * Time.deltaTime;
        }

        void MoveHorizontallyToPlayer()
        {
            Vector3 target =
                new Vector3(player.position.x, transform.position.y, player.position.z);

            transform.position =
                Vector3.MoveTowards(transform.position, target,
                    speed * 3f * Time.deltaTime);
        }

        bool HasReachedPlayerXZ()
        {
            Vector3 currentXZ =
                new Vector3(transform.position.x, 0f, transform.position.z);
            Vector3 playerXZ =
                new Vector3(player.position.x, 0f, player.position.z);

            float distance = Vector3.Distance(currentXZ, playerXZ);
            return distance < 0.01f;
        }

        void HandleFalling()
        {
            transform.position -= Vector3.up * gravity * Time.deltaTime;

            if (transform.position.y > groundY)
            {
                return;
            }

            SnapToGround();
            TransitionToNextPattern();
        }

        void SnapToGround()
        {
            Vector3 pos = transform.position;
            transform.position = new Vector3(pos.x, groundY, pos.z);
        }

        #endregion

        #region Pattern 2

        void Pattern2_FallingSnowball()
        {
            if (player == null)
            {
                TransitionToNextPattern();
                return;
            }

            if (fallingSnowballPrefab == null)
            {
                TransitionToNextPattern();
                return;
            }

            patternTimer += Time.deltaTime;

            if (isWaitingForFall)
            {
                UpdateSnowballFallWait();
            }
            else
            {
                UpdateSnowballSpawnInterval();
            }
        }

        void UpdateSnowballFallWait()
        {
            if (patternTimer < snowballFallTime)
            {
                return;
            }

            Vector3 landingPos =
                new Vector3(player.position.x, 0f, player.position.z);

            InstantiateIcePlatform(landingPos);

            isWaitingForFall = false;
            patternTimer = 0f;
        }

        void UpdateSnowballSpawnInterval()
        {
            if (currentSpawnCount == 0)
            {
                TrySpawnSnowball();
                return;
            }

            if (patternTimer >= fallingSnowballInterval)
            {
                TrySpawnSnowball();
            }
        }

        void TrySpawnSnowball()
        {
            if (currentSpawnCount < fallingSnowballCount)
            {
                SpawnSnowball();
                currentSpawnCount++;
                isWaitingForFall = true;
                patternTimer = 0f;
            }
            else
            {
                TransitionToNextPattern();
            }
        }

        void SpawnSnowball()
        {
            Vector3 spawnPos = GetSnowballSpawnPos();
            Instantiate(fallingSnowballPrefab, spawnPos, Quaternion.identity);
        }

        Vector3 GetSnowballSpawnPos()
        {
            float spawnY = groundY + snowballSpawnHeight;
            return new Vector3(player.position.x, spawnY, player.position.z);
        }

        void InstantiateIcePlatform(Vector3 landingXZ)
        {
            if (icePlatformPrefab == null) return;

            Vector3 icePos = new Vector3(landingXZ.x, groundY, landingXZ.z);
            GameObject ice =
                Instantiate(icePlatformPrefab, icePos, Quaternion.identity);

            Destroy(ice, iceDuration);
        }

        #endregion

        #region Pattern 3

        void Pattern3_RushToPlayer()
        {
            if (player == null)
            {
                TransitionToNextPattern();
                return;
            }

            UpdateMoveState();
            MoveTowardsPlayer();
            LookAtPlayerXZ();

            if (moveTimer >= moveTime)
            {
                TransitionToNextPattern();
            }
        }

        void UpdateMoveState()
        {
            moveTargetPos =
                new Vector3(player.position.x, transform.position.y, player.position.z);

            moveTimer += Time.deltaTime;
        }

        void MoveTowardsPlayer()
        {
            transform.position =
                Vector3.MoveTowards(transform.position, moveTargetPos,
                    speed * Time.deltaTime);
        }

        void LookAtPlayerXZ()
        {
            Vector3 lookDir = player.position - transform.position;
            lookDir.y = 0f;

            if (lookDir == Vector3.zero)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(lookDir);
        }

        #endregion

        #region Pattern 4

        void Pattern4_ThrowSnowballs()
        {
            if (throwAbility == null)
            {
                TransitionToNextPattern();
                return;
            }

            if (player == null)
            {
                TransitionToNextPattern();
                return;
            }

            patternTimer += Time.deltaTime;

            if (currentThrowCount < snowballCount)
            {
                TryThrowSnowball();
            }
            else
            {
                TransitionToNextPattern();
            }
        }

        void TryThrowSnowball()
        {
            if (currentThrowCount == 0)
            {
                ExecuteThrow();
                return;
            }

            if (patternTimer >= throwInterval)
            {
                ExecuteThrow();
            }
        }

        void ExecuteThrow()
        {
            LookAtPlayerXZ();
            throwAbility.ThrowFromBoss();
            currentThrowCount++;
            patternTimer = 0f;
        }

        #endregion
    }
}
