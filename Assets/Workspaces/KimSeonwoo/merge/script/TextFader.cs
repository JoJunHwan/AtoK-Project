using System;
using UnityEngine;
using TMPro; 
using System.Collections;
public class TextFader : MonoBehaviour
{
    public bool isLooping = false;
    
    public float fadeDuration = 2.0f; 
    public float minAlpha = 0.0f;   
    public float maxAlpha = 1.0f;    

    public TextMeshProUGUI tmpText;
    private Color originalColor;
    private bool fadingIn = true;

    void Awake()
    {
        originalColor = tmpText.color;
    }

    private void Start()
    {
        StartCoroutine(BlinkCoroutine());
    }

    IEnumerator BlinkCoroutine()
    {
        do 
        {
            float timer = 0f;

          
            float startAlpha = fadingIn ? minAlpha : maxAlpha;
            float endAlpha = fadingIn ? maxAlpha : minAlpha;

            while (timer < fadeDuration / 2f) 
            {
              
                float newAlpha = Mathf.Lerp(startAlpha, endAlpha, timer / (fadeDuration / 2f));
                
              
                tmpText.color = new Color(originalColor.r, originalColor.g, originalColor.b, newAlpha);

                timer += Time.deltaTime;
                yield return null; 
            }

          
            tmpText.color = new Color(originalColor.r, originalColor.g, originalColor.b, endAlpha);

            fadingIn = !fadingIn; 

            yield return null; // 잠시 대기
        }
        while(isLooping);
    }

    // 비활성화 시 코루틴 중지 및 색상 복원
    void OnDisable()
    {
        StopAllCoroutines();
        if (tmpText != null)
        {
            tmpText.color = originalColor;
        }
    }
}
