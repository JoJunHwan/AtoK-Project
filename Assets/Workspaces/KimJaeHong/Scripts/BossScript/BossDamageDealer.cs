using UnityEngine;
using SnowFight; // DamageData와 Health 클래스를 사용하기 위해 필요

public class BossDamageDealer : MonoBehaviour
{
    // Inspector에서 보스 접촉 데미지와 넉백 파워를 설정합니다.
    [Header("보스 접촉 데미지 설정")]
    public DamageData bossDamageData;

    private void OnTriggerEnter(Collider other)
    {
        // 충돌한 오브젝트에서 Health 컴포넌트를 찾습니다.
        Health playerHealth = other.GetComponent<Health>();

        if (playerHealth != null)
        {
            // 넉백 방향 계산을 위해 공격 원천(hitSource)을 현재 오브젝트로 설정합니다.
            bossDamageData.hitSource = gameObject;

            // 2. 데미지 적용
            playerHealth.TakeDamage(bossDamageData);
        }
    }
}