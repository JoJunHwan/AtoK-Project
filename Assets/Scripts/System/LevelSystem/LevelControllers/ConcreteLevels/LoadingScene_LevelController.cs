using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScene_LevelController : LevelController
{
    [SerializeField] private float delayForNextLevel;
    
    public override void StartLevel()
    {
        base.sceneBGMController.PlayByKey("None");
        this.NextLevel_Delay();
    }

    //코루틴 n초 후에 진행
    private void NextLevel_Delay()
    {
        StartCoroutine(Delay_LoadingScene());
        
    }
    
    IEnumerator Delay_LoadingScene()
    {
        yield return new WaitForSeconds(delayForNextLevel);
        SceneTransitionManager.Instance.LoadNextSceneInOrder();
        yield break;
    }
}
