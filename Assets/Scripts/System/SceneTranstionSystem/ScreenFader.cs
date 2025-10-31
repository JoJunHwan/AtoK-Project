using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private Image fadeImage;

    public IEnumerator FadeOut(float duration)
    {
        yield return RunFade(0f, 1f, duration);
    }

    public IEnumerator FadeIn(float duration)
    {
        yield return RunFade(1f, 0f, duration);
    }

    private IEnumerator RunFade(float startAlpha, float endAlpha, float duration)
    {
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.unscaledDeltaTime;
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, timeElapsed / duration);
            SetAlpha(currentAlpha);
            yield return null;
        }

        SetAlpha(endAlpha);
    }

    private void SetAlpha(float alpha)
    {
        if (fadeImage == null) return;

        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }
    
    public void SetInstantBlack()
    {
        if (fadeImage == null) return;
        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;
    }

}