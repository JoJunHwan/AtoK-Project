using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class StoryScene_UiController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI storyText;
    [SerializeField] private Button nextSceneButton;

    [Header("Dialogue Settings")] 
    [SerializeField] private float displayLineDuration = 0.1f;

    private string pureConfirmedText = "";
    
    private string[] dialogueLine = new string[]
    {
        "어린 시절, 동네 친구들과 눈싸움 할때마다",
        "맞고 도망만 다닌 불쌍한 플레이어는",
        "억울하고 화나고 분노가 쌓이고...",
        "", 
        "그는 이후로 꾸준히 공 던지기 연습을 해서",
        "엄청난 야구부 투수가 되어버리는데!...",
        "어느 겨울철, 플레이어는 고향에 방문해",
        "눈앞에 펼쳐진 눈밭을 보며 또 화병이 도진다,,",
        "", 
        "이날 밤, 잠에 든 플레이어는 눈 떠보니",
        "그 시절 새하얀 눈밭이 펼쳐지고",
        "친구들이 이번에도 맞기만 할거냐고",
        "조롱하는 상황이 펼쳐지는데,,,"
    };

    private int currentLineIndex = 0;
    private bool isDisplaying = false; //현재 줄 출력 중?
    
    
    void Start()
    {
        storyText.text = ""; //텍스트 초기화
        nextSceneButton.gameObject.SetActive(false); //next 버튼 비활성화

        StartCoroutine(ShowDialogueRoutine());
    }

    private IEnumerator ShowDialogueRoutine()
    {
        for (currentLineIndex = 0; currentLineIndex < dialogueLine.Length; currentLineIndex++)
        {
            if (!string.IsNullOrEmpty(dialogueLine[currentLineIndex]))
            {
                yield return StartCoroutine(DisplayCurrentLine(dialogueLine[currentLineIndex]));

                //대기시간
                yield return new WaitForSeconds(0.5f);

                
            }
            else
            {
                storyText.text += "\n";
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        DialogueFinished();
    }

    private IEnumerator DisplayCurrentLine(string line)
    {
        isDisplaying = true;
    
        int startOfNewLineIndex = storyText.text.Length;
    
        if (storyText.text.Length > 0)
        {
            storyText.text += "\n";
            startOfNewLineIndex++;
        }
        storyText.text += line;

        float timer = 0f;
        string fullText = storyText.text; 
    
        while (timer < displayLineDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / displayLineDuration);
        
            byte alphaByte = (byte)(alpha * 255f);
            string alphaHex = alphaByte.ToString("X2");

            // 투명도 태그
            string alphaTagStart = $"<alpha=#{alphaHex}>";
            
            string previousText = fullText.Substring(0, startOfNewLineIndex);
            string newLineContent = fullText.Substring(startOfNewLineIndex);

            storyText.text = previousText + alphaTagStart + newLineContent;

            yield return null;
        }
        
        isDisplaying = false;
    }
    
    private void DialogueFinished()
    {
        Debug.Log("모든 대사 출력");
        
        // 모든 대사가 끝났을 때 Next 버튼 활성화
        nextSceneButton.gameObject.SetActive(true);
        
        nextSceneButton.onClick.AddListener(OnClickNextButton);
        
    }
    
    //다음 씬..
    public void OnClickNextButton()
    {
        SceneTransitionManager.Instance.LoadNextSceneInOrder();
    }
    

}
