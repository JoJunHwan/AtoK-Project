using UnityEngine;

namespace SnowFight
{
    public class BossAttackPattern_FallingSnowball : BossAttackPattern
    {
        [Header("Pattern2 - Falling Snowball")]
        [SerializeField] private GameObject fallingSnowballPrefab;
        [SerializeField] private float snowballSpawnHeight = 15f;
        [SerializeField] private float snowballFallTime = 1f;
        [SerializeField] private int fallingSnowballCount = 3;
        [SerializeField] private float fallingSnowballInterval = 0.5f;

        [Header("아이스 플랫폼")]
        [SerializeField] private GameObject icePlatformPrefab;
        [SerializeField] private float iceDuration = 10f;

        private float patternTimer;
        private int currentSpawnCount;
        private bool isWaitingForFall;

        private Vector3 currentTargetPos;

        public override void ResetPatternState()
        {
            patternTimer = 0f;
            currentSpawnCount = 0;
            isWaitingForFall = false;
            IsFinished = false;
        }

        public override void UpdatePattern()
        {
            if (player == null || fallingSnowballPrefab == null)
            {
                IsFinished = true;
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

        private void UpdateSnowballFallWait()
        {
            if (patternTimer < snowballFallTime) return;

            InstantiateIcePlatform(currentTargetPos);

            isWaitingForFall = false;
            patternTimer = 0f;
        }

        private void UpdateSnowballSpawnInterval()
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

        private void TrySpawnSnowball()
        {
            if (currentSpawnCount < fallingSnowballCount)
            {
                CalculateTargetPosition();
                SpawnSnowball();

                currentSpawnCount++;
                isWaitingForFall = true;
                patternTimer = 0f;
            }
            else
            {
                IsFinished = true;
            }
        }

        private void CalculateTargetPosition()
        {
            currentTargetPos = new Vector3(player.position.x, bossAI.GroundY, player.position.z);
        }

        private void SpawnSnowball()
        {
            Vector3 spawnPos = new Vector3(currentTargetPos.x, bossAI.GroundY + snowballSpawnHeight, currentTargetPos.z);
            Instantiate(fallingSnowballPrefab, spawnPos, Quaternion.identity);
        }

        private void InstantiateIcePlatform(Vector3 landingPos)
        {
            if (icePlatformPrefab == null) return;

            Vector3 icePos = new Vector3(landingPos.x, bossAI.GroundY, landingPos.z);

            GameObject ice = Instantiate(icePlatformPrefab, icePos, Quaternion.identity);
            Destroy(ice, iceDuration);
        }
    }
}