using UnityEngine;

public abstract class LevelController : MonoBehaviour
{
    public static LevelController instance;
    
    [Header("Optional Info")]
    [SerializeField] protected string levelDisplayName;
    
    //[Header("Next Level")]
    //[SerializeField] private int gameSceneID = 2;
    
    [Header("Level Elements")]
    [SerializeField] protected SceneBGMController sceneBGMController;

    public virtual void AwakeLevel()
    {
        instance = this;
        
        this.ValidateFieldsInThisClass();
        sceneBGMController.InitByLevelController();
    }
    
    public virtual void StartLevel()
    {
        // 게임플레이 시작 시점
    }
    
    public virtual void UpdateLevel()
    {
        
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
        Debug.Log("EndLevel");
        //SceneTransitionManager.Instance.LoadSceneByIndex(gameSceneID);
        
        // 게임 클리어시, SceneTable에 적힌 대로 다음 인덱스로 넘어감 (일방향)
        SceneTransitionManager.Instance.LoadNextSceneInOrder();
    }

    public virtual void OnLevelUnloaded()
    {
        // 레벨 언로드 직전 정리
    }
    
    public string GetLevelDisplayName()
    {
        return levelDisplayName;
    }

    protected virtual void ValidateFields()
    {
        
    }

    protected void ValidateFieldsInThisClass()
    {
        Debug.Assert(sceneBGMController != null, "sceneBGMController: sceneBGMController가 비어있습니다.");
    }
}