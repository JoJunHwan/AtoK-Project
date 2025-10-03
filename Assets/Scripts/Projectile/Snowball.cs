using UnityEngine;

namespace SnowFight
{
    /// <summary>
    /// 눈덩이 투사체 (도착점 고정 직구)
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Snowball : MonoBehaviour
    {
        [Header("충돌 검출할 레이어들")]
        [SerializeField] private LayerMask destroyLayers;
        [SerializeField] private string damagedTag;

        [Header("데미지 정보")]
        public DamageData damageData;

        [Header("도착 처리")]
        [SerializeField, Min(0f)] private float arrivalRadius = 0.2f;

        private Rigidbody rb;
        private float lifeTime = 0f;               // 만료 시각(Time.time 기준)
        private Vector3 targetPos;
        private bool hasTarget = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            //rb.useGravity = false;
        }

        public void Init(LayerMask _destroyLayers)
        {
            destroyLayers =  _destroyLayers;
        }

        public void SetTarget(Vector3 worldTarget)
        {
            targetPos = worldTarget;
            hasTarget = true;
        }

        // 외부에서 전달한 초기 속도의 "크기"를 사용해, 타겟 향 직선 속도로 설정
        public void Launch(Vector3 velocity, bool useCurve, float sideForce, float lifeTime)
        {
            SetExpire(lifeTime);
            if (hasTarget)
            {
                ApplyVelocityTowardTarget(velocity);
            }
            else
            {
                rb.linearVelocity = velocity;
            }
        }
        
        // 직선 발사용 오버로드: 목표 지점, 속도, 생존시간(초)
        public void LaunchToDestination(Vector3 destination, float speed, float lifeTimeSeconds)
        {
            //isCurved = false; // 직구
            rb.linearVelocity = GetDirectionTo(destination) * speed;
            this.lifeTime = Time.time + lifeTimeSeconds;
        }

        // 목표 지점까지의 단위 방향 계산 (안전 처리 포함)
        private Vector3 GetDirectionTo(Vector3 destination)
        {
            Vector3 dir = destination - transform.position;
            if (dir.sqrMagnitude <= 0.0001f)
            {
                return transform.forward;
            }
            dir.Normalize();
            return dir;
        }
        
        private void Update()
        {
            if (Time.time >= lifeTime)
            {
                Destroy(gameObject);
                return;
            }
            TryDestroyOnArrival();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsInMask(other.gameObject.layer))
            {
                return;
            }

            IDamageable damageable = FindDamageable(other.gameObject);
            if (damageable != null)
            {
                damageable.TakeDamage(damageData);
            }

            Destroy(gameObject);
        }

        // ===== Helper =====

        private void SetExpire(float ttlSeconds)
        {
            lifeTime = Time.time + ttlSeconds;
        }

        private void ApplyVelocityTowardTarget(Vector3 baseVelocity)
        {
            float speed = baseVelocity.magnitude;
            Vector3 dir = GetDirToTarget();
            rb.linearVelocity = dir * speed;
        }

        private Vector3 GetDirToTarget()
        {
            Vector3 dir = targetPos - transform.position;
            if (dir.sqrMagnitude > 0.0001f)
            {
                return dir.normalized;
            }
            return Vector3.forward; // 안전장치
        }

        private void TryDestroyOnArrival()
        {
            if (!hasTarget)
            {
                return;
            }
            float dist = Vector3.Distance(transform.position, targetPos);
            if (dist <= arrivalRadius)
            {
                Destroy(gameObject);
            }
        }

        private bool IsInMask(int layer)
        {
            int mask = 1 << layer;
            return (destroyLayers.value & mask) != 0;
        }

        private IDamageable FindDamageable(GameObject go)
        {
            IDamageable d = go.GetComponent<IDamageable>();
            if (d == null)
            {
                d = go.GetComponentInParent<IDamageable>();
                if (d == null)
                {
                    d = go.GetComponentInChildren<IDamageable>();
                }
            }
            return d;
        }
    }
}
