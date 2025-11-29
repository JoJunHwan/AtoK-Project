using UnityEngine;

public class TitleScene_UiController : MonoBehaviour
{
    //[Header("SceneTransition - GameScene")]
    //[SerializeField] private string gameSceneName = "GameScene";
    //[SerializeField] private int gameSceneID = 2;

    public void OnClickStartGame()
    {
        //SceneTransitionManager.Instance.LoadScene(gameSceneName);
        SceneTransitionManager.Instance.LoadNextSceneInOrder();
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