using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class EnemyDeathCounter : MonoBehaviour
{
    private List<DamageableBase> enemyList = new List<DamageableBase>();
    [SerializeField] private GameObject enemyDeathCountingParent;
    
    public event Action<int> OnAliveEnemyCountChanged;
    
    public void InitByLevelController()
    {
        Debug.Assert(enemyDeathCountingParent!=null, "enemyDeathCountingParent가 비어있습니다");
        CacheAllEnemies();
        SubscribeToEnemyDeathEvents();
        
        OnAliveEnemyCountChanged.Invoke(this.GetAliveEnemyCount());
    }

    private void CacheAllEnemies()
    {
        Debug.Log("CacheAllEnemies");
        DamageableBase[] enemies = enemyDeathCountingParent.GetComponentsInChildren<DamageableBase>();
        
        if (enemyList != null && enemyList.Count > 0)
        {
            enemyList.Clear();
        }
        
        foreach (DamageableBase enemy in enemies)
        {
            enemyList.Add(enemy);
        }
    }

    private void SubscribeToEnemyDeathEvents()
    {
        foreach (DamageableBase enemy in enemyList)
        {
            enemy.OnDeathEvent += HandleEnemyDeath;
        }
    }

    private void HandleEnemyDeath(DamageData damageData)
    {
        Debug.Log("EDC: HandleEnemyDeath");
        Debug.Log("enemyList.count: "  + enemyList.Count);
        RemoveDeadEnemies();
        OnAliveEnemyCountChanged.Invoke(this.GetAliveEnemyCount());
        CheckIfAllEnemiesDead();
    }

    private void RemoveDeadEnemies()
    {
        enemyList.RemoveAll(e => e.IsAlive == false);
    }

    private void CheckIfAllEnemiesDead()
    {
        if (enemyList.Count == 0)
        {
            CallEndLevel();
        }
    }

    public int GetAliveEnemyCount()
    {
        return enemyList.Count;
    }

    private void CallEndLevel()
    {
        Debug.Log("EDC: Call EndLevel");
        LevelController.instance.EndLevel();
    }
}