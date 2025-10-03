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
        // 이미 체력 0이하면, 피격 적용X
        if (CurrentHealth <= 0)
        {
            isAlive = false;
            return;
        }
        
        CurrentHealth -= damageData.damageAmount;
        if (CurrentHealth > 0) OnDamagedEvent?.Invoke(damageData);
        else if (CurrentHealth <= 0) OnDeathEvent?.Invoke(damageData);
    }
    
    protected virtual void HandleDamaged(DamageData damageData) {}
    protected virtual void HandleDeath(DamageData damageData) {}
}