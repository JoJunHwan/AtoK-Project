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
    
    public void OnClickStartGame()
    {
        //SceneTransitionManager.Instance.LoadScene(gameSceneName);
        SceneTransitionManager.Instance.LoadSceneByIndex(3);
    }

    public void OnClickExitGame()
    {
        // 빌드된 게임에서 종료
        Application.Quit();

#if UNITY_EDITOR
        // 에디터 환경에서는 에디터 실행 중지
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
