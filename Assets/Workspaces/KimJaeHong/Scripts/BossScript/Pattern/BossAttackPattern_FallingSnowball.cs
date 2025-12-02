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

        public override void ResetPatternState()
        {
            patternTimer = 0f;
            currentSpawnCount = 0;
            isWaitingForFall = false;
            IsFinished = false;
        }

        public override void UpdatePattern()
        {
            if (player == null)
            {
                IsFinished = true;
                return;
            }

            if (fallingSnowballPrefab == null)
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

            Vector3 landingPos =
                new Vector3(player.position.x, groundY, player.position.z);

            InstantiateIcePlatform(landingPos);

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

        private void SpawnSnowball()
        {
            Vector3 spawnPos = GetSnowballSpawnPos();
            Instantiate(fallingSnowballPrefab, spawnPos, Quaternion.identity);
        }

        private Vector3 GetSnowballSpawnPos()
        {
            float spawnY = groundY + snowballSpawnHeight;
            return new Vector3(player.position.x, spawnY, player.position.z);
        }

        private void InstantiateIcePlatform(Vector3 landingPos)
        {
            if (icePlatformPrefab == null) return;

            Vector3 icePos =
                new Vector3(landingPos.x, groundY, landingPos.z);

            GameObject ice =
                Instantiate(icePlatformPrefab, icePos, Quaternion.identity);

            Destroy(ice, iceDuration);
        }
    }
}
