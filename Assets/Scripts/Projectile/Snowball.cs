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
        
        private bool isActive = false; // 물리/공격의 활성 여부
        private bool canCollide = false;

        [Header("Charge Info")]
        [SerializeField] private int chargeCount = 0;
        [SerializeField] private int maxChargeCount = 3;
        
        [Header("Charge Setting")]
        [SerializeField] private float scalePerCharge = 0.15f;
        [SerializeField] private float damagePerCharge = 10.0f;
        [SerializeField] private float knockbackPowerPerCharge = 5.0f;
        
        [Header("Particle")]
        [SerializeField] private ParticleSystem impactParticle; // 눈덩이 충돌 파티클 프리팹
        [SerializeField] private ParticleSystem trailParticle;  // 눈덩이 궤적 파티클 프리팹
        [SerializeField] private ParticleController particleController;
        
        public void ActivateSnowball(bool _isActive)
        {
            this.isActive = _isActive;
            
            if (isActive == true)
            {
                rb.useGravity = true;
                this.canCollide = true;
            }
            else
            {
                rb.useGravity = false;
                this.canCollide = false;
            }
        }

        public void Init(LayerMask _destroyLayers)
        {
            destroyLayers =  _destroyLayers;
            
            rb = GetComponent<Rigidbody>();
            rb.linearVelocity = Vector3.zero;
            
            ActivateSnowball(false);
        }
        
        private void Update()
        {
            /*
            if (Time.time >= lifeTime)
            {
                //Destroy(gameObject);
                return;
            }*/
            
            TryDestroyOnArrival();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (canCollide == false) return;
            
            if (!IsInMask(other.gameObject.layer))
            {
                return;
            }

            IDamageable damageable = FindDamageable(other.gameObject);
            if (damageable != null)
            {
                damageable.TakeDamage(damageData);
            }
            
            PlayImpactParticle();
            StopTrailParticle();
            
            Destroy(gameObject);
        }
        
        // 눈덩이 크기 증가 처리
        public void ExecuteCharging()
        {
            // CanCharge
            this.chargeCount++;
            if (CanCharge() == false) { return; }
            
            this.GrowScale();
            this.GrowDamage();
            this.GrowKnockback();
        }

        private void GrowScale()
        {
            // 크기 증가
            Vector3 resizedScale = this.transform.localScale + Vector3.one * scalePerCharge;
            //if (resizedScale.x > maxScale) resizedScale = Vector3.one * maxScale;
        
            this.transform.localScale = resizedScale;
        }

        private void GrowDamage()
        {
            this.damageData.damageAmount += damagePerCharge;
        }

        private void GrowKnockback()
        {
            this.damageData.knockbackPower += knockbackPowerPerCharge;
        }

        private bool CanCharge()
        {
            if (chargeCount > maxChargeCount)
            {
                this.chargeCount = maxChargeCount;
                return false;
            }
            
            return true;
        }

        // ===== Helper =====

        private void SetExpire(float ttlSeconds)
        {
            lifeTime = Time.time + ttlSeconds;
        }

        private void TryDestroyOnArrival()
        {
            Debug.Assert(hasTarget == false,"타깃이 없음");
            
            float distance = Vector3.Distance(transform.position, targetPos);
            if (distance <= arrivalRadius)
            {
                StopTrailParticle();
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
        
        //impact particle 
        private void PlayImpactParticle()
        {
            if (impactParticle != null)
            {
                // // 현재 눈덩이 위치에 객체 생성
                // ParticleSystem impactInstance = Instantiate(
                //     impactParticle,
                //     transform.position,
                //     Quaternion.identity
                // );
                //
                // // 파티클 재생
                // impactInstance.Play();
                //
                // //최대 지속시간(Duration + Max lifetime)만큼 후에 파티클 파괴 예약
                // float maxDuration = impactParticle.main.duration + impactParticle.main.startLifetime.constantMax;
                // Destroy(impactInstance.gameObject, maxDuration);
                
                particleController.PlayImpact(impactParticle, transform.position);
            }
        }

        //궤적 파티클 play
        private void PlayTrailParticle()
        {
            if (particleController != null)
            {
                //     // 궤적 파티클 인스턴스화 및 눈덩이의 자식으로 설정 (궤적을 따라가게 함)
                //     trailInstance = Instantiate(
                //         trailParticle,
                //         Vector3.zero, // 로컬 위치 0
                //         Quaternion.identity, 
                //         this.transform
                //     );
                //     
                //     Debug.Log("playing trail particle");
                //
                //     // 위치 보정 및 재생
                //     trailInstance.transform.localPosition = Vector3.zero;
                //     trailInstance.Play();
                // 
                
                particleController.PlayTrail(trailParticle);
            }
        }

        //궤적 파티클 stop (잔상 처리 로직 포함)
        private void StopTrailParticle()
        {
            if (particleController != null)
            {
                Debug.Log("stopping trail particle, allowing fade out.");

                // // 1. 부모 분리: 눈덩이 파괴 시 파티클 인스턴스가 독립적으로 남게 함
                // trailInstance.transform.parent = null;
                //
                // // 2. 입자 방출 중단: 이미 생성된 입자(잔상)는 그대로 남김
                // trailInstance.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                //
                // // 3. 파괴 예약 시간 계산 (trailInstance의 속성을 사용해야 합니다)
                // float duration = trailInstance.main.duration; 
                // float maxLifetime = trailInstance.main.startLifetime.constantMax;
                //
                // // 4. 파괴 예약: 잔상이 완전히 사라진 후 파티클 오브젝트를 제거
                // Destroy(trailInstance.gameObject, maxLifetime + duration + 0.1f);
                //
                // // 5. 참조 끊기
                // trailInstance = null;
                
                particleController.StopTrail();
            }
        }

#region LaunchCurved
        /// <summary>
        /// 목표 지점까지 "직진" 진행을 기준으로,
        /// 그 진행도 t(0~1)에 대해 xCurve(오른쪽), yCurve(Up) 오프셋을 더해 곡선 비행.
        /// Rigidbody 물리는 비활성화(kinematic)하여 정확히 궤적대로 이동.
        /// </summary>
        public void LaunchCurvedToDestination(Vector3 destination, float speed, float lifeTimeSeconds)
        {
            PlayTrailParticle();
            
            Debug.Log("lifeTimeSeconds: " + lifeTimeSeconds);
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
            
            StopTrailParticle();
            
            Debug.Log("씨발 뭐야2");
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
