using UnityEngine;

/// 파티클 효과의 생명 주기 및 잔상 처리를 관리.
public class ParticleController : MonoBehaviour
{
    private ParticleSystem activeInstance;
    
    /// 파티클 프리팹을 인스턴스화하고 재생합니다. (궤적 생성 시 사용)
    public void PlayTrail(ParticleSystem prefab)
    {
        if (prefab == null) return;
        
        // 프리팹 객체화
        activeInstance = Instantiate(
            prefab,
            Vector3.zero, 
            Quaternion.identity,
            this.transform 
        );

        activeInstance.transform.localPosition = Vector3.zero;
        activeInstance.Play();
    }

    
    //
    public void StopTrail()
    {
        if (activeInstance == null) return;

        // 부모 분리 -> 궤적 생존 보장
        activeInstance.transform.parent = null;
        
        // 입자 방출 중단(기존 파티클은 남김)
        activeInstance.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        // 파티클 파괴 예약 시간
        float duration = activeInstance.main.duration; 
        float maxLifetime = activeInstance.main.startLifetime.constantMax;
        
        // 파괴(예약)
        Destroy(activeInstance.gameObject, maxLifetime + duration + 0.1f);
        
        activeInstance = null;
    }
    
    /// 1회성 파티클 재생
    public void PlayImpact(ParticleSystem impactPrefab, Vector3 position)
    {
        if (impactPrefab == null) return;

        ParticleSystem impactInstance = Instantiate(
            impactPrefab,
            position,
            Quaternion.identity
        );
        
        impactInstance.Play();
        
        float maxDuration = impactPrefab.main.duration + impactPrefab.main.startLifetime.constantMax;
        Destroy(impactInstance.gameObject, maxDuration);
    }
}