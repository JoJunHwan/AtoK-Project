using UnityEngine;

public abstract class LevelController : MonoBehaviour
{
    [Header("Optional Info")]
    [SerializeField] private string levelDisplayName;

    public virtual void OnLevelLoaded()
    {
        // 레벨 로드시(씬 진입 직후) 필요한 초기 세팅
    }

    public virtual void OnLevelUnloaded()
    {
        // 레벨 언로드 직전 정리
    }

    public virtual void StartLevel()
    {
        // 게임플레이 시작 시점
    }

    public virtual void PauseLevel()
    {
        // 일시정지 처리
    }

    public virtual void ResumeLevel()
    {
        // 일시정지 해제
    }

    public virtual void EndLevel()
    {
        // 클리어/실패 등 종료 처리
    }

    public string GetLevelDisplayName()
    {
        return levelDisplayName;
    }
}