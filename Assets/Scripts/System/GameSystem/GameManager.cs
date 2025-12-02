using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manager들의 실행순서 지정
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    //[SerializeField] private Transform managersParent;
    //private List<GameObject> allManagers = new List<GameObject>();
    
    [Header("Managers")]
    [SerializeField] private SceneTransitionManager sceneTransitionManager;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private SoundManager soundManager;
    
    private void Awake()
    {
        Debug.Log("GameManager Awake");
        EnforceSingleton();
        
        InitManagers();
    }

    private void EnforceSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }
        Destroy(gameObject);
    }
    
    private void InitManagers()
    {
        levelManager.InitByGameManager();
        sceneTransitionManager.InitByGameManager();
        soundManager.InitByGameManager();
    }
}