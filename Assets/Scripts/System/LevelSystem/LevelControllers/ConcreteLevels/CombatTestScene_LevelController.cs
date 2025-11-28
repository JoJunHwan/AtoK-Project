using UnityEngine;

public class CombatTestScene_LevelController : GameScene_LevelController
{
    public virtual void OnLevelUnloaded()
    {
        // 레벨 언로드 직전 정리
    }

    public override void StartLevel()
    {
        base.StartLevel();
        sceneBGMController.PlayByKey("BGM");
        spawnController.SpawnPlayer(); //이거 왜 안됨..?
    }
}
