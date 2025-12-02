using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : SystemManager
{
    public static SceneTransitionManager Instance;

    [SerializeField] private SceneTable sceneTable;
    [SerializeField] private ScreenFader screenFader;
    [SerializeField] private float fadeDuration = 0.5f;
    
    [Header("Scene Settings")]
    [SerializeField] private int index_splashScene;
    [SerializeField] private int index_titleScene;
    [SerializeField] private int index_deadScene;
    [SerializeField] private int index_endScene;
    [SerializeField] private int index_firstGameScene;
    
    [Header("Debug")]
    // 0 Splash, 1 Title, 2 Dead, 3 GameLevel
    [SerializeField] private int curSceneIndex = 0;
    [SerializeField] private int preSceneIndex = 0;

    private bool isLoading = false;
    
    public override void InitByGameManager()
    {
        SetupSingleton();

        FindCurrentSceneIndex();
    }

    public void FindCurrentSceneIndex()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        int currentSceneIndex = sceneTable.GetIndexBySceneName((currentSceneName));
        SetCurSceneIndex(currentSceneIndex);
    }
    
    protected void SetupSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadNextSceneInOrder()
    {
        int newSceneIndex = this.curSceneIndex + 1;
        SetCurSceneIndex(newSceneIndex);
        this.LoadSceneByIndex(curSceneIndex);
    }
    
    public void LoadPreSceneInOrder()
    {
        //SetCurSceneIndex(preSceneIndex);
        this.LoadSceneByIndex(preSceneIndex);
    }
    
    public void LoadSceneByIndex(int index)
    {
        SceneInfo sceneInfo = sceneTable.GetSceneByIndex(index);
        if (sceneInfo == null) return;

        LoadScene(sceneInfo.sceneName);
    }
    
    public void LoadScene(string sceneName)
    {
        int newSceneIndex = sceneTable.GetIndexBySceneName(sceneName);
        Debug.Assert(newSceneIndex != -1, "존재하지 않는 씬");

        this.SetCurSceneIndex(newSceneIndex);
        if (isLoading == true) return;
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isLoading = true;

        yield return StartFadeOut();
        yield return StartLoadScene(sceneName);
        yield return StartFadeIn();

        isLoading = false;
    }
    
    
    private IEnumerator StartFadeOut()
    {
        if (screenFader != null)
        {
            yield return screenFader.FadeOut(fadeDuration);
        }
        else
        {
            yield break;
        }
    }

    private IEnumerator StartFadeIn()
    {
        if (screenFader != null)
        {
            yield return screenFader.FadeIn(fadeDuration);
        }
        else
        {
            yield break;
        }
    }

    private IEnumerator StartLoadScene(string sceneName)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
        loadOperation.allowSceneActivation = false;

        while (loadOperation.progress < 0.9f)
        {
            yield return null;
        }

        loadOperation.allowSceneActivation = true;

        while (loadOperation.isDone == false)
        {
            yield return null;
        }
    }

    public int GetCurrentSceneIndex()
    {
        return curSceneIndex;
    }

    public void SetCurSceneIndex(int _index)
    {
        preSceneIndex = this.curSceneIndex;
        this.curSceneIndex = _index;
    }
}