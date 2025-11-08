using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DamageableBase : MonoBehaviour, IDamageable
{
    [field: SerializeField] public float MaxHealth { get; set; }
    [field: SerializeField] public float CurrentHealth { get; set; }
    public bool IsAlive => isAlive;
    protected bool isAlive = true;
    
    public event Action <DamageData> OnDamagedEvent;
    public event Action <DamageData> OnDeathEvent;
    
    protected virtual void Awake()
    {
        OnDamagedEvent += HandleDamaged; // 데미지 이벤트 연결
        OnDeathEvent   += HandleDeath;   // 사망 이벤트 연결
    }
    
    public virtual void Heal(float amount)
    {
        CurrentHealth = CurrentHealth + amount;
        if (CurrentHealth > MaxHealth)
            CurrentHealth = MaxHealth;
    }

    public void FullHeal()
    {
        CurrentHealth = MaxHealth;
    }

    public virtual void TakeDamage(DamageData damageData)
    {
        CurrentHealth -= damageData.damageAmount;
        
        if (CurrentHealth <= 0)
        {
            isAlive = false;
            OnDeathEvent?.Invoke(damageData);
            return;
        }
        
        OnDamagedEvent?.Invoke(damageData);
    }
    
    public float GetHealthRatio()
    {
        float ratio = this.CurrentHealth / this.MaxHealth;

        if (ratio < 0f)
        {
            ratio = 0f;
        }
        else if (ratio > 1f)
        {
            ratio = 1f;
        }

        return ratio;
    }

    
    protected virtual void HandleDamaged(DamageData damageData) {}
    protected virtual void HandleDeath(DamageData damageData) {}
}