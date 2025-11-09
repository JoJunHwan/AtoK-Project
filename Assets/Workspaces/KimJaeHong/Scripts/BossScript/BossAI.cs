using UnityEngine;
using System.Collections;

namespace SnowFight
{
    public class BossAI : MonoBehaviour
    {
        [Header("보스 기본 설정")]
        public float speed = 10f;       // 기본 이동 속도
        public float riseHeight = 10f;   // Pattern 1 상승 높이
        public float groundY = 0f;       // 바닥 높이
        public float gravity = 9.8f;     // 하강 속도
        public float moveTime = 2f;      // Pattern 3 이동 시간

        [Header("플레이어")]
        public Transform player;         // 플레이어 Transform

        // ===============================================
        [Header("능력 및 패턴 4 설정")]
        private ThrowSnowball_Boss throwAbility; // 던지기 능력 컴포넌트
        public int snowballCount = 5;            // 던질 눈덩이 횟수 (Pattern 4)
        public float throwInterval = 0.3f;       // 눈덩이 던지는 간격 (Pattern 4)

        [Header("아이스 플랫폼")]
        public GameObject icePlatformPrefab; // Inspector에서 할당
        public float iceDuration = 10f;      // 생존 시간

        // ➡️ [Pattern 2 설정]
        [Header("패턴 2 (눈덩이 소환/낙하) 설정")]
        public GameObject fallingSnowballPrefab; // 떨어지는 눈덩이 프리팹
        public float snowballSpawnHeight = 15f; // 눈덩이가 소환될 높이
        public float snowballFallTime = 1f;     // 눈덩이 소환 후 대기 시간
        public int fallingSnowballCount = 3;    // 떨어뜨릴 눈덩이 횟수
        public float fallingSnowballInterval = 0.5f; // 각 눈덩이 소환 간격

        private int currentPattern = 1;      // 현재 패턴 (1 → 2 → 3 → 4 순환)

        private Vector3 moveStartPos;    // Pattern 3 이동 시작 위치
        private Vector3 moveTargetPos;   // Pattern 3 이동 목표 위치
        private float moveTimer;        // Pattern 3 이동 누적 시간

        public float pauseDuration = 1f; // 멈춤 시간
        private float pauseTimer = 0f;  // 실제 멈춤 계산용

        // 패턴 1 (점프) 내부 상태
        private bool pattern1Ascending = true;
        private bool pattern1Falling = false;


        void Start()
        {
            // 던지기 능력 스크립트 참조 
            throwAbility = GetComponentInChildren<ThrowSnowball_Boss>();
            Debug.Assert(throwAbility != null, "ThrowSnowball_Boss 컴포넌트를 자식에서 찾을 수 없습니다.");
        }

        void Update()
        {
            // 멈춤 상태 확인
            if (pauseTimer > 0f)
            {
                pauseTimer -= Time.deltaTime;
                return;
            }

            if (currentPattern == 1) // ⬅️ Pattern 1 (점프)
            {
                Pattern1_JumpAndFall();
            }
            else if (currentPattern == 2) // ⬅️ Pattern 2 (눈덩이 소환)
            {
                // 코루틴으로 실행
            }
            else if (currentPattern == 3) // ⬅️ Pattern 3 (이동)
            {
                Pattern3_MoveToPlayer();
            }
            else if (currentPattern == 4) // ⬅️ Pattern 4 (던지기)
            {
                // 코루틴으로 실행
            }
        }

        // ================== Pattern 1: 점프 + 공중 이동 + 하강 ==================
        void Pattern1_JumpAndFall()
        {
            // 상승 중
            if (pattern1Ascending)
            {
                transform.position += Vector3.up * speed * Time.deltaTime;
                if (transform.position.y >= groundY + riseHeight)
                {
                    // 목표 높이 도달 → 공중에서 플레이어 xz 이동
                    Vector3 targetPos = new Vector3(player.position.x, transform.position.y, player.position.z);
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * 3f * Time.deltaTime);

                    // 공중 이동이 완료되면 하강 시작
                    if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                         new Vector3(player.position.x, 0, player.position.z)) < 0.01f)
                    {
                        pattern1Ascending = false;
                        pattern1Falling = true;
                    }
                }
            }
            // 하강 중
            else if (pattern1Falling)
            {
                transform.position -= Vector3.up * gravity * Time.deltaTime;
                if (transform.position.y <= groundY)
                {
                    transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
                    pattern1Falling = false;
                    pattern1Ascending = true; // 다음 Pattern 1 시작을 위해 초기화

                    // Pattern 2로 전환 및 코루틴 시작
                    currentPattern = 2; // Pattern 2 (눈덩이 소환)로 전환
                    pauseTimer = pauseDuration;
                    StartCoroutine(Pattern2_FallingSnowball());
                }
            }
        }

        // ================== Pattern 2: 플레이어 위치에 눈덩이 소환/낙하 ==================
        IEnumerator Pattern2_FallingSnowball()
        {
            if (player == null || fallingSnowballPrefab == null) yield break;

            for (int i = 0; i < fallingSnowballCount; i++)
            {
                //소환 위치 계산 (플레이어의 XZ 위치 + 지정된 높이)
                Vector3 spawnPos = new Vector3(player.position.x,
                                               groundY + snowballSpawnHeight,
                                               player.position.z);

                // 눈덩이의 최종 착지 지점(XZ)을 미리 저장
                Vector3 landingXZ = new Vector3(player.position.x, 0f, player.position.z);

                // 눈덩이 소환 및 수명 설정 (기존 로직 유지)
                GameObject snowballGO = Instantiate(fallingSnowballPrefab, spawnPos, Quaternion.identity);
                Snowball snowball = snowballGO.GetComponent<Snowball>();

                if (snowball != null)
                {
                    float totalLifeTime = snowballFallTime + 1.0f;
                    snowball.LaunchToDestination(spawnPos, 0.001f, totalLifeTime); // 수명 설정 목적
                }

                //눈덩이가 떨어질 시간만큼 대기
                yield return new WaitForSeconds(snowballFallTime);

                //눈덩이가 떨어진 위치에 아이스 플랫폼 생성
                if (icePlatformPrefab != null)
                {
                    //눈덩이가 소환될 때 저장해 둔 착지 지점(landingXZ)으로 고정
                    Vector3 icePos = new Vector3(landingXZ.x, groundY, landingXZ.z);

                    GameObject ice = Instantiate(icePlatformPrefab, icePos, Quaternion.identity);
                    Destroy(ice, iceDuration);
                }

                //다음 눈덩이 소환 전 잠시 대기
                if (i < fallingSnowballCount - 1)
                {
                    yield return new WaitForSeconds(fallingSnowballInterval);
                }
            }

            // 6. 다음 패턴 전환
            currentPattern = 3;
            pauseTimer = pauseDuration;
        }

        // ================== Pattern 3: 플레이어 방향으로 이동 ==================
        void Pattern3_MoveToPlayer()
        {
            // 이동 시작 시 위치 초기화
            if (moveTimer <= 0f)
            {
                moveStartPos = transform.position;
                moveTargetPos = new Vector3(player.position.x, transform.position.y, player.position.z);
            }

            moveTimer += Time.deltaTime;
            float t = Mathf.Clamp01(moveTimer / moveTime);

            // XZ 이동 보간
            transform.position = Vector3.Lerp(moveStartPos, moveTargetPos, t);

            // 플레이어 바라보기 (XZ 기준)
            Vector3 lookDir = player.position - transform.position;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookDir);

            // 이동 완료 시 Pattern 4로 전환
            if (t >= 1f)
            {
                moveTimer = 0f;
                currentPattern = 4; // ⬅️ Pattern 4 (눈덩이 던지기)로 전환
                pauseTimer = pauseDuration;

                // Pattern 4 코루틴 시작
                StartCoroutine(Pattern4_ThrowSnowballs());
            }
        }

        // ================== Pattern 4: 눈덩이 5번 던지기 ==================
        IEnumerator Pattern4_ThrowSnowballs()
        {
            if (throwAbility == null) yield break;

            for (int i = 0; i < snowballCount; i++)
            {
                // 플레이어를 바라봅니다
                Vector3 lookDir = player.position - transform.position;
                lookDir.y = 0;
                if (lookDir != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(lookDir);

                // 눈덩이 던지기 능력 호출
                throwAbility.ThrowFromBoss();

                // 지정된 간격만큼 대기
                yield return new WaitForSeconds(throwInterval);
            }

            // 모든 눈덩이 던지기 완료 후 다음 패턴(Pattern 1)으로 전환
            currentPattern = 1; // Pattern 1 (점프)로 전환
            pauseTimer = pauseDuration;
        }
    }
}