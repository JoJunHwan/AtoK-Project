using UnityEngine;

public class UiController : MonoBehaviour
{
    [SerializeField] private GameObject uiElementsParent;
    private UI_ElementBase [] uiElements;
    
    public virtual void InitByLevelController()
    {
        this.uiElements = this.FindAllUiElements();
        this.InitAllUiElements();
        
    }

    protected virtual void InitAllUiElements()
    {
        Debug.Assert(uiElementsParent  != null, "uiElementsParent가 비어있음");

        foreach (UI_ElementBase uiElement in this.uiElements)
        {
            uiElement.InitByUiController();
        }
    }

    private UI_ElementBase[] FindAllUiElements()
    {
        return uiElementsParent.GetComponentsInChildren<UI_ElementBase>();
    }
}
