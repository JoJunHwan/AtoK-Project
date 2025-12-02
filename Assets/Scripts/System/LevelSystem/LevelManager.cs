using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : SystemManager
{
    private enum LevelSetPhase
    {
        loading,
        updating,
        unloading
    }
    
    private LevelSetPhase curLevelSetPhase = LevelSetPhase.loading;
    public static LevelManager Instance { get; private set; }
    public LevelController CurrentLevelController { get; private set; }

    public override void InitByGameManager()
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
        SceneManager.sceneLoaded += HandleSceneLoaded;
        SceneManager.sceneUnloaded += HandleSceneUnloaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        curLevelSetPhase = LevelSetPhase.loading;
        
        BindLevelController();
        CallAwakeLevel();
        CallStartLevel();
        curLevelSetPhase = LevelSetPhase.updating;
    }

    private void Update()
    {
        if (curLevelSetPhase != LevelSetPhase.updating) return;
        CallUpdateLevel();
    }

    private void HandleSceneUnloaded(Scene scene)
    {
        curLevelSetPhase = LevelSetPhase.loading;
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

    private void CallAwakeLevel()
    {
        CurrentLevelController.AwakeLevel();
    }
    
    private void CallStartLevel()
    {
        CurrentLevelController.StartLevel();
    }
    
    private void CallUpdateLevel()
    {
        CurrentLevelController.UpdateLevel();
    }

    private void CallOnLevelUnloaded()
    {
        CurrentLevelController.OnLevelUnloaded();
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
