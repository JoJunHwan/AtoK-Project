using UnityEngine;

public class DeadScene_LevelController : LevelController
{
    //[Header("SceneTransition - GameScene")]
    //[SerializeField] private string gameSceneName = "GameScene";
    //[SerializeField] private int gameSceneID = 2;

    public void OnClickReStartGame()
    {
        //SceneTransitionManager.Instance.LoadScene(gameSceneName);
        SceneTransitionManager.Instance.LoadPreSceneInOrder();
    }

    public void OnClickGoMainmenu()
    {
        // TitleScene으로 이동
        SceneTransitionManager.Instance.LoadScene("TitleScene");
    }
}
