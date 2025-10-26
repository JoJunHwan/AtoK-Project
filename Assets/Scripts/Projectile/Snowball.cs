using UnityEngine;
using System.Collections;

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
        
        [Header("곡선 궤적 (AnimationCurve 오프셋)")]
        [Tooltip("곡선 궤적 사용 여부 (이 값은 LaunchCurvedToDestination 사용시 자동으로 true가 됩니다)")]
        [SerializeField] private bool curveEnabled = false;

        [Tooltip("정규화 진행도 t(0~1)에 대해, 비행 진행 방향의 '오른쪽' 축으로 적용할 오프셋(월드 유닛)")]
        [SerializeField] private AnimationCurve xCurve = AnimationCurve.Linear(0f, 0f, 1f, 0f);

        [Tooltip("정규화 진행도 t(0~1)에 대해, 월드 Up(Y) 축으로 적용할 오프셋(월드 유닛)")]
        [SerializeField] private AnimationCurve yCurve = AnimationCurve.Linear(0f, 0f, 1f, 0f);

        [Tooltip("오프셋 적용 시 수평면 기준의 오른쪽 축을 계산할 때, 목표까지의 진행 방향을 수평으로 투영하여 사용")]
        [SerializeField] private bool projectForwardOnGround = true;

        private Coroutine curvedRoutine;

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

#region LaunchCurved
        /// <summary>
        /// 목표 지점까지 "직진" 진행을 기준으로,
        /// 그 진행도 t(0~1)에 대해 xCurve(오른쪽), yCurve(Up) 오프셋을 더해 곡선 비행.
        /// Rigidbody 물리는 비활성화(kinematic)하여 정확히 궤적대로 이동.
        /// </summary>
        public void LaunchCurvedToDestination(Vector3 destination, float speed, float lifeTimeSeconds)
        {
            SetExpire(lifeTimeSeconds);

            if (curvedRoutine != null)
            {
                StopCoroutine(curvedRoutine);
            }

            curveEnabled = true;

            Vector3 start = transform.position;
            float dist = Vector3.Distance(start, destination);
            if (dist <= 0.0001f)
            {
                rb.linearVelocity = Vector3.zero;
                Destroy(gameObject);
                return;
            }

            float travelTime = dist / Mathf.Max(speed, 0.0001f);
            rb.isKinematic = true;

            Vector3 forward = (destination - start).normalized;
            if (projectForwardOnGround)
            {
                forward.y = 0f;
                if (forward.sqrMagnitude > 0.000001f)
                {
                    forward.Normalize();
                }
                else
                {
                    forward = transform.forward;
                }
            }

            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            if (right.sqrMagnitude <= 0.000001f)
            {
                right = transform.right;
            }

            curvedRoutine = StartCoroutine(CurvedFlight(start, destination, travelTime, right));
        }

        private IEnumerator CurvedFlight(Vector3 start, Vector3 destination, float travelTime, Vector3 right)
        {
            float startTime = Time.time;
            float endTime = startTime + travelTime;

            while (Time.time < endTime)
            {
                if (Time.time >= lifeTime)
                {
                    break;
                }

                float t = (Time.time - startTime) / travelTime;
                if (t < 0f)
                {
                    t = 0f;
                }
                if (t > 1f)
                {
                    t = 1f;
                }

                Vector3 basePos = Vector3.Lerp(start, destination, t);
                float xOff = EvaluateCurveSafe(xCurve, t);
                float yOff = EvaluateCurveSafe(yCurve, t);

                Vector3 pos = basePos + right * xOff + Vector3.up * yOff;
                transform.position = pos;

                yield return null;
            }

            transform.position = destination;
            Destroy(gameObject);
        }

        private float EvaluateCurveSafe(AnimationCurve curve, float t)
        {
            if (curve == null)
            {
                return 0f;
            }
            return curve.Evaluate(t);
        }
        

#endregion
        
    }
}
