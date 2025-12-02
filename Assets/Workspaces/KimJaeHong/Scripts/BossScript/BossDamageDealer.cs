using UnityEngine;
using SnowFight; 
public class BossDamageDealer : MonoBehaviour
{
    [Header("Boss Damage Damagedata")]
    public DamageData bossDamageData;

    private void OnTriggerEnter(Collider other)
    {
        Health playerHealth = other.GetComponent<Health>();

        if (playerHealth != null)
        {
            bossDamageData.hitSource = gameObject;
            playerHealth.TakeDamage(bossDamageData);
        }
    }
}