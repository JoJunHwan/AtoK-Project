using UnityEngine;

public abstract class SystemManager : MonoBehaviour
{
    // Init By GameManager in Order
    public abstract void InitByGameManager();

    //protected abstract void SetupSingleton();
}
