using UnityEngine;

namespace SnowFight
{
    /// <summary>
    /// AnimationCurve로 Y 높이를 제어하는 수동 이동 투사체
    /// </summary>
    public class SnowballCurveProjectile : MonoBehaviour
    {
        [Header("충돌 시 파괴될 레이어들")]
        [SerializeField] private LayerMask destroyLayers;
        
        [Header("Damage")]
        public float damage = 10f;                         // 피해량(후처리용)

        private Vector3 startPos;
        private Vector3 targetPos;
        private float speed;
        private AnimationCurve yCurve;
        private float yScale;
        private bool alignToVelocity;
        private float lifeTime;

        private float duration;
        private float t;
        private float deathTime;
        private bool launched;

        // 발사 파라미터를 설정
        public void Launch(Vector3 from,
                           Vector3 to,
                           float moveSpeed,
                           AnimationCurve heightCurve,
                           float heightScale,
                           bool rotateToVelocity,
                           float maxLifeTime)
        {
            startPos = from;
            targetPos = to;
            speed = moveSpeed;
            yCurve = heightCurve;
            yScale = heightScale;
            alignToVelocity = rotateToVelocity;
            lifeTime = maxLifeTime;

            float planarDist = Vector3.Distance(new Vector3(from.x, 0f, from.z), new Vector3(to.x, 0f, to.z));
            if (planarDist < 0.0001f)
            {
                planarDist = 0.0001f;
            }

            duration = planarDist / Mathf.Max(0.0001f, speed);
            t = 0f;
            deathTime = Time.time + lifeTime;
            launched = true;

            transform.position = startPos;
        }

        // 매 프레임 곡선을 따라 이동
        private void Update()
        {
            if (!launched)
            {
                return;
            }

            if (Time.time >= deathTime)
            {
                Destroy(gameObject);
                return;
            }

            if (duration <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            float dt = Time.deltaTime / duration;
            t += dt;

            if (t > 1f)
            {
                t = 1f;
            }

            Vector3 nextPos = EvaluatePosition(t);
            if (alignToVelocity)
            {
                Vector3 prevPos = transform.position;
                Vector3 vel = nextPos - prevPos;
                if (vel.sqrMagnitude > 0.0000001f)
                {
                    transform.rotation = Quaternion.LookRotation(vel.normalized, Vector3.up);
                }
            }

            transform.position = nextPos;

            if (t >= 1f)
            {
                // 목표 t=1 도달 시 생존 유지 여부는 커브에 따라 달라질 수 있음
                // 필요 시 여기서 Destroy(gameObject)를 호출해도 됨
            }
        }

        // 정규화된 진행도 t(0~1)에 대한 위치를 계산
        private Vector3 EvaluatePosition(float normalizedT)
        {
            Vector3 basePos = Vector3.Lerp(startPos, targetPos, normalizedT);
            float baseY = Mathf.Lerp(startPos.y, targetPos.y, normalizedT);
            float yOffset = 0f;

            if (yCurve != null)
            {
                yOffset = yCurve.Evaluate(normalizedT) * yScale;
            }

            basePos.y = baseY + yOffset;
            return basePos;
        }

        // 간단 충돌 처리(원하면 OnTriggerEnter로 바꾸고 Collider를 Trigger로 설정)
        private void OnCollisionEnter(Collision collision)
        {
            // 충돌한 오브젝트의 레이어
            int otherLayer = collision.gameObject.layer;

            // otherLayer가 destroyLayers에 포함되어 있는지 체크
            if ((destroyLayers.value & (1 << otherLayer)) != 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
