using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SnowFight
{
    public class BossAI : MonoBehaviour
    {
        [Header("보스 기본 설정")]
        public float speed = 10f;       // 기본 이동 속도
        public float riseHeight = 10f;  // Pattern 1 상승 높이
        public float groundY = 0f;      // 바닥 높이 (Y축 기준점)
        public float gravity = 9.8f;    // Pattern 1 하강 속도
        public float moveTime = 2f;     // Pattern 3 이동 지속 시간
        public float pauseDuration = 1f; // 패턴 전환 시 멈춤 시간

        [Header("플레이어/능력")]
        public Transform player;                 // 플레이어 Transform
        private ThrowSnowball_Boss throwAbility; // 눈덩이 던지기 능력 컴포넌트
        public int snowballCount = 5;            // Pattern 4에서 던질 눈덩이 횟수
        public float throwInterval = 0.3f;       // Pattern 4 눈덩이 던지는 간격
        
        [Header("아이스 플랫폼")]
        public GameObject icePlatformPrefab; // Pattern 2에서 생성할 플랫폼 프리팹
        public float iceDuration = 10f;     // 플랫폼 생존 시간

        [Header("패턴 2 (눈덩이 소환/낙하) 설정")]
        public GameObject fallingSnowballPrefab; // 떨어지는 눈덩이 프리팹
        public float snowballSpawnHeight = 15f; // 눈덩이가 소환될 높이
        public float snowballFallTime = 1f;     // 눈덩이가 떨어질 시간 (대기 시간)
        public int fallingSnowballCount = 3;    // 떨어뜨릴 눈덩이 횟수
        public float fallingSnowballInterval = 0.5f; // 각 눈덩이 소환 간격

        [Header("패턴 종류 활성화")]
        // 0: P1, 1: P2, 2: P3, 3: P4
        public bool[] patternEnabled = new bool[4] { true, true, true, true };
        
       
        private List<int> activePatterns; // 활성화된 패턴 번호 리스트 (1, 2, 3, 4)
        private int currentPatternIndex = 0; // activePatterns 리스트의 현재 인덱스

        // 패턴 전환/정지 상태
        private float pauseTimer = 0f; // 멈춤 시간 계산용 타이머

        // Pattern 1 상태 관리
        private bool pattern1Ascending = true; // 상승 중인가?
        private bool pattern1Falling = false;  // 하강 중인가?
        
        // Pattern 3 상태 관리
        private float moveTimer;      // 이동 시간 누적 타이머
        private Vector3 moveTargetPos; // 목표 위치

        // Pattern 2/4 타이머 및 횟수 관리 (코루틴 대체)
        private float patternTimer = 0f;         // 패턴 2/4 진행 시간
        private int currentThrowCount = 0;       // Pattern 4 현재 던진 횟수
        private int currentSpawnCount = 0;       // Pattern 2 현재 소환한 횟수
        private bool isWaitingForFall = false;   // Pattern 2: 눈덩이 낙하 대기 중
        
        void Start()
        {
            // 던지기 능력 컴포넌트 참조
            throwAbility = GetComponentInChildren<ThrowSnowball_Boss>();
            Debug.Assert(throwAbility != null, "**ThrowSnowball_Boss** 컴포넌트가 필요합니다.");
            InitializePatternOrder(); // 활성화된 패턴 목록 초기화
        }

        // 활성화된 패턴 번호 리스트를 만듭니다.
        void InitializePatternOrder()
        {
            activePatterns = new List<int>();
            for (int i = 0; i < patternEnabled.Length; i++)
            {
                if (patternEnabled[i]) activePatterns.Add(i + 1); // 패턴 번호는 1부터 시작
            }

            if (activePatterns.Count == 0)
            {
                Debug.LogError("활성화된 패턴이 없습니다. AI 비활성화.");
                enabled = false;
                return;
            }
            currentPatternIndex = 0;
            // 최초 패턴 시작 상태 초기화 (P1, P3은 Update에서 자동으로 시작됨)
            ResetPatternState(activePatterns[currentPatternIndex]);
        }
        
        // ===============================================
        // 👉 주 루프
        // ===============================================
        void Update()
        {
            if (player == null) return; // 플레이어가 없으면 작동 중지
            if (HandlePause()) return;  // 멈춤 시간 중이면 대기

            SelectPattern(); // 현재 패턴 로직 실행
        }

        // 보스가 잠시 멈추는 시간(pauseDuration)을 처리합니다.
        bool HandlePause()
        {
            if (pauseTimer > 0f)
            {
                pauseTimer -= Time.deltaTime;
                return true;
            }
            return false;
        }

        // 현재 인덱스의 패턴 번호를 확인하고 해당 로직을 호출합니다.
        void SelectPattern()
        {
            if (activePatterns.Count == 0) return;
            int pattern = activePatterns[currentPatternIndex];
            
            if (pattern == 1) Pattern1_JumpAndFall();
            else if (pattern == 2) Pattern2_FallingSnowball();
            else if (pattern == 3) Pattern3_RushToPlayer();
            else if (pattern == 4) Pattern4_ThrowSnowballs();
        }

        // ===============================================
        // 👉 패턴 전환 및 상태 리셋 (중앙 관리)
        // ===============================================
        void TransitionToNextPattern()
        {
            pauseTimer = pauseDuration; // 패턴 전환 시 멈춤 시간 설정
            
            // 다음 인덱스로 이동 (리스트 끝이면 처음으로 순환)
            currentPatternIndex = (currentPatternIndex + 1) % activePatterns.Count;
            
            // 다음 패턴의 내부 상태 초기화
            ResetPatternState(activePatterns[currentPatternIndex]);
        }
        
        // 패턴 시작 시 내부 상태를 초기화합니다.
        void ResetPatternState(int patternNumber)
        {
            patternTimer = 0f; // 공통 타이머 리셋
            moveTimer = 0f;    // P3 타이머 리셋

            if (patternNumber == 1)
            {
                // P1은 항상 상승 상태에서 시작
                pattern1Ascending = true;
                pattern1Falling = false;
            }
            else if (patternNumber == 2)
            {
                // P2 상태 리셋
                currentSpawnCount = 0;
                isWaitingForFall = false;
            }
            else if (patternNumber == 4)
            {
                // P4 상태 리셋
                currentThrowCount = 0;
            }
        }

#region Attack Pattern
        // ================== Pattern 1: 점프 + 공중 이동 + 하강 ==================
        void Pattern1_JumpAndFall()
        {
            if (pattern1Ascending) HandleAscending();
            else if (pattern1Falling) HandleFalling();
        }

        void HandleAscending()
        {
            transform.position += Vector3.up * speed * Time.deltaTime; 
            if (transform.position.y < groundY + riseHeight) return;

            Vector3 target = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.position = Vector3.MoveTowards(transform.position, target, speed * 3f * Time.deltaTime);

            Vector3 currentXZ = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 playerXZ = new Vector3(player.position.x, 0, player.position.z);

            if (Vector3.Distance(currentXZ, playerXZ) < 0.01f)
            {
                pattern1Ascending = false;
                pattern1Falling = true;
            }
        }

        void HandleFalling()
        {
            transform.position -= Vector3.up * gravity * Time.deltaTime; 
            if (transform.position.y > groundY) return;

            transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
            TransitionToNextPattern(); // 패턴 종료 후 다음 행동 결정
        }
        
        // ================== Pattern 2: 플레이어 위치에 눈덩이 소환/낙하 (Update) ==================
        void Pattern2_FallingSnowball()
        {
            if (player == null || fallingSnowballPrefab == null)
            {
                TransitionToNextPattern(); 
                return;
            }

            patternTimer += Time.deltaTime;

            if (isWaitingForFall)
            {
                // 1. 눈덩이 낙하 대기 중
                if (patternTimer >= snowballFallTime)
                {
                    // 낙하 시간 완료: 플랫폼 생성 후 다음 눈덩이 소환 준비
                    InstantiateIcePlatform(new Vector3(player.position.x, 0f, player.position.z));
                    isWaitingForFall = false;
                    patternTimer = 0f; // 인터벌 타이머로 사용하기 위해 리셋
                }
            }
            else
            {
                // 2. 눈덩이 소환 인터벌 대기 또는 첫 소환
                if (currentSpawnCount == 0 || patternTimer >= fallingSnowballInterval)
                {
                    if (currentSpawnCount < fallingSnowballCount)
                    {
                        // 소환 실행
                        SpawnSnowball();
                        currentSpawnCount++;
                        isWaitingForFall = true;
                        patternTimer = 0f; // 낙하 대기 타이머로 사용하기 위해 리셋
                    }
                    else
                    {
                        // 모든 눈덩이 소환 완료
                        TransitionToNextPattern(); // 패턴 종료 후 다음 행동 결정
                    }
                }
            }
        }

        // 눈덩이를 소환합니다.
        void SpawnSnowball()
        {
            Vector3 spawnPos = GetSnowballSpawnPos();
            Instantiate(fallingSnowballPrefab, spawnPos, Quaternion.identity);
        }

        // 눈덩이 소환 위치를 계산합니다.
        Vector3 GetSnowballSpawnPos()
        {
            return new Vector3(player.position.x, groundY + snowballSpawnHeight, player.position.z);
        }

        // 지정된 XZ 위치(바닥)에 아이스 플랫폼을 생성하고 수명을 설정합니다.
        void InstantiateIcePlatform(Vector3 landingXZ)
        {
            if (icePlatformPrefab == null) return;
            Vector3 icePos = new Vector3(landingXZ.x, groundY, landingXZ.z);
            GameObject ice = Instantiate(icePlatformPrefab, icePos, Quaternion.identity);
            Destroy(ice, iceDuration);
        }

        // ================== Pattern 3: 플레이어 방향으로 돌진/이동 ==================
        void Pattern3_RushToPlayer()
        {
            if (player == null)
            {
                TransitionToNextPattern(); 
                return;
            }

            UpdateMoveState();      // 목표 위치 및 타이머 갱신
            MoveTowardsPlayer();    // 목표를 향해 이동
            LookAtPlayerXZ();       // 플레이어 바라보기

            // 이동 시간이 초과되면 패턴 전환
            if (moveTimer >= moveTime)
            {
                TransitionToNextPattern(); // 패턴 종료 후 다음 행동 결정
            }
        }

        // 이동 목표 위치(XZ)를 갱신하고 타이머를 증가시킵니다.
        void UpdateMoveState()
        {
            moveTargetPos = new Vector3(player.position.x, transform.position.y, player.position.z);
            moveTimer += Time.deltaTime;
        }

        // 현재 목표 위치로 이동합니다.
        void MoveTowardsPlayer()
        {
            transform.position = Vector3.MoveTowards(transform.position, moveTargetPos, speed * Time.deltaTime);
        }

        // 플레이어의 XZ 위치를 기준으로 보스를 회전시켜 플레이어를 바라보게 합니다.
        void LookAtPlayerXZ()
        {
            Vector3 lookDir = player.position - transform.position;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookDir);
        }

        // ================== Pattern 4: 눈덩이 5번 던지기 (Update) ==================
        void Pattern4_ThrowSnowballs()
        {
            if (throwAbility == null || player == null)
            {
                TransitionToNextPattern(); 
                return;
            }

            patternTimer += Time.deltaTime;

            if (currentThrowCount < snowballCount)
            {
                // 던질 횟수가 남았고, 간격 시간이 충족되었을 때
                if (currentThrowCount == 0 || patternTimer >= throwInterval)
                {
                    // 던지기 실행
                    LookAtPlayerXZ(); 
                    throwAbility.ThrowFromBoss();
                    currentThrowCount++;
                    patternTimer = 0f; // 타이머 리셋
                }
            }
            else
            {
                // 모든 던지기 완료
                TransitionToNextPattern(); // 패턴 종료 후 다음 행동 결정
            }
        }
#endregion
    }
}