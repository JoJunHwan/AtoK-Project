using UnityEngine;
using System.Collections;

namespace SnowFight
{
    public class BossAI : MonoBehaviour
    {
        [Header("보스 기본 설정")]
        public float speed = 10f;           // 기본 이동 속도
        public float riseHeight = 10f;      // 패턴1 상승 높이
        public float groundY = 0f;          // 바닥 높이
        public float gravity = 9.8f;        // 하강 속도
        public float moveTime = 2f;         // 패턴2 이동 시간
       

        [Header("플레이어")]
        public Transform player;            // 플레이어 Transform

        private int currentPattern = 1;     // 현재 패턴 (1 또는 2)

        private Vector3 moveStartPos;       // 패턴2 이동 시작 위치
        private Vector3 moveTargetPos;      // 패턴2 이동 목표 위치
        private float moveTimer;            // 패턴2 이동 누적 시간

        [Header("아이스 플랫폼")]
        public GameObject icePlatformPrefab; // Inspector에서 할당
        public float iceDuration = 10f;      // 생존 시간


        public float pauseDuration = 1f; // 여기에 원하는 멈춤 시간 설정
        private float pauseTimer = 0f;   // 실제 멈춤 계산용

        void Update()
        {
            // 멈춤 상태 확인
            if(pauseTimer > 0f)
{
                pauseTimer -= Time.deltaTime;
                return;
            }


            if (currentPattern == 1)
            {
                Pattern1_JumpAndFall();
            }
            else if (currentPattern == 2)
            {
                Pattern2_MoveToPlayer();
            }
        }

        // ================== 패턴 1: 점프 + 공중 이동 + 하강 ==================
        private bool pattern1Ascending = true;
        private bool pattern1Falling = false;

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
                    currentPattern = 2;
                    pauseTimer = pauseDuration;

                    // ============================
                    // 땅에 닿았을 때 아이스 플랫폼 생성
                    if (icePlatformPrefab != null)
                    {
                        GameObject ice = Instantiate(icePlatformPrefab,
                                                     new Vector3(transform.position.x, groundY, transform.position.z),
                                                     Quaternion.identity);
                        Destroy(ice, iceDuration); // 10초 후 자동 제거
                    }
                    // ============================
                }
            }
        }

        // ================== 패턴 2: 플레이어 방향으로 이동 ==================
        void Pattern2_MoveToPlayer()
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
            lookDir.y = 0; // Y축 회전만
            if (lookDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookDir);

            // 이동 완료 시 패턴1로 전환
            if (t >= 1f)
            {
                moveTimer = 0f;
                currentPattern = 1;
                pauseTimer = pauseDuration;
                pattern1Ascending = true;
                pattern1Falling = false;
            }
        }
    }
}