using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class EnemyGroupController : MonoBehaviour
{
    [SerializeField] private GameObject enemyGroupParent;

    [SerializeField] private List<GameObject> enemyGroupList = new List<GameObject>();
    [SerializeField] private List<DamageableBase> enemyList = new List<DamageableBase>();

    private int currentGroupIndex = -1;

    public event Action<int> OnAliveEnemyCountChanged;

    public void InitByLevelController()
    {
        enemyGroupParent = GameObject.FindWithTag("EnemyGroupParent");
        Debug.Assert(enemyGroupParent != null, "enemyGroupParent가 비어있습니다");

        InitEnemyGroups();
        currentGroupIndex = -1;
        ActivateNextGroup();
    }

    private void InitEnemyGroups()
    {
        enemyGroupList.Clear();

        foreach (Transform child in enemyGroupParent.transform)
        {
            enemyGroupList.Add(child.gameObject);
        }

        foreach (GameObject enemyGroup in enemyGroupList)
        {
            enemyGroup.SetActive(false);
        }
    }

    /// <summary>
    /// 다음 EnemyGroup 활성화
    /// (만약 마지막 EnemyGroup이면, 다음레벨 호출)
    /// </summary>
    private void ActivateNextGroup()
    {
        UnsubscribeTo_EnemyDeathEvent();
        currentGroupIndex++;

        if (currentGroupIndex >= enemyGroupList.Count)
        {
            CallEndLevel();
            return;
        }

        GameObject group = enemyGroupList[currentGroupIndex];
        ActivateGroup(group);
    }

    private void ActivateGroup(GameObject enemyGroup)
    {
        enemyList.Clear();

        DamageableBase[] enemies = enemyGroup.GetComponentsInChildren<DamageableBase>();
        foreach (DamageableBase enemy in enemies)
        {
            enemyList.Add(enemy);
        }

        SubscribeTo_EnemyDeathEvents();
        enemyGroup.SetActive(true);

        Debug.Log($"EnemyGroupController: 그룹 {currentGroupIndex} 활성화, 적 수 : {enemyList.Count}");
        InvokeAliveEnemyCountChanged();
    }
    
    private void SubscribeTo_EnemyDeathEvents()
    {
        foreach (DamageableBase enemy in enemyList)
        {
            enemy.OnDeathEvent += HandleEnemyDeath;
        }
    }

    private void UnsubscribeTo_EnemyDeathEvent()
    {
        foreach (DamageableBase enemy in enemyList)
        {
            if (enemy == null)
            {
                continue;
            }

            enemy.OnDeathEvent -= HandleEnemyDeath;
        }

        enemyList.Clear();
    }

    private void HandleEnemyDeath(DamageData damageData)
    {
        Debug.Log("EnemyGroupController: HandleEnemyDeath");
        RemoveDeadEnemies();
        InvokeAliveEnemyCountChanged();
        CheckIfAllEnemiesDeadInCurrentGroup();
    }

    private void RemoveDeadEnemies()
    {
        enemyList.RemoveAll(e => e == null || e.IsAlive == false);
        Debug.Log("EnemyGroupController: enemyList.count: " + enemyList.Count);
    }

    private void CheckIfAllEnemiesDeadInCurrentGroup()
    {
        if (enemyList.Count > 0)
        {
            return;
        }

        Debug.Log($"EnemyGroupController: 그룹 {currentGroupIndex} 전멸, 다음 그룹으로 진행");
        ActivateNextGroup();
    }

    public int GetAliveEnemyCount()
    {
        return enemyList.Count;
    }

    private void InvokeAliveEnemyCountChanged()
    {
        if (OnAliveEnemyCountChanged == null)
        {
            return;
        }

        OnAliveEnemyCountChanged.Invoke(GetAliveEnemyCount());
    }

    private void CallEndLevel()
    {
        Debug.Log("EnemyGroupController: 모든 그룹 전멸 → EndLevel() 호출");
        LevelController.instance.EndLevel();
    }
}