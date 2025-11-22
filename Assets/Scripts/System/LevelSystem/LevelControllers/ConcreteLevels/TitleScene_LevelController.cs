using UnityEngine;

public class TitleScene_LevelController : LevelController
{
    public virtual void OnLevelUnloaded()
    {
        // 레벨 언로드 직전 정리
    }

    public override void StartLevel()
    {
        base.sceneBGMController.PlayByKey("Opening");
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
