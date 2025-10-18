using SnowFight;
using UnityEngine;

public class Health : DamageableBase
{
    private Character ownerCharacter;
    private Move moveAbility;
    //public UI_HpBar uiHpBar;

    [Header("Resistances")]
    public bool resistStagger;
    public bool resistKnockback;

    /// <summary>필요한 컴포넌트와 능력을 참조합니다.</summary>
    public void Init()
    {
        ownerCharacter = GetComponent<Character>();
        moveAbility = ownerCharacter.GetTargetAbility<Move>();
        Debug.Assert(moveAbility != null, $"{gameObject.name}의 Move 능력이 초기화되지 않았습니다.");

        //uiHpBar.ChangeSlideBarAmount(base.GetHealthRatio());
    }

    /// <summary> 데미지 처리 후 경직/넉백/UI갱신/이펙트 반응을 수행합니다.</summary>
    protected override void HandleDamaged(DamageData damageData)
    {
        //uiHpBar.ChangeSlideBarAmount(base.GetHealthRatio());
        
        if (!resistStagger) ApplyStagger();
        if (!resistKnockback) ApplyKnockback(damageData);
    }

    /// <summary>사망 처리: 플래그 비활성화 및 오브젝트 파괴.</summary>
    protected override void HandleDeath(DamageData damageData)
    {
        isAlive = false;
        Debug.Log($"{gameObject.name} 사망");
        Destroy(gameObject);
    }

    /// <summary>경직 반응(연출, FSM 전환 등은 이후 확장).</summary>
    private void ApplyStagger()
    {
        Debug.Log($"{gameObject.name} 경직 발생");
    }

    /// <summary>피격 원천을 기준으로 넉백을 계산해 Move에 전달합니다.</summary>
    private void ApplyKnockback(DamageData damageData)
    {
        if (moveAbility == null) return;
        Transform damageSourceTransform = GetDamageSourceTransform(damageData);
        if (damageSourceTransform == null) return;
        ApplyKnockbackFromSource(damageSourceTransform, damageData);
    }

    /// <summary>hitSource가 있으면 우선 사용, 없으면 attacker를 사용합니다.</summary>
    private Transform GetDamageSourceTransform(DamageData damageData)
    {
        if (damageData.hitSource != null) return damageData.hitSource.transform;
        if (damageData.attacker != null) return damageData.attacker.transform;
        return null;
    }

    /// <summary>수평면 기준(지면)으로 공격원천 → 피격자 방향을 정규화해 반환합니다.</summary>
    private Vector3 ComputeKnockbackDirectionFromSource(Transform sourceTransform)
    {
        Vector3 direction = transform.position - sourceTransform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0f) direction = direction.normalized;
        return direction;
    }

    /// <summary>넉백 파워를 0.12~0.35초 구간으로 선형 매핑합니다.</summary>
    private float ComputeKnockbackDurationFromPower(float knockbackPower)
    {
        float normalized = (knockbackPower - 2f) / 6f;
        normalized = Mathf.Clamp01(normalized);
        return Mathf.Lerp(0.12f, 2.0f, normalized);
    }

    /// <summary>계산된 방향/지속시간으로 Move의 넉백 API를 호출합니다.</summary>
    private void ApplyKnockbackFromSource(Transform sourceTransform, DamageData damageData)
    {
        Vector3 knockbackDirection = ComputeKnockbackDirectionFromSource(sourceTransform);
        float knockbackDuration = ComputeKnockbackDurationFromPower(damageData.knockbackPower);
        moveAbility.ApplyKnockback(knockbackDirection, damageData.knockbackPower, knockbackDuration, 0.2f);
    }
}
