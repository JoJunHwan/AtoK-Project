using UnityEngine;

public class CombatTestScene_LevelController : LevelController
{
    [SerializeField] private SceneBGMController sceneBGMController;
    
    public override void OnLevelLoaded()
    {
        sceneBGMController.Init();
    }

    public virtual void OnLevelUnloaded()
    {
        // 레벨 언로드 직전 정리
    }

    public override void StartLevel()
    {
        sceneBGMController.PlayByKey("BGM");
    }

    public virtual void PauseLevel()
    {
        // 일시정지 처리
    }

    public virtual void ResumeLevel()
    {
        // 일시정지 해제
    }

    public virtual void EndLevel()
    {
        // 클리어/실패 등 종료 처리
    }
}
