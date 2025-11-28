using SnowFight;
using UnityEngine;

public class GameScene_LevelController : LevelController
{
    protected CharacterController playerController;
    
    [SerializeField] protected UiController uiController;
    [SerializeField] protected SpawnController spawnController;
    [SerializeField] protected EnemyDeathCounter enemyDeathCounter;
    [SerializeField] private Health health;
    
    [SerializeField] private Entity[] entities;

    public override void AwakeLevel()
    {
        base.AwakeLevel();

        this.SetFields();
        this.ValidateFields();
        
        foreach (Entity entity in entities)
        {
            entity.AwakeByLevelController();
        }

        spawnController.InitByLevelController(playerController);
        uiController.InitByLevelController();

        enemyDeathCounter.InitByLevelController();
        //health.OnDeathEvent += CallDeadLevel;
    }
    
    public override void StartLevel()
    {
        base.StartLevel();
        
        foreach (Entity entity in entities)
        {
            entity.StartByLevelController();
        }
    }
    
    public override void UpdateLevel()
    {
        foreach (Entity entity in entities)
        {
            entity.UpdateByLevelController();
        }
    }

    private void SetFields()
    {
        playerController = GameObject.FindWithTag("Player").GetComponent<CharacterController>();
        entities = GameObject.FindObjectsOfType<Character>();
        //엔티디 담기
        //플레이어
        //Enemy
        //그외
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
}
