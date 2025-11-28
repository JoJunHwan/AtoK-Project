using UnityEngine;

public abstract class SystemManager : MonoBehaviour
{
    // Init By GameManager in Order
    public abstract void InitByLevelManager();

    //protected abstract void SetupSingleton();
}
