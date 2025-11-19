using UnityEngine;
using System.Collections;
using System.Collections.Generic; // List를 사용하기 위해 추가

namespace SnowFight
{
    public class BossAI : MonoBehaviour
    {
        [Header("보스 기본 설정")]
        public float speed = 10f;       // 기본 이동 속도
        public float riseHeight = 10f;   // Pattern 1 상승 높이
        public float groundY = 0f;       // 바닥 높이
        public float gravity = 9.8f;     // 하강 속도
        public float moveTime = 2f;      // Pattern 3 이동 시간

        [Header("플레이어")]
        public Transform player;         // 플레이어 Transform ⬅️ 선언 확인

        // ===============================================
        [Header("능력 및 패턴 4 설정")]
        private ThrowSnowball_Boss throwAbility; // 던지기 능력 컴포넌트
        public int snowballCount = 5;            // 던질 눈덩이 횟수 (Pattern 4)
        public float throwInterval = 0.3f;       // 눈덩이 던지는 간격 (Pattern 4)

        [Header("아이스 플랫폼")]
        public GameObject icePlatformPrefab; // Inspector에서 할당
        public float iceDuration = 10f;      // 생존 시간

        // ➡️ [Pattern 2 설정]
        [Header("패턴 2 (눈덩이 소환/낙하) 설정")]
        public GameObject fallingSnowballPrefab; // 떨어지는 눈덩이 프리팹
        public float snowballSpawnHeight = 15f; // 눈덩이가 소환될 높이
        public float snowballFallTime = 1f;     // 눈덩이 소환 후 대기 시간
        public int fallingSnowballCount = 3;    // 떨어뜨릴 눈덩이 횟수
        public float fallingSnowballInterval = 0.5f; // 각 눈덩이 소환 간격

        // [패턴 순환 설정]
        [Header("패턴 순환 설정")]
        // Inspector에서 크기를 4로 설정하고 각 패턴의 활성화 여부를 체크합니다.
        // 0: P1, 1: P2, 2: P3, 3: P4
        public bool[] patternEnabled = new bool[4] { true, true, true, true };

        private List<int> activePatterns; // 활성화된 패턴 번호 (1, 2, 3, 4 중) 리스트
        private int currentPatternIndex = 0; // activePatterns 리스트의 현재 인덱스

        private Vector3 moveStartPos;    // Pattern 3 이동 시작 위치
        private Vector3 moveTargetPos;   // Pattern 3 이동 목표 위치
        private float moveTimer;        // Pattern 3 이동 누적 시간

        public float pauseDuration = 1f; // 멈춤 시간
        private float pauseTimer = 0f;  // 실제 멈춤 계산용

        // 패턴 1 (점프) 내부 상태
        private bool pattern1Ascending = true;
        private bool pattern1Falling = false;


        void Start()
        {
            // 던지기 능력 스크립트 참조 
            throwAbility = GetComponentInChildren<ThrowSnowball_Boss>();
            Debug.Assert(throwAbility != null, "ThrowSnowball_Boss 컴포넌트를 자식에서 찾을 수 없습니다.");

            InitializePatternOrder(); // ⬅️ 패턴 순서 초기화

            // 활성화된 첫 번째 패턴을 시작합니다.
            if (activePatterns.Count > 0)
            {
                int firstPattern = activePatterns[currentPatternIndex];

                // Pattern 2, 4는 코루틴이므로 StartCoroutine을 호출합니다.
                if (firstPattern == 2)
                {
                    StartCoroutine(Pattern2_FallingSnowball());
                }
                else if (firstPattern == 4)
                {
                    StartCoroutine(Pattern4_ThrowSnowballs());
                }
                // Pattern 1, 3은 Update에서 자동으로 호출됩니다.
            }
        }

        // 활성화된 패턴 리스트를 만듭니다.
        void InitializePatternOrder()
        {
            activePatterns = new List<int>();
            for (int i = 0; i < patternEnabled.Length; i++)
            {
                if (patternEnabled[i])
                {
                    // 패턴 번호는 1부터 시작하므로 (i + 1)을 저장
                    activePatterns.Add(i + 1);
                }
            }

            if (activePatterns.Count == 0)
            {
                Debug.LogError("활성화된 패턴이 없습니다. Boss AI를 비활성화합니다.");
                enabled = false;
                return;
            }

            // 첫 번째 활성화된 패턴으로 시작
            currentPatternIndex = 0;
        }

        // 다음 활성화된 패턴으로 전환하고 코루틴을 시작합니다.
        void AdvanceToNextPattern()
        {
            pauseTimer = pauseDuration;

            // 다음 인덱스로 이동 (리스트의 끝이면 처음으로 돌아감)
            currentPatternIndex = (currentPatternIndex + 1) % activePatterns.Count;

            // 다음 패턴 번호 (1, 2, 3, 4 중 하나)를 가져옵니다.
            int nextPattern = activePatterns[currentPatternIndex];

            // 패턴 번호에 따라 적절한 코루틴을 호출합니다.
            if (nextPattern == 2)
            {
                StartCoroutine(Pattern2_FallingSnowball());
            }
            else if (nextPattern == 4)
            {
                StartCoroutine(Pattern4_ThrowSnowballs());
            }
            // P1과 P3은 Update에서 자동으로 호출되므로 StartCoroutine은 필요 없습니다.
        }


        void Update()
        {
            //  플레이어 유효성 검사: 플레이어가 파괴되면 멈춥니다.
            if (player == null)
            {
                return;
            }

            // 멈춤 상태 확인
            if (pauseTimer > 0f)
            {
                pauseTimer -= Time.deltaTime;
                return;
            }

            // 활성화된 패턴 번호를 가져옵니다.
            if (activePatterns == null || activePatterns.Count == 0) return;
            int currentPattern = activePatterns[currentPatternIndex];


            if (currentPattern == 1) // ⬅️ Pattern 1 (점프)
            {
                Pattern1_JumpAndFall();
            }
            else if (currentPattern == 2) // ⬅️ Pattern 2 (눈덩이 소환)
            {
                // 코루틴으로 실행되므로 Update에서는 아무것도 하지 않습니다.
            }
            else if (currentPattern == 3) // ⬅️ Pattern 3 (이동)
            {
                Pattern3_MoveToPlayer();
            }
            else if (currentPattern == 4) // ⬅️ Pattern 4 (던지기)
            {
                // 코루틴으로 실행되므로 Update에서는 아무것도 하지 않습니다.
            }
        }

        // ================== Pattern 1: 점프 + 공중 이동 + 하강 ==================
        void Pattern1_JumpAndFall()
        {
            // Pattern 1 내부 Null 체크
            if (player == null)
            {
                return;
            }

            // 상승 중
            if (pattern1Ascending)
            {
                transform.position += Vector3.up * speed * Time.deltaTime;
                if (transform.position.y >= groundY + riseHeight)
                {
                    // 목표 높이 도달 → 공중에서 플레이어 xz 이동
                    // Null 체크: player.position에 접근하기 전에 다시 한 번 확인합니다.
                    if (player == null) return;

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

                    // 다음 활성화된 패턴으로 전환
                    AdvanceToNextPattern();
                }
            }
        }

        // ================== Pattern 2: 플레이어 위치에 눈덩이 소환/낙하 ==================
        IEnumerator Pattern2_FallingSnowball()
        {
            // 코루틴 시작 시 플레이어 유효성 검사
            if (player == null || fallingSnowballPrefab == null) yield break;

            for (int i = 0; i < fallingSnowballCount; i++) // 눈덩이 횟수만큼 반복
            {
                // 루프 내부 플레이어 유효성 검사 (yield return 대기 후 다시 확인)
                if (player == null) yield break;

                // 1. 소환 위치 계산 (플레이어의 XZ 위치 + 지정된 높이)
                Vector3 spawnPos = new Vector3(player.position.x,
                                               groundY + snowballSpawnHeight,
                                               player.position.z);

                // 2. 눈덩이의 최종 착지 지점(XZ)을 미리 저장
                Vector3 landingXZ = new Vector3(player.position.x, 0f, player.position.z);

                // 3. 눈덩이 소환 및 수명 설정 (LaunchToDestination 사용)
                GameObject snowballGO = Instantiate(fallingSnowballPrefab, spawnPos, Quaternion.identity);
                Snowball snowball = snowballGO.GetComponent<Snowball>();

                if (snowball != null)
                {
                    float totalLifeTime = snowballFallTime + 1.0f;
                    // LaunchCurvedToDestination 함수가 player Transform 대신 spawnPos와 totalLifeTime을 사용하여 안전합니다.
                    // 단, 이 함수가 Snowball 스크립트에 정의되어 있어야 합니다.
                    // snowball.LaunchCurvedToDestination(spawnPos, 0.001f, totalLifeTime); // 주석 처리: LaunchCurvedToDestination의 정확한 인수를 모르므로 제거
                }

                // 4. 눈덩이가 떨어질 시간만큼 대기
                yield return new WaitForSeconds(snowballFallTime);

                // 5. 눈덩이가 떨어진 위치에 아이스 플랫폼 생성
                if (icePlatformPrefab != null)
                {
                    Vector3 icePos = new Vector3(landingXZ.x, groundY, landingXZ.z);
                    GameObject ice = Instantiate(icePlatformPrefab, icePos, Quaternion.identity);
                    Destroy(ice, iceDuration);
                }

                // 6. 다음 눈덩이 소환 전 잠시 대기
                if (i < fallingSnowballCount - 1)
                {
                    yield return new WaitForSeconds(fallingSnowballInterval);
                }
            }

            // 다음 활성화된 패턴으로 전환
            AdvanceToNextPattern();
        }

        // ================== Pattern 3: 플레이어 방향으로 이동 ==================
        void Pattern3_MoveToPlayer()
        {
            // Pattern 3 내부 Null 체크
            if (player == null)
            {
                return;
            }

            //매 프레임 플레이어의 현재 XZ 위치를 목표 위치로 갱신
            moveTargetPos = new Vector3(player.position.x, transform.position.y, player.position.z);


            // moveTimer는 시간 제한을 체크하기 위해 계속 증가합니다.
            moveTimer += Time.deltaTime;

            // 목표 위치를 향해 speed로 이동
            transform.position = Vector3.MoveTowards(transform.position, moveTargetPos, speed * Time.deltaTime);

            // 플레이어 바라보기 (XZ 기준)
            Vector3 lookDir = player.position - transform.position;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookDir);

            // 이동 완료 조건: moveTime이 초과되었을 때만 다음 패턴으로 전환
            if (moveTimer >= moveTime)
            {
                moveTimer = 0f;

                // 다음 활성화된 패턴으로 전환
                AdvanceToNextPattern();
            }
        }

        // ================== Pattern 4: 눈덩이 5번 던지기 ==================
        IEnumerator Pattern4_ThrowSnowballs()
        {
            if (throwAbility == null) yield break;

            for (int i = 0; i < snowballCount; i++)
            {
                //루프 내부 Null 체크 (yield return 대기 후 다시 확인)
                if (player == null)
                {
                    yield break;
                }

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

            //다음 활성화된 패턴으로 전환
            AdvanceToNextPattern();
        }
    }
}