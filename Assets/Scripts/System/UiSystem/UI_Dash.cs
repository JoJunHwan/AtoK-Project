using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SnowFight
{
    public class UI_Dash : MonoBehaviour
    {
        [SerializeField] private Dash dash;
        [SerializeField] private Image barImage;

        [Header("Highlight")]
        [SerializeField] private float highlightScale = 1.15f;
        [SerializeField] private float highlightDuration = 0.15f;

        private Vector3 originalScale;
        private Coroutine highlightRoutine;

        private void Start()
        {
            originalScale = barImage.transform.localScale;
            dash.OnDashStarted += ResetBar;
            dash.OnDashCooldownFinished += Highlight;
            StartCoroutine(UpdateBarLoop());
        }

        private IEnumerator UpdateBarLoop()
        {
            while (true)
            {
                float ratio = dash.GetCooldownRatio();
                barImage.fillAmount = ratio;
                yield return null;
            }
        }

        private void ResetBar()
        {
            barImage.fillAmount = 0f;
            barImage.transform.localScale = originalScale;
        }

        private void Highlight()
        {
            if (highlightRoutine != null)
            {
                StopCoroutine(highlightRoutine);
            }
            highlightRoutine = StartCoroutine(HighlightPulse());
        }

        private IEnumerator HighlightPulse()
        {
            float t = 0f;
            while (t < highlightDuration)
            {
                t += Time.deltaTime;
                float k = t / highlightDuration;
                barImage.transform.localScale = Vector3.Lerp(originalScale, originalScale * highlightScale, k);
                yield return null;
            }

            t = 0f;
            while (t < highlightDuration)
            {
                t += Time.deltaTime;
                float k = t / highlightDuration;
                barImage.transform.localScale = Vector3.Lerp(originalScale * highlightScale, originalScale, k);
                yield return null;
            }

            barImage.transform.localScale = originalScale;
            highlightRoutine = null;
        }
    }
}
