using UnityEngine;

public class GameScene_LevelController : LevelController
{
    protected CharacterController playerController;
    
    [SerializeField] protected UiController uiController;
    [SerializeField] protected SpawnController spawnController;
    [SerializeField] protected EnemyDeathCounter enemyDeathCounter;
    [SerializeField] private Health health;
    
    public override void OnLevelLoaded()
    {
        base.OnLevelLoaded();
        
        this.SetFields();
        this.ValidateFields();
        
        spawnController.InitByLevelController(playerController);
        uiController.InitByLevelController();
        
        enemyDeathCounter.InitByLevelController();
        health.OnDeathEvent += CallDeadLevel;
    }

    private void SetFields()
    {
        playerController = GameObject.FindWithTag("Player").GetComponent<CharacterController>();
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
