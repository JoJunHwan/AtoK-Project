using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : SystemManager
{
    public static SceneTransitionManager Instance;

    [SerializeField] private SceneTable sceneTable;
    [SerializeField] private ScreenFader screenFader;
    [SerializeField] private float fadeDuration = 0.5f;
    
    [Header("Debug")]
    [SerializeField] private int curSceneIndex = 0;

    private bool isLoading = false;
    
    public override void InitByLevelManager()
    {
        SetupSingleton();
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
        this.curSceneIndex++;
        this.LoadSceneByIndex(curSceneIndex);
    }
    
    public void LoadSceneByIndex(int index)
    {
        SceneInfo sceneInfo = sceneTable.GetSceneByIndex(index);
        if (sceneInfo == null) return;

        LoadScene(sceneInfo.sceneName);
    }
    
    public void LoadScene(string sceneName)
    {
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
    
}