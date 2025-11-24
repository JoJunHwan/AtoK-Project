using UnityEngine;
using UnityEngine.UI;

public class UI_SwitchSnowball : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private SwitchSnowball switchSnowball;

    [Header("UI Texts (Index 0/1/2)")]
    [SerializeField] private Text[] uiTexts = new Text[3];

    [Header("Style")]
    [SerializeField] private Color baseColor = Color.white;
    [SerializeField] private int baseFontSize = 24;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private int highlightFontSize = 28;

    private void OnEnable()
    {
        SubscribeEvents();
        SyncImmediately();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        if (switchSnowball != null)
        {
            switchSnowball.OnSnowballChanged += OnSnowballChanged;
        }
    }

    private void UnsubscribeEvents()
    {
        if (switchSnowball != null)
        {
            switchSnowball.OnSnowballChanged -= OnSnowballChanged;
        }
    }

    private void SyncImmediately()
    {
        if (switchSnowball == null) return;
        Highlight(switchSnowball.CurrentIndex);
    }

    private void OnSnowballChanged(int newIndex, SnowFight.Snowball _)
    {
        Highlight(newIndex);
    }

    public void Highlight(int selectedIndex)
    {
        if (uiTexts == null) return;

        for (int i = 0; i < uiTexts.Length; i++)
        {
            Text t = uiTexts[i];
            if (t == null) continue;

            if (i == selectedIndex)
            {
                t.color = highlightColor;
                t.fontSize = highlightFontSize;
            }
            else
            {
                t.color = baseColor;
                t.fontSize = baseFontSize;
            }
        }
    }
}
