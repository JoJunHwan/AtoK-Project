using System.Collections.Generic;
using System.Linq;
using SnowFight;
using UnityEngine;

public class GameScene_LevelController : LevelController
{
    protected GameObject playerGameObject;
    
    [SerializeField] protected UiController uiController;
    [SerializeField] protected SpawnController spawnController;
    [SerializeField] protected EnemyGroupController enemyGroupController;
    [SerializeField] private Health playerHealth;
    
    [SerializeField] private List<Entity> entities;
    private List<Entity> entityRemoveList = new List<Entity>();

    public override void AwakeLevel()
    {
        base.AwakeLevel();

        this.SetFields();
        this.ValidateFields();
        
        foreach (Entity currentEntity in entities)
        {
            if (IsEntityActive(currentEntity) == false) continue;
            currentEntity.AwakeByLevelController();
        }

        spawnController.InitByLevelController(playerGameObject);
        uiController.InitByLevelController();

        enemyGroupController.InitByLevelController();
        
        playerHealth = playerGameObject.GetComponent<Health>();
        playerHealth.OnDeathEvent += CallDeadLevel;
    }
    
    public override void StartLevel()
    {
        base.StartLevel();
        
        foreach (Entity currentEntity in entities)
        {
            if (IsEntityActive(currentEntity) == false) continue;
            currentEntity.StartByLevelController();
        }
    }
    
    /// <summary>
    /// Update 돌면서 파괴된 것은, 지연 삭제
    /// </summary>
    public override void UpdateLevel()
    {
        foreach (Entity currentEntity in entities)
        {
            if (IsEntityActive(currentEntity) == false) continue;
            currentEntity.UpdateByLevelController();
        }

        ApplyEntityRemovals();
    }

    private bool IsEntityActive(Entity entity)
    {
        if (entity == null) return false;
        if (entity.gameObject.activeInHierarchy == false) return false;
        return true;
    }

    private void SetFields()
    {
        playerGameObject = GameObject.FindWithTag("Player");
        
        // 비활성화된 게임오브젝트도 같이 가져온다
        entities = GameObject.FindObjectsOfType<Entity>(includeInactive: true).ToList();
    }
    
    protected override void ValidateFields()
    {
        base.ValidateFieldsInThisClass();
        
        Debug.Assert(playerGameObject != null, "playerController: playerController가 비어있습니다.");
        Debug.Assert(uiController != null, "GameScene_LevelController: uiController가 비어있습니다.");
        Debug.Assert(spawnController != null, "GameScene_LevelController: spawnController가 비어있습니다.");
        Debug.Assert(enemyGroupController != null, "GameScene_LevelController: enemyDeathCounter가 비어있습니다.");
    }
    
    public void CallDeadLevel(DamageData damageData)
    {
        Debug.LogWarning("플레이어 사망함!!!");
        SceneTransitionManager.Instance.Load_DeadScene();
    }
    
    public void RegisterEntity(Entity entity)
    {
        if (entities.Contains(entity) == false)
        {
            entities.Add(entity);
        }
    }

    public void UnregisterEntity(Entity entity)
    {
        if (entities.Contains(entity) == true)
        {
            // entityRemoveList에 넣어둠
            entityRemoveList.Add(entity);
            //entities.Remove(entity);
        }
    }
    
    /// <summary>
    /// 지연 삭제
    /// (Update foreach 도는 와중에, 리스트의 원소를 제거하면 안됨. Update끝나고 제거)
    /// </summary>
    private void ApplyEntityRemovals()
    {
        for (int i = 0; i < entityRemoveList.Count; i++)
        {
            Entity entity = entityRemoveList[i];
            entities.Remove(entity);
        }

        entityRemoveList.Clear();
    }
    
    public override void EndLevel()
    {
        // 클리어/실패 등 종료 처리
        Debug.Log("EndLevel");
        //SceneTransitionManager.Instance.LoadSceneByIndex(gameSceneID);
        
        // 게임 클리어시, SceneTable에 적힌 대로 다음 인덱스로 넘어감 (일방향)
        SceneTransitionManager.Instance.LoadNextSceneInOrder();
    }
    
}
