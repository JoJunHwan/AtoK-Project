using UnityEngine;
using System.Collections;

public class SplashScene_LevelController : MonoBehaviour
{
    [Header("GameStart FadeIn")]
    [SerializeField] private float fadeInDuration = 1.5f; // 페이드 인 시간
    [SerializeField] private float stayDuration = 2f;     // 로고가 유지되는 시간
    
    [Header("SceneTransition - TitleScene")]
    [SerializeField] private int titleSceneID = 1;
    
    [Header("Refrence")]
    [SerializeField] private ScreenFader screenFader;

    private void Start()
    {
        StartCoroutine(PlaySplashSequence());
    }

    private IEnumerator PlaySplashSequence()
    {
        // 1. 시작 시 화면을 완전히 어둡게
        screenFader.SetInstantBlack();

        // 2. 페이드 인 (밝아지기)
        yield return screenFader.FadeIn(fadeInDuration);

        // 3. 로고가 잠시 유지
        yield return new WaitForSeconds(stayDuration);

        // 4. 다음 씬으로 전환 (페이드 아웃 → 로드 → 페이드 인)
        SceneTransitionManager.Instance.LoadSceneByIndex(titleSceneID);
    }
}