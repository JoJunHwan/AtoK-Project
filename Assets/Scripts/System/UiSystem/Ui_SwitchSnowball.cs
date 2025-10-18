using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Ui_SwitchSnowball : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SwitchSnowball switchSnowball;
    [SerializeField] private Transform iconContainer;
    [SerializeField] private GameObject iconPrefab; // Image + Text(이름) 프리팹

    [Header("Styles")]
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color unselectedColor = new Color(1, 1, 1, 0.35f);
    [SerializeField] private Vector3 selectedScale = Vector3.one * 1.1f;
    [SerializeField] private Vector3 unselectedScale = Vector3.one;

    private readonly List<GameObject> iconObjects = new List<GameObject>();
    private readonly List<Image> iconImages = new List<Image>();
    private readonly List<TextMeshProUGUI> iconLabels = new List<TextMeshProUGUI>();

    private void Awake()
    {
        ClearIcons();
    }

    private void Start()
    {
        TryBuildFromSwitch();
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        TryUnsubscribe();
    }

    private void TryBuildFromSwitch()
    {
        if (switchSnowball == null) return;
        BuildIcons(switchSnowball.availableSnowballs);
        UpdateVisual(switchSnowball.CurrentIndex);
    }

    private void TrySubscribe()
    {
        if (switchSnowball == null) return;
        switchSnowball.OnSnowballChanged += HandleSnowballChanged;
    }

    private void TryUnsubscribe()
    {
        if (switchSnowball == null) return;
        switchSnowball.OnSnowballChanged -= HandleSnowballChanged;
    }

    private void HandleSnowballChanged(int newIndex, SnowFight.Snowball snowball)
    {
        UpdateVisual(newIndex);
    }

    public void BuildIcons(List<SnowFight.Snowball> snowballs)
    {
        ClearIcons();
        if (snowballs == null) return;
        if (iconContainer == null) return;
        if (iconPrefab == null) return;

        for (int i = 0; i < snowballs.Count; i++)
        {
            GameObject created = Instantiate(iconPrefab, iconContainer);
            iconObjects.Add(created);

            Image image = created.GetComponentInChildren<Image>();
            TextMeshProUGUI label = created.GetComponentInChildren<TextMeshProUGUI>();

            iconImages.Add(image);
            iconLabels.Add(label);

            SetIconSprite(image, snowballs[i]);
            SetIconLabel(label, snowballs[i]);
            SetIconUnselected(created, image);
        }
    }

    private void ClearIcons()
    {
        for (int i = 0; i < iconObjects.Count; i++)
        {
            if (iconObjects[i] != null) Destroy(iconObjects[i]);
        }
        iconObjects.Clear();
        iconImages.Clear();
        iconLabels.Clear();
    }

    private void SetIconSprite(Image image, SnowFight.Snowball snowball)
    {
        if (image == null) return;
        Sprite sprite = TryGetSprite(snowball);
        image.sprite = sprite;
        image.enabled = sprite != null;
    }

    private Sprite TryGetSprite(SnowFight.Snowball snowball)
    {
        Sprite result = null;
        if (snowball == null) return result;

        SpriteRenderer sr = snowball.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) result = sr.sprite;

        return result;
    }

    private void SetIconLabel(TextMeshProUGUI label, SnowFight.Snowball snowball)
    {
        if (label == null) return;
        if (snowball == null) 
        {
            label.text = "Unknown";
            return;
        }
        label.text = snowball.name;
    }

    private void SetIconUnselected(GameObject go, Image image)
    {
        if (go != null) go.transform.localScale = unselectedScale;
        if (image != null) image.color = unselectedColor;
    }

    private void SetIconSelected(GameObject go, Image image)
    {
        if (go != null) go.transform.localScale = selectedScale;
        if (image != null) image.color = selectedColor;
    }

    private void UpdateVisual(int selectedIndex)
    {
        for (int i = 0; i < iconObjects.Count; i++)
        {
            bool isSelected = i == selectedIndex;
            if (isSelected) SetIconSelected(iconObjects[i], iconImages[i]);
            else SetIconUnselected(iconObjects[i], iconImages[i]);
        }
    }
}
