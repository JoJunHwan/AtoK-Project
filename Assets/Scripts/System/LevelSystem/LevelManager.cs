using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : SystemManager
{
    public static LevelManager Instance { get; private set; }
    public LevelController CurrentLevelController { get; private set; }

    public override void Init()
    {
        EnforceSingleton();
        RegisterSceneCallbacks();
    }

    private void EnforceSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }
        Destroy(gameObject);
    }

    private void RegisterSceneCallbacks()
    {
        // 씬이 메모리에 로드된 직후, 실행
        SceneManager.sceneLoaded += HandleSceneLoaded;
        
        // 씬이 메모리에서 완전히 내려간 직후, 실행
        // (씬 안의 모든 GameObject들이 파괴된 직후)
        SceneManager.sceneUnloaded += HandleSceneUnloaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindLevelController();
        
        CallOnLevelLoaded();
        
        StartCurrentLevel();
    }

    private void HandleSceneUnloaded(Scene scene)
    {
        CallOnLevelUnloaded();
        
        UnbindLevelController();
    }

    private void BindLevelController()
    {
        CurrentLevelController = FindObjectOfType<LevelController>();
        Debug.Assert(CurrentLevelController != null, "[LevelManager] 이 씬에는 LevelController가 없습니다.");
    }

    private void UnbindLevelController()
    {
        CurrentLevelController = null;
    }

    private void CallOnLevelLoaded()
    {
        CurrentLevelController.OnLevelLoaded();
    }

    private void CallOnLevelUnloaded()
    {
        CurrentLevelController.OnLevelUnloaded();
    }

    // ------- 외부 제어용 API -------

    public void StartCurrentLevel()
    {
        CurrentLevelController.StartLevel();
    }

    public void PauseCurrentLevel()
    {
        CurrentLevelController.PauseLevel();
    }

    public void ResumeCurrentLevel()
    {
        CurrentLevelController.ResumeLevel();
    }

    public void EndCurrentLevel()
    {
        CurrentLevelController.EndLevel();
        //이거 애매하네
    }
}
