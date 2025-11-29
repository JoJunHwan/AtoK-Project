using UnityEngine;

public class SingletonLayer : MonoBehaviour
{
    public static SingletonLayer Instance { get; private set; }
    
    private void Awake()
    {
        EnforceSingleton();
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
}
