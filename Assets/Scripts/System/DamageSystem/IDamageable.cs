using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
   void Heal(float amount);
   void FullHeal();
   void TakeDamage(DamageData damageData);
   //void ApplyKnockback(DamageData damageData);
}
