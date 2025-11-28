using System.Collections.Generic;
using System.Linq;
using SnowFight;
using UnityEngine;

public class GameScene_LevelController : LevelController
{
    protected CharacterController playerController;
    
    [SerializeField] protected UiController uiController;
    [SerializeField] protected SpawnController spawnController;
    [SerializeField] protected EnemyDeathCounter enemyDeathCounter;
    [SerializeField] private Health health;
    
    [SerializeField] private List<Entity> entities;
    private List<Entity> entityRemoveList = new List<Entity>();

    public override void AwakeLevel()
    {
        base.AwakeLevel();

        this.SetFields();
        this.ValidateFields();
        
        foreach (Entity currentEntity in entities)
        {
            currentEntity.AwakeByLevelController();
        }

        spawnController.InitByLevelController(playerController);
        uiController.InitByLevelController();

        enemyDeathCounter.InitByLevelController();
        //health.OnDeathEvent += CallDeadLevel;
    }
    
    public override void StartLevel()
    {
        base.StartLevel();
        
        foreach (Entity currentEntity in entities)
        {
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
            if (currentEntity == null)
            {
                //UnregisterEntity(currentEntity);
                continue;
            }
            currentEntity.UpdateByLevelController();
        }

        ApplyEntityRemovals();
    }

    private void SetFields()
    {
        playerController = GameObject.FindWithTag("Player").GetComponent<CharacterController>();
        entities = GameObject.FindObjectsOfType<Entity>().ToList();
    }
    
    protected override void ValidateFields()
    {
        base.ValidateFieldsInThisClass();
        
        Debug.Assert(playerController != null, "playerController: playerController가 비어있습니다.");
        Debug.Assert(uiController != null, "GameScene_LevelController: uiController가 비어있습니다.");
        Debug.Assert(spawnController != null, "GameScene_LevelController: spawnController가 비어있습니다.");
        Debug.Assert(enemyDeathCounter != null, "GameScene_LevelController: enemyDeathCounter가 비어있습니다.");
    }
    
    public void CallDeadLevel(DamageData damageData)
    {
        Debug.LogWarning("플레이어 사망함!!!");
        //SceneTransitionManager.Instance.LoadLevel("DeadScene");
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
}
